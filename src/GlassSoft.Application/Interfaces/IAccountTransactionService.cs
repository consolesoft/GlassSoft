using GlassSoft.Application.DTOs.Accounting;

namespace GlassSoft.Application.Interfaces;

public interface IAccountTransactionService
{
    Task<IEnumerable<AccountTransactionDto>> GetAllAsync();
    Task<IEnumerable<AccountTransactionDto>> GetByCustomerAsync(int customerId);
    Task<AccountTransactionDto?> GetByIdAsync(int id);
    Task<AccountTransactionDto> CreateAsync(AccountTransactionCreateDto dto);
    Task UpdateAsync(AccountTransactionUpdateDto dto);
    Task DeleteAsync(int id);
    Task<decimal> GetCustomerBalanceAsync(int customerId);
}
