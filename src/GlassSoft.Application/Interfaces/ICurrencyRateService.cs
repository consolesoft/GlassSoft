using GlassSoft.Application.DTOs.Accounting;

namespace GlassSoft.Application.Interfaces;

public interface ICurrencyRateService
{
    Task<IEnumerable<CurrencyRateDto>> GetAllAsync();
    Task<IEnumerable<CurrencyRateDto>> GetLatestRatesAsync();
    Task<CurrencyRateDto> CreateAsync(CurrencyRateCreateDto dto);
}
