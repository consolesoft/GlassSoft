using GlassSoft.Application.DTOs.Accounting;

namespace GlassSoft.Application.Interfaces;

public interface ICashTransactionService
{
    Task<IEnumerable<CashTransactionDto>> GetAllAsync();
    Task<IEnumerable<CashTransactionDto>> GetByRegisterAsync(int cashRegisterId);
    Task<CashTransactionDto?> GetByIdAsync(int id);
    Task<CashTransactionDto> CreateAsync(CashTransactionCreateDto dto);
    Task<CashTransactionDto> CreateWithReferenceAsync(CashTransactionCreateDto dto, string referenceType, int referenceId);
    Task DeleteAsync(int id);
}
