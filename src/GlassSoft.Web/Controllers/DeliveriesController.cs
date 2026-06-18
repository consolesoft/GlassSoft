using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class DeliveriesController : Controller
{
    private readonly IDeliveryService _deliveryService;
    private readonly IOrderService _orderService;
    private readonly IPrintTemplateService _templateService;

    public DeliveriesController(
        IDeliveryService deliveryService,
        IOrderService orderService,
        IPrintTemplateService templateService)
    {
        _deliveryService = deliveryService;
        _orderService = orderService;
        _templateService = templateService;
    }

    public async Task<IActionResult> Index()
    {
        var deliveries = await _deliveryService.GetAllAsync();
        return View(deliveries);
    }

    public async Task<IActionResult> Details(int id)
    {
        var delivery = await _deliveryService.GetByIdAsync(id);
        if (delivery == null) return NotFound();

        var templates = await _templateService.GetByOutputTypeAsync(OutputType.Teslimat);
        var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault) ?? templates.FirstOrDefault();
        ViewBag.PrintTemplateId = defaultTemplate?.Id ?? 0;

        return View(delivery);
    }

    public async Task<IActionResult> Create(int? orderId = null)
    {
        var openOrders = await _deliveryService.GetOpenOrdersAsync();
        ViewBag.OpenOrders = openOrders.ToList();

        var dto = new DeliveryCreateDto
        {
            DeliveryDate = DateTime.Today,
            OrderId = orderId ?? 0
        };

        if (orderId.HasValue && orderId.Value > 0)
        {
            var openLines = await _deliveryService.GetOpenLinesForOrderAsync(orderId.Value);
            ViewBag.OpenLines = openLines.ToList();
        }
        else
        {
            ViewBag.OpenLines = new List<OpenOrderLineDto>();
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DeliveryCreateDto dto, string? action = null)
    {
        // Boş satırları temizle
        dto.Lines = dto.Lines?.Where(l => l.Quantity > 0).ToList() ?? new List<DeliveryLineCreateDto>();

        if (dto.OrderId <= 0)
        {
            TempData["Error"] = "Sipariş seçimi zorunlu.";
            return await ReloadCreateView(dto);
        }

        if (!dto.Lines.Any())
        {
            TempData["Error"] = "En az bir kalem ve miktar girilmelidir.";
            return await ReloadCreateView(dto);
        }

        try
        {
            var created = await _deliveryService.CreateAsync(dto);
            if (action == "approve")
                await _deliveryService.ApproveAsync(created.Id);

            TempData["Success"] = action == "approve"
                ? $"Teslimat {created.DeliveryNumber} oluşturuldu ve onaylandı."
                : $"Teslimat {created.DeliveryNumber} oluşturuldu (Taslak).";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
            return await ReloadCreateView(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var delivery = await _deliveryService.GetByIdAsync(id);
        if (delivery == null) return NotFound();

        var openLines = (await _deliveryService.GetOpenLinesForOrderAsync(delivery.OrderId, excludeDeliveryId: id)).ToList();

        // Bu teslimatın mevcut satırlarını da listeye ekle (zira onlar açık miktardan çıkmış olur excludeDeliveryId ile)
        ViewBag.OpenLines = openLines;
        ViewBag.Delivery = delivery;

        var dto = new DeliveryUpdateDto
        {
            Id = delivery.Id,
            DeliveryDate = delivery.DeliveryDate,
            Notes = delivery.Notes,
            Lines = delivery.Lines.Select(l => new DeliveryLineCreateDto
            {
                OrderLineId = l.OrderLineId,
                Quantity = l.Quantity,
                Notes = l.Notes
            }).ToList()
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DeliveryUpdateDto dto)
    {
        dto.Lines = dto.Lines?.Where(l => l.Quantity > 0).ToList() ?? new List<DeliveryLineCreateDto>();

        if (!dto.Lines.Any())
        {
            TempData["Error"] = "En az bir kalem ve miktar girilmelidir.";
            return RedirectToAction(nameof(Edit), new { id = dto.Id });
        }

        try
        {
            await _deliveryService.UpdateAsync(dto);
            TempData["Success"] = "Teslimat başarıyla güncellendi.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Güncelleme hatası: {ex.Message}";
            return RedirectToAction(nameof(Edit), new { id = dto.Id });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _deliveryService.ApproveAsync(id);
            TempData["Success"] = "Teslimat onaylandı.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Onaylama hatası: {ex.Message}";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unapprove(int id)
    {
        try
        {
            await _deliveryService.UnapproveAsync(id);
            TempData["Success"] = "Teslimat onayı geri alındı (Taslağa düştü).";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _deliveryService.DeleteAsync(id);
            TempData["Success"] = "Teslimat silindi. Onaylıysa miktarlar siparişe iade edildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Silme hatası: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetOpenLines(int orderId, int? excludeDeliveryId = null)
    {
        var lines = await _deliveryService.GetOpenLinesForOrderAsync(orderId, excludeDeliveryId);
        return Json(lines);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackfillCompletedDeliveries(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction("Index", "Settings");
        }

        var allOrders = (await _orderService.GetAllAsync())
            .Where(o => o.Status == GlassSoft.Domain.Enums.OrderStatus.Tamamlandi)
            .ToList();

        int created = 0, skipped = 0, errors = 0;
        var errorList = new List<string>();

        foreach (var o in allOrders)
        {
            try
            {
                // Bu siparişin açık kalanı varsa otomatik onaylı teslimat oluştur
                var openLines = (await _deliveryService.GetOpenLinesForOrderAsync(o.Id)).ToList();
                if (!openLines.Any())
                {
                    skipped++;
                    continue;
                }

                var deliveryDate = o.CompletedAt ?? o.DeliveryDate ?? o.OrderDate;
                var dto = new GlassSoft.Application.DTOs.Sales.DeliveryCreateDto
                {
                    OrderId = o.Id,
                    DeliveryDate = deliveryDate,
                    Notes = "Sistem ayarlarından toplu oluşturuldu (tamamlanmış sipariş)",
                    Lines = openLines.Select(l => new GlassSoft.Application.DTOs.Sales.DeliveryLineCreateDto
                    {
                        OrderLineId = l.OrderLineId,
                        Quantity = l.RemainingQuantity
                    }).ToList()
                };
                var newDelivery = await _deliveryService.CreateAsync(dto);
                await _deliveryService.ApproveAsync(newDelivery.Id);
                created++;
            }
            catch (Exception ex)
            {
                errors++;
                errorList.Add($"{o.OrderNumber}: {ex.Message}");
            }
        }

        var msg = $"Tamamlanmış {allOrders.Count} sipariş kontrol edildi. " +
                  $"Oluşturulan teslimat: {created}, Atlanılan (zaten tam teslim): {skipped}";
        if (errors > 0)
            msg += $", Hata: {errors} → " + string.Join("; ", errorList.Take(3));
        TempData["Success"] = msg;
        return RedirectToAction("Index", "Settings");
    }

    private async Task<IActionResult> ReloadCreateView(DeliveryCreateDto dto)
    {
        var openOrders = await _deliveryService.GetOpenOrdersAsync();
        ViewBag.OpenOrders = openOrders.ToList();

        if (dto.OrderId > 0)
        {
            var openLines = await _deliveryService.GetOpenLinesForOrderAsync(dto.OrderId);
            ViewBag.OpenLines = openLines.ToList();
        }
        else
        {
            ViewBag.OpenLines = new List<OpenOrderLineDto>();
        }

        return View(dto);
    }
}
