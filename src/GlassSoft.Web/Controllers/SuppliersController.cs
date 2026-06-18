using GlassSoft.Application.DTOs.Purchasing;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _service;

    public SuppliersController(ISupplierService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var suppliers = await _service.GetAllAsync();
        return View(suppliers);
    }

    public IActionResult Create()
    {
        return View(new SupplierCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Tedarikçi başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _service.GetByIdAsync(id);
        if (supplier == null) return NotFound();
        return View(new SupplierUpdateDto
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Title = supplier.Title,
            TaxNumber = supplier.TaxNumber,
            TaxOffice = supplier.TaxOffice,
            Address = supplier.Address,
            City = supplier.City,
            Phone = supplier.Phone,
            Email = supplier.Email,
            IsActive = supplier.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SupplierUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Tedarikçi başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Tedarikçi başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }
}
