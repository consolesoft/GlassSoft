using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class GlassPlatesController : Controller
{
    private readonly IGlassPlateDefinitionService _service;

    public GlassPlatesController(IGlassPlateDefinitionService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var plates = await _service.GetAllAsync();
        return View(plates);
    }

    public IActionResult Create()
    {
        return View(new GlassPlateDefinitionCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GlassPlateDefinitionCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Plaka tanımı başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var plate = await _service.GetByIdAsync(id);
        if (plate == null) return NotFound();
        return View(new GlassPlateDefinitionUpdateDto
        {
            Id = plate.Id,
            Name = plate.Name,
            WidthMm = plate.WidthMm,
            HeightMm = plate.HeightMm,
            IsDefault = plate.IsDefault
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GlassPlateDefinitionUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Plaka tanımı başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Plaka tanımı başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }
}
