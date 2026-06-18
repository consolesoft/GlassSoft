using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class StockController : Controller
{
    private readonly IStockService _stockService;
    private readonly IProductItemService _productService;
    private readonly IGlassPlateDefinitionService _plateService;

    public StockController(IStockService stockService, IProductItemService productService, IGlassPlateDefinitionService plateService)
    {
        _stockService = stockService;
        _productService = productService;
        _plateService = plateService;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _stockService.GetStockSummaryAsync();
        return View(summary);
    }

    public async Task<IActionResult> Movements(int? productItemId)
    {
        IEnumerable<StockEntryDto> entries;
        if (productItemId.HasValue)
        {
            entries = await _stockService.GetEntriesByProductAsync(productItemId.Value);
            var product = await _productService.GetByIdAsync(productItemId.Value);
            ViewBag.ProductName = product?.Name;

            // Güncel stok miktarı
            var stock = await _stockService.GetProductStockAsync(productItemId.Value);
            ViewBag.CurrentStockQty = stock?.TotalQuantity ?? 0m;
            ViewBag.CurrentStockPlateCount = stock?.TotalPlateCount ?? 0;
            ViewBag.StockUnit = stock?.UnitDisplay ?? "Adet";
            ViewBag.IsPlate = stock?.IsPlate ?? false;
        }
        else
        {
            entries = await _stockService.GetRecentEntriesAsync();
        }
        ViewBag.ProductItemId = productItemId;
        return View(entries);
    }

    public async Task<IActionResult> Entry()
    {
        await LoadSelectLists();
        return View(new StockEntryCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entry(StockEntryCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _stockService.CreateEntryAsync(dto);
        TempData["Success"] = "Stok hareketi başarıyla kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entry = await _stockService.GetEntryByIdAsync(id);
        if (entry == null) return NotFound();
        await LoadSelectLists();
        return View(new StockEntryCreateDto
        {
            ProductItemId = entry.ProductItemId,
            MovementType = entry.MovementType,
            Quantity = entry.Quantity,
            PlateCount = entry.PlateCount,
            GlassPlateDefinitionId = entry.GlassPlateDefinitionId,
            Description = entry.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockEntryCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _stockService.UpdateEntryAsync(id, dto);
        TempData["Success"] = "Stok hareketi güncellendi.";
        return RedirectToAction(nameof(Movements), new { productItemId = dto.ProductItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? productItemId)
    {
        await _stockService.DeleteEntryAsync(id);
        TempData["Success"] = "Stok hareketi silindi.";
        return RedirectToAction(nameof(Movements), new { productItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReverseAndDelete(int id, int? productItemId)
    {
        try
        {
            await _stockService.DeleteEntryAsync(id);
            TempData["Success"] = "Stok hareketi silindi. Stok bakiyesi otomatik güncellendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Silme hatası: {ex.Message}";
        }
        return RedirectToAction(nameof(Movements), new { productItemId });
    }

    private async Task LoadSelectLists()
    {
        var products = await _productService.GetAllAsync();
        ViewBag.Products = new SelectList(products.Where(p => p.IsActive), "Id", "Name");
        var plates = await _plateService.GetAllAsync();
        ViewBag.PlateDefinitions = new SelectList(plates, "Id", "DisplayName");
        ViewBag.MovementTypes = new SelectList(
            new[] {
                new { Value = "0", Text = "Giriş" },
                new { Value = "1", Text = "Çıkış" }
            }, "Value", "Text");
    }
}
