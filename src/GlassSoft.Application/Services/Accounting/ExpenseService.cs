using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Accounting;

public class ExpenseService : IExpenseService
{
    private readonly IRepository<Expense> _repository;

    public ExpenseService(IRepository<Expense> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                Category = e.Category,
                Title = e.Title,
                Amount = e.Amount,
                Currency = e.Currency,
                ExchangeRate = e.ExchangeRate,
                AmountTRY = e.AmountTRY,
                PaymentType = e.PaymentType,
                Description = e.Description,
                ReceiptNo = e.ReceiptNo
            })
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseDto>> GetByCategoryAsync(ExpenseCategoryType category)
    {
        return await _repository.Query()
            .Where(e => e.Category == category)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                Category = e.Category,
                Title = e.Title,
                Amount = e.Amount,
                Currency = e.Currency,
                ExchangeRate = e.ExchangeRate,
                AmountTRY = e.AmountTRY,
                PaymentType = e.PaymentType,
                Description = e.Description,
                ReceiptNo = e.ReceiptNo
            })
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExpenseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _repository.Query()
            .Where(e => e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                Category = e.Category,
                Title = e.Title,
                Amount = e.Amount,
                Currency = e.Currency,
                ExchangeRate = e.ExchangeRate,
                AmountTRY = e.AmountTRY,
                PaymentType = e.PaymentType,
                Description = e.Description,
                ReceiptNo = e.ReceiptNo
            })
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(e => e.Id == id)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                ExpenseDate = e.ExpenseDate,
                Category = e.Category,
                Title = e.Title,
                Amount = e.Amount,
                Currency = e.Currency,
                ExchangeRate = e.ExchangeRate,
                AmountTRY = e.AmountTRY,
                PaymentType = e.PaymentType,
                Description = e.Description,
                ReceiptNo = e.ReceiptNo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ExpenseDto> CreateAsync(ExpenseCreateDto dto)
    {
        var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;

        var entity = new Expense
        {
            ExpenseDate = TurkeyTime.WithCurrentTime(dto.ExpenseDate),
            Category = dto.Category,
            Title = dto.Title,
            Amount = dto.Amount,
            Currency = dto.Currency,
            ExchangeRate = dto.ExchangeRate,
            AmountTRY = amountTRY,
            PaymentType = dto.PaymentType,
            Description = dto.Description,
            ReceiptNo = dto.ReceiptNo
        };

        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(ExpenseUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Gider bulunamadı: {dto.Id}");

        entity.ExpenseDate = TurkeyTime.WithCurrentTime(dto.ExpenseDate);
        entity.Category = dto.Category;
        entity.Title = dto.Title;
        entity.Amount = dto.Amount;
        entity.Currency = dto.Currency;
        entity.ExchangeRate = dto.ExchangeRate;
        entity.AmountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;
        entity.PaymentType = dto.PaymentType;
        entity.Description = dto.Description;
        entity.ReceiptNo = dto.ReceiptNo;

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Gider bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task<decimal> GetTotalByMonthAsync(int year, int month)
    {
        return await _repository.Query()
            .Where(e => e.ExpenseDate.Year == year && e.ExpenseDate.Month == month)
            .SumAsync(e => e.AmountTRY);
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int year)
    {
        var expenses = await _repository.Query()
            .Where(e => e.ExpenseDate.Year == year)
            .ToListAsync();

        return expenses
            .GroupBy(e => e.Category)
            .ToDictionary(
                g => g.Key switch
                {
                    ExpenseCategoryType.Kira => "Kira",
                    ExpenseCategoryType.Fatura => "Fatura",
                    ExpenseCategoryType.Maas => "Maaş",
                    ExpenseCategoryType.Malzeme => "Malzeme",
                    ExpenseCategoryType.Nakliye => "Nakliye",
                    ExpenseCategoryType.Bakim => "Bakım/Onarım",
                    ExpenseCategoryType.Vergi => "Vergi/Harç",
                    ExpenseCategoryType.Sigorta => "Sigorta",
                    ExpenseCategoryType.Reklam => "Reklam/Pazarlama",
                    ExpenseCategoryType.Diger => "Diğer",
                    _ => g.Key.ToString()
                },
                g => g.Sum(e => e.AmountTRY));
    }
}
