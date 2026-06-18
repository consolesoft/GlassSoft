using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly ICashTransactionService _cashTransactionService;

    public ExpensesController(
        IExpenseService expenseService,
        ICashRegisterService cashRegisterService,
        ICashTransactionService cashTransactionService)
    {
        _expenseService = expenseService;
        _cashRegisterService = cashRegisterService;
        _cashTransactionService = cashTransactionService;
    }

    private async Task LoadCashRegisters()
    {
        var registers = await _cashRegisterService.GetAllAsync();
        ViewBag.CashRegisters = new SelectList(registers.Where(r => r.IsActive), "Id", "Name");
    }

    public async Task<IActionResult> Index(ExpenseCategoryType? category, DateTime? startDate, DateTime? endDate, string? search)
    {
        var expenses = (await _expenseService.GetAllAsync()).ToList();

        // Tarih filtresi
        if (startDate.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseDate.Date >= startDate.Value.Date).ToList();
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
        }
        if (endDate.HasValue)
        {
            expenses = expenses.Where(e => e.ExpenseDate.Date <= endDate.Value.Date).ToList();
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
        }

        // Kategori filtresi
        if (category.HasValue)
        {
            expenses = expenses.Where(e => e.Category == category.Value).ToList();
            ViewBag.SelectedCategory = (int)category.Value;
        }

        // Metin arama (başlık + açıklama)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLowerInvariant();
            expenses = expenses.Where(e =>
                (e.Title != null && e.Title.ToLowerInvariant().Contains(q)) ||
                (e.Description != null && e.Description.ToLowerInvariant().Contains(q))
            ).ToList();
            ViewBag.Search = search;
        }

        var isFiltered = startDate.HasValue || endDate.HasValue || category.HasValue || !string.IsNullOrWhiteSpace(search);
        ViewBag.IsFiltered = isFiltered;
        ViewBag.TotalTRY = expenses.Sum(e => e.AmountTRY);
        ViewBag.MonthlyTotal = await _expenseService.GetTotalByMonthAsync(TurkeyTime.Now.Year, TurkeyTime.Now.Month);
        ViewBag.CategorySummary = await _expenseService.GetMonthlySummaryAsync(TurkeyTime.Now.Year);
        ViewBag.FilteredCount = expenses.Count;

        return View(expenses);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCashRegisters();
        return View(new ExpenseCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseCreateDto dto, int? cashRegisterId)
    {
        if (!ModelState.IsValid)
        {
            await LoadCashRegisters();
            return View(dto);
        }

        // Kasa seçimi zorunlu
        if (!cashRegisterId.HasValue || cashRegisterId.Value <= 0)
        {
            await LoadCashRegisters();
            ModelState.AddModelError("", "Kasa seçimi zorunludur. Gider hangi kasadan çıkacaksa onu seçmelisiniz.");
            TempData["Error"] = "Kasa seçimi zorunludur!";
            return View(dto);
        }

        var expense = await _expenseService.CreateAsync(dto);

        await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
        {
            CashRegisterId = cashRegisterId.Value,
            Type = CashTransactionType.Cikis,
            Amount = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate,
            TransactionDate = dto.ExpenseDate,
            PaymentType = dto.PaymentType,
            Description = $"Gider: {dto.Title}"
        }, "Expense", expense.Id);

        TempData["Success"] = "Gider başarıyla kaydedildi. (Kasa çıkışı oluşturuldu)";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var expense = await _expenseService.GetByIdAsync(id);
        if (expense == null) return NotFound();

        await LoadCashRegisters();

        // Bu gidere bağlı kasa hareketi var mı?
        var allCash = await _cashTransactionService.GetAllAsync();
        var linkedCash = allCash.FirstOrDefault(c => c.ReferenceType == "Expense" && c.ReferenceId == id);
        ViewBag.CurrentCashRegisterId = linkedCash?.CashRegisterId;

        var dto = new ExpenseUpdateDto
        {
            Id = expense.Id,
            ExpenseDate = expense.ExpenseDate,
            Category = expense.Category,
            Title = expense.Title,
            Amount = expense.Amount,
            Currency = expense.Currency,
            ExchangeRate = expense.ExchangeRate,
            PaymentType = expense.PaymentType,
            Description = expense.Description,
            ReceiptNo = expense.ReceiptNo
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExpenseUpdateDto dto, int? cashRegisterId)
    {
        if (!ModelState.IsValid)
        {
            await LoadCashRegisters();
            return View(dto);
        }

        // Kasa seçimi zorunlu
        if (!cashRegisterId.HasValue || cashRegisterId.Value <= 0)
        {
            await LoadCashRegisters();
            ViewBag.CurrentCashRegisterId = null;
            ModelState.AddModelError("", "Kasa seçimi zorunludur. Gider hangi kasadan çıkacaksa onu seçmelisiniz.");
            TempData["Error"] = "Kasa seçimi zorunludur!";
            return View(dto);
        }

        // Mevcut bağlı kasa hareketini sil
        var allCash = await _cashTransactionService.GetAllAsync();
        var linkedCash = allCash.FirstOrDefault(c => c.ReferenceType == "Expense" && c.ReferenceId == dto.Id);
        if (linkedCash != null)
            await _cashTransactionService.DeleteAsync(linkedCash.Id);

        await _expenseService.UpdateAsync(dto);

        var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;
        await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
        {
            CashRegisterId = cashRegisterId.Value,
            Type = CashTransactionType.Cikis,
            Amount = amountTRY,
            TransactionDate = dto.ExpenseDate,
            PaymentType = dto.PaymentType,
            Description = $"Gider: {dto.Title}"
        }, "Expense", dto.Id);

        TempData["Success"] = "Gider başarıyla güncellendi. (Kasa çıkışı güncellendi)";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // İlgili kasa hareketini de sil (ReferenceType = "Expense")
        var allCashTransactions = await _cashTransactionService.GetAllAsync();
        var relatedCash = allCashTransactions.FirstOrDefault(t => t.ReferenceType == "Expense" && t.ReferenceId == id);
        if (relatedCash != null)
        {
            await _cashTransactionService.DeleteAsync(relatedCash.Id);
        }

        await _expenseService.DeleteAsync(id);
        TempData["Success"] = "Gider silindi." + (relatedCash != null ? " (İlgili kasa hareketi de silindi)" : "");
        return RedirectToAction(nameof(Index));
    }
}
