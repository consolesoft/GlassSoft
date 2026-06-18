using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class OutputController : Controller
{
    private readonly IOutputRenderService _renderService;
    private readonly IPrintTemplateService _templateService;
    private readonly IOrderService _orderService;

    public OutputController(IOutputRenderService renderService, IPrintTemplateService templateService, IOrderService orderService)
    {
        _renderService = renderService;
        _templateService = templateService;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplates(OutputType type)
    {
        var templates = await _templateService.GetByOutputTypeAsync(type);
        return Json(templates.Select(t => new { t.Id, t.Name, t.IsDefault }));
    }

    [HttpGet]
    public async Task<IActionResult> Render(OutputType type, int entityId, int templateId,
        DateTime? startDate, DateTime? endDate, bool autoPrint = false, bool isDelivery = false, bool priceless = false)
    {
        try
        {
            var html = type switch
            {
                OutputType.Siparis => await _renderService.RenderOrderAsync(entityId, templateId),
                OutputType.Teslimat => isDelivery
                    ? await _renderService.RenderDeliveryFromDeliveryAsync(entityId, templateId, priceless)
                    : await _renderService.RenderDeliveryAsync(entityId, templateId),
                OutputType.Uretim => await _renderService.RenderWorkOrderAsync(entityId, templateId),
                OutputType.CariHareketleri => await _renderService.RenderAccountTransactionsAsync(entityId, templateId, startDate, endDate),
                OutputType.CariEkstre => await _renderService.RenderAccountStatementAsync(entityId, templateId, startDate, endDate),
                OutputType.CitaRaporu => await _renderService.RenderCitaReportAsync(entityId, templateId),
                OutputType.FiyatsizSiparis => await _renderService.RenderPricelessOrderAsync(entityId, templateId),
                OutputType.UrunSatisRaporu => await _renderService.RenderProductSalesReportAsync(templateId, startDate ?? TurkeyTime.Now.AddMonths(-1), endDate ?? TurkeyTime.Now),
                OutputType.CariBakiyeListesi => await _renderService.RenderBalanceListAsync(templateId),
                _ => throw new ArgumentException("Geçersiz çıktı türü")
            };

            var titleMap = new Dictionary<OutputType, string>
            {
                [OutputType.Siparis] = "Sipariş Fişi",
                [OutputType.Teslimat] = "Teslimat İrsaliyesi",
                [OutputType.Uretim] = "Üretim Emri",
                [OutputType.CariHareketleri] = "Cari Hareket Raporu",
                [OutputType.CariEkstre] = "Hesap Ekstresi",
                [OutputType.CitaRaporu] = "Çıta Raporu",
                [OutputType.FiyatsizSiparis] = "Fiyatsız Sipariş Fişi",
                [OutputType.UrunSatisRaporu] = "Ürün Satış Raporu",
                [OutputType.CariBakiyeListesi] = "Cari Bakiye Listesi"
            };

            ViewData["DocumentTitle"] = titleMap.GetValueOrDefault(type, "Çıktı");
            if (autoPrint) ViewData["AutoPrint"] = true;

            return View("Render", (object)html);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> RenderLabel(int orderId, int templateId, [FromQuery] int[] lineIds, bool autoPrint = false)
    {
        try
        {
            // Tek veya çoklu seçim fark etmez: her kalem için Quantity kadar etiket basılır.
            // RenderLabelsAsync bu mantığı taşıyor.
            var html = await _renderService.RenderLabelsAsync(orderId, lineIds, templateId);

            ViewData["DocumentTitle"] = "Etiket Yazdırma";
            if (autoPrint) ViewData["AutoPrint"] = true;

            return View("Render", (object)html);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> MarkLabelsPrinted([FromBody] int[] lineIds)
    {
        if (lineIds == null || lineIds.Length == 0)
            return Json(new { success = false });

        foreach (var lineId in lineIds)
            await _orderService.SetLabelPrintedAsync(lineId);

        return Json(new { success = true });
    }
}
