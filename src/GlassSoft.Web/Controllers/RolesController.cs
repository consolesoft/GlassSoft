using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Web.Models.Role;
using GlassSoft.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public RolesController(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

        var model = new List<RoleListViewModel>();
        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            model.Add(new RoleListViewModel
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UserCount = usersInRole.Count
            });
        }

        return View(model);
    }

    public IActionResult Create()
    {
        return View(new RoleCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var roleExists = await _roleManager.RoleExistsAsync(model.Name);
        if (roleExists)
        {
            ModelState.AddModelError("Name", "Bu rol adı zaten mevcut.");
            return View(model);
        }

        var role = new ApplicationRole
        {
            Name = model.Name,
            Description = model.Description
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = "Rol başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var model = new RoleEditViewModel
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            SelectedPermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync()
        };

        await LoadPermissionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync(model);
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(model.Id);
        if (role == null) return NotFound();

        // Aynı isimde başka bir rol var mı kontrol et
        var existingRole = await _roleManager.FindByNameAsync(model.Name);
        if (existingRole != null && existingRole.Id != model.Id)
        {
            ModelState.AddModelError("Name", "Bu rol adı zaten mevcut.");
            await LoadPermissionsAsync(model);
            return View(model);
        }

        role.Name = model.Name;
        role.Description = model.Description;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadPermissionsAsync(model);
            return View(model);
        }

        var currentPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync();
        _context.RolePermissions.RemoveRange(currentPermissions);
        var validPermissionIds = await _context.Permissions
            .Where(p => model.SelectedPermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync();
        await _context.RolePermissions.AddRangeAsync(validPermissionIds.Select(permissionId => new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permissionId
        }));
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rol başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            TempData["Error"] = "Bu role atanmış kullanıcılar var. Önce kullanıcıların rollerini değiştirin.";
            return RedirectToAction(nameof(Index));
        }

        var rolePermissions = await _context.RolePermissions.Where(rp => rp.RoleId == role.Id).ToListAsync();
        _context.RolePermissions.RemoveRange(rolePermissions);
        await _context.SaveChangesAsync();

        await _roleManager.DeleteAsync(role);

        TempData["Success"] = "Rol başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadPermissionsAsync(RoleEditViewModel model)
    {
        var permissions = await _context.Permissions
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .ToListAsync();
        model.PermissionGroups = permissions.GroupBy(p => p.Module)
            .Select(g => new PermissionGroupViewModel
            {
                Module = g.Key,
                Permissions = g.Select(p => new PermissionOptionViewModel
                {
                    Id = p.Id,
                    Action = p.Action,
                    Description = p.Description ?? p.Code
                }).ToList()
            }).ToList();
    }
}
