using GlassSoft.Application.DTOs.Purchasing;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderService _orderService;
    private readonly IProductItemService _productService;
    private readonly ICustomerService _customerService;

    public PurchaseOrdersController(
        IPurchaseOrderService orderService,
        IProductItemService productService,
        ICustomerService customerService)
    {
        _orderService = orderService;
        _productService = productService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectLists();
        return View(new PurchaseOrderCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] PurchaseOrderCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _orderService.CreateAsync(dto);
        TempData["Success"] = "Satın alma siparişi başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        await LoadSelectLists();
        return View(new PurchaseOrderUpdateDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            Currency = order.Currency,
            TaxRate = order.TaxRate,
            Notes = order.Notes,
            Lines = order.Lines.Select(l => new PurchaseOrderLineCreateDto
            {
                ProductItemId = l.ProductItemId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] PurchaseOrderUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _orderService.UpdateAsync(dto);
        TempData["Success"] = "Satın alma siparişi başarıyla güncellendi.";
        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, PurchaseOrderStatus status)
    {
        await _orderService.UpdateStatusAsync(id, status);
        var statusText = status switch
        {
            PurchaseOrderStatus.Onaylandi => "onaylandı (stok + cari hareket oluşturuldu)",
            PurchaseOrderStatus.KismiTeslim => "kısmi teslim olarak güncellendi",
            PurchaseOrderStatus.Tamamlandi => "tamamlandı",
            PurchaseOrderStatus.IptalEdildi => "iptal edildi",
            _ => "güncellendi"
        };
        TempData["Success"] = $"Sipariş durumu {statusText}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _orderService.DeleteAsync(id);
        TempData["Success"] = "Satın alma siparişi başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackfillTransactions(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction("Index", "Settings");
        }

        var (updated, deleted, processed, errors) = await _orderService.BackfillTransactionsAsync();

        var msg = $"Satın alma cari hareketleri senkronize edildi. " +
                  $"İşlenen sipariş: {processed}, Oluşturulan: {updated}, Silinen (eski/duplicate): {deleted}";
        if (errors.Count > 0)
            msg += $" | Hata: {errors.Count} → " + string.Join("; ", errors.Take(3));

        TempData["Success"] = msg;
        return RedirectToAction("Index", "Settings");
    }

    private async Task LoadSelectLists()
    {
        var products = await _productService.GetAllAsync();
        ViewBag.Products = new SelectList(products.Where(p => p.IsActive), "Id", "Name");

        var customers = await _customerService.GetAllAsync();
        ViewBag.Customers = new SelectList(
            customers.Where(c => c.CustomerType == CustomerType.Tedarikci || c.CustomerType == CustomerType.HerIkisi),
            "Id", "Title");
    }
}
