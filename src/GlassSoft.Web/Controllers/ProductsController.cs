using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IProductItemService _service;
    private readonly IProductGroupService _groupService;
    private readonly ISystemSettingService _settingService;

    public ProductsController(IProductItemService service, IProductGroupService groupService, ISystemSettingService settingService)
    {
        _service = service;
        _groupService = groupService;
        _settingService = settingService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _service.GetAllAsync();
        return View(products);
    }

    public async Task<IActionResult> Create()
    {
        await LoadGroupsSelectList();
        var prefix = await _settingService.GetValueAsync("ProductCodePrefix") ?? "URN";
        var code = await _service.GenerateCodeAsync(prefix);
        return View(new ProductItemCreateDto { Code = code });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductItemCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadGroupsSelectList(dto.ProductGroupId);
            return View(dto);
        }
        await _service.CreateAsync(dto);
        TempData["Success"] = "Ürün başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null) return NotFound();
        await LoadGroupsSelectList(product.ProductGroupId);
        return View(new ProductItemUpdateDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            ProductGroupId = product.ProductGroupId,
            Unit = product.Unit,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            IsPlate = product.IsPlate,
            ThicknessMm = product.ThicknessMm
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadGroupsSelectList(dto.ProductGroupId);
            return View(dto);
        }
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Ürün başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Copy(int id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null) return NotFound();
        await LoadGroupsSelectList(product.ProductGroupId);
        return View("Create", new ProductItemCreateDto
        {
            Code = product.Code + "-KOPYA",
            Name = product.Name + " (Kopya)",
            Description = product.Description,
            ProductGroupId = product.ProductGroupId,
            Unit = product.Unit,
            UnitPrice = product.UnitPrice,
            IsActive = product.IsActive,
            IsPlate = product.IsPlate,
            ThicknessMm = product.ThicknessMm
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Ürün başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadGroupsSelectList(int? selectedId = null)
    {
        var groups = await _groupService.GetAllAsync();
        ViewBag.ProductGroups = new SelectList(groups.Where(g => g.IsActive), "Id", "Name", selectedId);

        // Birim listesi — Display(Name="...") attribute'larını kullanır
        ViewBag.Units = Enum.GetValues<UnitType>()
            .Select(u => new SelectListItem
            {
                Value = ((int)u).ToString(),
                Text = u.GetType().GetField(u.ToString())?
                       .GetCustomAttribute<DisplayAttribute>()?.Name ?? u.ToString()
            }).ToList();
    }
}
