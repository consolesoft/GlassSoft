using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Web.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GlassSoft.Application.Licensing;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILicenseService _licenseService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILicenseService licenseService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _licenseService = licenseService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                LastLoginAt = user.LastLoginAt
            });
        }

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        await LoadRolesAsync();
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return View(model);
        }

        var license = await _licenseService.GetStatusAsync(HttpContext.RequestAborted);
        var activeUserCount = await _userManager.Users.CountAsync(u => u.IsActive);
        if (model.IsActive && license.License != null && activeUserCount >= license.License.MaxUsers)
        {
            ModelState.AddModelError(string.Empty, $"Lisans aktif kullanıcı limiti dolu ({activeUserCount}/{license.License.MaxUsers}).");
            await LoadRolesAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            IsActive = model.IsActive,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadRolesAsync();
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.SelectedRoleId))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRoleId);
        }

        TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new UserEditViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            SelectedRoleId = userRoles.FirstOrDefault()
        };

        await LoadRolesAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null) return NotFound();

        var license = await _licenseService.GetStatusAsync(HttpContext.RequestAborted);
        if (!user.IsActive && model.IsActive && license.License != null)
        {
            var activeUserCount = await _userManager.Users.CountAsync(u => u.IsActive);
            if (activeUserCount >= license.License.MaxUsers)
            {
                ModelState.AddModelError(string.Empty, $"Lisans aktif kullanıcı limiti dolu ({activeUserCount}/{license.License.MaxUsers}).");
                await LoadRolesAsync();
                return View(model);
            }
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.IsActive = model.IsActive;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadRolesAsync();
            return View(model);
        }

        // Rol güncelleme
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!string.IsNullOrEmpty(model.SelectedRoleId))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRoleId);
        }

        TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // Soft delete: kullanıcıyı pasife al
        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Kullanıcı başarıyla devre dışı bırakıldı.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name");
    }
}
