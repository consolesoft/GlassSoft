using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllAsync();
    Task<IEnumerable<ExpenseDto>> GetByCategoryAsync(ExpenseCategoryType category);
    Task<IEnumerable<ExpenseDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(ExpenseCreateDto dto);
    Task UpdateAsync(ExpenseUpdateDto dto);
    Task DeleteAsync(int id);
    Task<decimal> GetTotalByMonthAsync(int year, int month);
    Task<Dictionary<string, decimal>> GetMonthlySummaryAsync(int year);
}
