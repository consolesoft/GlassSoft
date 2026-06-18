using GlassSoft.Application.DTOs.Accounting;

namespace GlassSoft.Application.Interfaces;

public interface ICashRegisterService
{
    Task<IEnumerable<CashRegisterDto>> GetAllAsync();
    Task<CashRegisterDto?> GetByIdAsync(int id);
    Task<CashRegisterDto> CreateAsync(CashRegisterCreateDto dto);
    Task UpdateAsync(CashRegisterUpdateDto dto);
    Task DeleteAsync(int id);
    Task<(int RegisterCount, List<(string Name, decimal OldBalance, decimal NewBalance)> Details)> RecalculateAllBalancesAsync();
}
