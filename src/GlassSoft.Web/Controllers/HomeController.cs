using System.Diagnostics;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Entities.Production;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GlassSoft.Web.Models;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Customer> _customerRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;

    public HomeController(
        IRepository<Order> orderRepo,
        IRepository<Customer> customerRepo,
        IRepository<WorkOrder> workOrderRepo)
    {
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _workOrderRepo = workOrderRepo;
    }

    public async Task<IActionResult> Index()
    {
        var activeOrders = await _orderRepo.Query()
            .Where(o => o.Status == OrderStatus.Onaylandi || o.Status == OrderStatus.Uretimde)
            .CountAsync();

        var productionCount = await _workOrderRepo.Query()
            .Where(w => !w.IsCompleted)
            .CountAsync();

        var customerCount = await _customerRepo.Query().CountAsync();

        var recentOrders = await _orderRepo.Query()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Take(8)
            .Select(o => new RecentOrderViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerTitle = o.Customer.Title,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                Status = o.Status
            })
            .ToListAsync();

        ViewBag.ActiveOrders = activeOrders;
        ViewBag.ProductionCount = productionCount;
        ViewBag.CustomerCount = customerCount;
        ViewBag.RecentOrders = recentOrders;

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode)
    {
        var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? HttpContext.Response.StatusCode,
            ErrorMessage = feature?.Error?.Message,
            ErrorDetail = feature?.Error?.ToString()
        };
        return View(model);
    }
}

public class RecentOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public OrderStatus Status { get; set; }

    public string StatusBadge => Status switch
    {
        OrderStatus.Taslak => "badge-secondary",
        OrderStatus.Onaylandi => "badge-info",
        OrderStatus.Uretimde => "badge-warning",
        OrderStatus.Tamamlandi => "badge-success",
        OrderStatus.IptalEdildi => "badge-danger",
        _ => "badge-secondary"
    };

    public string StatusText => Status switch
    {
        OrderStatus.Taslak => "Taslak",
        OrderStatus.Onaylandi => "Onaylandı",
        OrderStatus.Uretimde => "Üretimde",
        OrderStatus.Tamamlandi => "Tamamlandı",
        OrderStatus.IptalEdildi => "İptal",
        _ => Status.ToString()
    };
}
