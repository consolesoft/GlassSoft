using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _service;
    private readonly IAccountTransactionService _transactionService;
    private readonly IOrderService _orderService;
    private readonly ISystemSettingService _settingService;

    public CustomersController(
        ICustomerService service,
        IAccountTransactionService transactionService,
        IOrderService orderService,
        ISystemSettingService settingService)
    {
        _service = service;
        _transactionService = transactionService;
        _orderService = orderService;
        _settingService = settingService;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _service.GetAllAsync();
        // Her müşteri için bakiye hesapla
        var balances = new Dictionary<int, decimal>();
        foreach (var c in customers)
        {
            balances[c.Id] = await _transactionService.GetCustomerBalanceAsync(c.Id);
        }
        ViewBag.Balances = balances;
        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _service.GetByIdAsync(id);
        if (customer == null) return NotFound();

        var transactions = await _transactionService.GetByCustomerAsync(id);
        var balance = await _transactionService.GetCustomerBalanceAsync(id);

        ViewBag.Transactions = transactions;
        ViewBag.Balance = balance;
        return View(customer);
    }

    public async Task<IActionResult> Create()
    {
        var prefix = await _settingService.GetValueAsync("CustomerCodePrefix") ?? "CRI";
        var code = await _service.GenerateCodeAsync(prefix);
        return View(new CustomerCreateDto { Code = code });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.CreateAsync(dto);
        TempData["Success"] = "Müşteri başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _service.GetByIdAsync(id);
        if (customer == null) return NotFound();
        return View(new CustomerUpdateDto
        {
            Id = customer.Id,
            Code = customer.Code,
            Title = customer.Title,
            CustomerType = customer.CustomerType,
            TaxNumber = customer.TaxNumber,
            TaxOffice = customer.TaxOffice,
            Address = customer.Address,
            City = customer.City,
            Phone = customer.Phone,
            Email = customer.Email,
            Currency = customer.Currency,
            IsActive = customer.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Müşteri başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Müşteri başarıyla silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetReferences(int id)
    {
        var refs = await _service.GetReferencesAsync(id);
        return Json(new
        {
            orderCount = refs.OrderCount,
            purchaseOrderCount = refs.PurchaseOrderCount,
            accountTransactionCount = refs.AccountTransactionCount,
            chequeNoteCount = refs.ChequeNoteCount,
            cashTransactionCount = refs.CashTransactionCount,
            hasAnyReference = refs.HasAnyReference
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForceDelete(int id)
    {
        try
        {
            await _service.ForceDeleteAsync(id);
            TempData["Success"] = "Müşteri ve tüm bağlı kayıtları kalıcı olarak silindi.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Zorla silme sırasında hata oluştu: " + ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
