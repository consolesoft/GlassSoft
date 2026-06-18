using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class ProductGroupsController : Controller
{
    private readonly IProductGroupService _service;

    public ProductGroupsController(IProductGroupService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _service.GetAllAsync();
        return View(groups);
    }

    public IActionResult Create()
    {
        return View(new ProductGroupCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductGroupCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Ürün grubu başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var group = await _service.GetByIdAsync(id);
        if (group == null) return NotFound();
        return View(new ProductGroupUpdateDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            IsActive = group.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductGroupUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Ürün grubu başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Ürün grubu başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }
}
