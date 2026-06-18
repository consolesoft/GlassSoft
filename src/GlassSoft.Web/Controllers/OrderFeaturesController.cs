using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class OrderFeaturesController : Controller
{
    private readonly IRepository<OrderFeatureDefinition> _repository;

    public OrderFeaturesController(IRepository<OrderFeatureDefinition> repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var features = await _repository.Query().OrderBy(f => f.Name).ToListAsync();
        return View(features);
    }

    public IActionResult Create()
    {
        return View(new OrderFeatureDefinition());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderFeatureDefinition model)
    {
        if (!ModelState.IsValid) return View(model);
        await _repository.AddAsync(model);
        TempData["Success"] = "Özellik tanımı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var feature = await _repository.GetByIdAsync(id);
        if (feature == null) return NotFound();
        return View(feature);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OrderFeatureDefinition model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = await _repository.GetByIdAsync(model.Id);
        if (entity == null) return NotFound();
        entity.Name = model.Name;
        entity.UnitPrice = model.UnitPrice;
        entity.Description = model.Description;
        entity.IsActive = model.IsActive;
        await _repository.UpdateAsync(entity);
        TempData["Success"] = "Özellik tanımı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return NotFound();
        await _repository.DeleteAsync(entity);
        TempData["Success"] = "Özellik tanımı silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var features = await _repository.Query()
            .Where(f => f.IsActive)
            .Select(f => new { f.Id, f.Name, f.UnitPrice })
            .OrderBy(f => f.Name)
            .ToListAsync();
        return Json(features);
    }
}
