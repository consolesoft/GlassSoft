using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class CashRegistersController : Controller
{
    private readonly ICashRegisterService _registerService;
    private readonly ICashTransactionService _transactionService;

    public CashRegistersController(
        ICashRegisterService registerService,
        ICashTransactionService transactionService)
    {
        _registerService = registerService;
        _transactionService = transactionService;
    }

    public async Task<IActionResult> Index()
    {
        var registers = await _registerService.GetAllAsync();
        ViewBag.TotalBalance = registers.Where(r => r.Currency == "TRY").Sum(r => r.Balance);
        return View(registers);
    }

    public IActionResult Create()
    {
        return View(new CashRegisterCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CashRegisterCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _registerService.CreateAsync(dto);
        TempData["Success"] = "Kasa başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var register = await _registerService.GetByIdAsync(id);
        if (register == null) return NotFound();

        var dto = new CashRegisterUpdateDto
        {
            Id = register.Id,
            Name = register.Name,
            Currency = register.Currency,
            IsActive = register.IsActive,
            Description = register.Description
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CashRegisterUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _registerService.UpdateAsync(dto);
        TempData["Success"] = "Kasa bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _registerService.DeleteAsync(id);
        TempData["Success"] = "Kasa silindi.";
        return RedirectToAction(nameof(Index));
    }

    // Kasa Detay - İşlemler listesi (filtreleme desteği)
    public async Task<IActionResult> Details(int id, DateTime? startDate, DateTime? endDate, string? filterType)
    {
        var register = await _registerService.GetByIdAsync(id);
        if (register == null) return NotFound();

        var transactions = (await _transactionService.GetByRegisterAsync(id)).ToList();

        // Tarih filtresi
        if (startDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionDate.Date >= startDate.Value.Date).ToList();
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
        }
        if (endDate.HasValue)
        {
            transactions = transactions.Where(t => t.TransactionDate.Date <= endDate.Value.Date).ToList();
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
        }

        // Tür filtresi
        if (!string.IsNullOrEmpty(filterType))
        {
            if (filterType == "Giris")
                transactions = transactions.Where(t => t.Type == CashTransactionType.Giris).ToList();
            else if (filterType == "Cikis")
                transactions = transactions.Where(t => t.Type == CashTransactionType.Cikis).ToList();
            ViewBag.FilterType = filterType;
        }

        ViewBag.Register = register;
        return View(transactions);
    }

    // Kasa Devir İşlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CarryForward(int registerId)
    {
        var register = await _registerService.GetByIdAsync(registerId);
        if (register == null) return NotFound();

        // Dünün sonundaki bakiyeyi hesapla
        var allTransactions = await _transactionService.GetByRegisterAsync(registerId);
        var yesterday = TurkeyTime.Today.AddDays(-1);
        var balanceUntilYesterday = allTransactions
            .Where(t => t.TransactionDate.Date <= yesterday)
            .Sum(t => t.Type == CashTransactionType.Giris ? t.Amount : -t.Amount);

        if (balanceUntilYesterday == 0)
        {
            TempData["Error"] = "Devir bakiyesi 0, devir işlemi yapılmadı.";
            return RedirectToAction(nameof(Details), new { id = registerId });
        }

        // Devir hareketi oluştur
        var devirType = balanceUntilYesterday >= 0 ? CashTransactionType.Giris : CashTransactionType.Cikis;
        await _transactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
        {
            CashRegisterId = registerId,
            Type = devirType,
            Amount = Math.Abs(balanceUntilYesterday),
            TransactionDate = TurkeyTime.Now,
            PaymentType = PaymentType.Nakit,
            Description = $"Devir: {yesterday:dd.MM.yyyy} bakiyesi ({balanceUntilYesterday:N2} {register.Currency})"
        }, "CarryForward", 0);

        TempData["Success"] = $"Kasa devir işlemi yapıldı. Devir bakiyesi: {balanceUntilYesterday:N2} {register.Currency}";
        return RedirectToAction(nameof(Details), new { id = registerId });
    }

    // Kasa İşlemi Ekle
    public async Task<IActionResult> AddTransaction(int? registerId)
    {
        var registers = await _registerService.GetAllAsync();
        ViewBag.Registers = new SelectList(registers.Where(r => r.IsActive), "Id", "Name", registerId);

        var dto = new CashTransactionCreateDto();
        if (registerId.HasValue)
            dto.CashRegisterId = registerId.Value;

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTransaction(CashTransactionCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var registers = await _registerService.GetAllAsync();
            ViewBag.Registers = new SelectList(registers.Where(r => r.IsActive), "Id", "Name", dto.CashRegisterId);
            return View(dto);
        }

        await _transactionService.CreateAsync(dto);
        TempData["Success"] = "Kasa işlemi başarıyla kaydedildi.";
        return RedirectToAction(nameof(Details), new { id = dto.CashRegisterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransaction(int id, int registerId)
    {
        await _transactionService.DeleteAsync(id);
        TempData["Success"] = "Kasa işlemi silindi.";
        return RedirectToAction(nameof(Details), new { id = registerId });
    }
}
