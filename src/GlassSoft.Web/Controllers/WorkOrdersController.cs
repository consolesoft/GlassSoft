using GlassSoft.Application.DTOs.Production;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class WorkOrdersController : Controller
{
    private readonly IWorkOrderService _service;
    private readonly IOrderService _orderService;

    public WorkOrdersController(IWorkOrderService service, IOrderService orderService)
    {
        _service = service;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        var workOrders = await _service.GetAllAsync();
        return View(workOrders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var wo = await _service.GetByIdAsync(id);
        if (wo == null) return NotFound();
        return View(wo);
    }

    public async Task<IActionResult> Create()
    {
        var lines = await _service.GetUnoptimizedOrderLinesAsync();
        return View(lines);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] WorkOrderCreateFromLinesDto dto)
    {
        if (dto.SelectedOrderLineIds == null || dto.SelectedOrderLineIds.Count == 0)
        {
            TempData["Error"] = "En az bir sipariş kalemi seçmelisiniz.";
            return RedirectToAction(nameof(Create));
        }
        var wo = await _service.CreateFromLinesAsync(dto);
        TempData["Success"] = "İş emri oluşturuldu ve reçete patlatma tamamlandı.";
        return RedirectToAction(nameof(Details), new { id = wo.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetLinesByMaterial(int productItemId)
    {
        var allLines = await _service.GetUnoptimizedOrderLinesAsync();
        var filtered = allLines.Where(l => l.ProductItemId == productItemId).ToList();
        return Json(filtered.Select(l => l.OrderLineId));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunOptimization(int id)
    {
        try
        {
            var plans = await _service.RunCuttingOptimizationAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, planCount = plans.Count });

            TempData["Success"] = $"Kesim optimizasyonu tamamlandı. {plans.Count} plaka planlandı.";
        }
        catch (InvalidOperationException ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest(new { success = false, message = ex.Message });

            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCuttingPlan(int id, int planId, [FromBody] CuttingPlanUpdateDto dto)
    {
        try
        {
            await _service.UpdateCuttingPlanAsync(id, planId, dto);
            return Json(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            await _service.CompleteAsync(id);
            TempData["Success"] = "İş emri tamamlandı, stok düşüldü.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "İş emri silindi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadCsv(int id, int planId)
    {
        var wo = await _service.GetByIdAsync(id);
        var plan = wo?.CuttingPlans.FirstOrDefault(p => p.Id == planId);
        if (plan?.CsvOutput == null) return NotFound();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plan.CsvOutput);
        return File(bytes, "text/csv", $"KesimPlani_{wo!.WorkOrderNumber}_Plaka{plan.PlateIndex}.csv");
    }

    public async Task<IActionResult> DownloadDxf(int id, int planId)
    {
        var wo = await _service.GetByIdAsync(id);
        var plan = wo?.CuttingPlans.FirstOrDefault(p => p.Id == planId);
        if (plan?.DxfOutput == null) return NotFound();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plan.DxfOutput);
        return File(bytes, "application/dxf", $"KesimPlani_{wo!.WorkOrderNumber}_Plaka{plan.PlateIndex}.dxf");
    }
}
