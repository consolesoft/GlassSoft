using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Accounting;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly IRepository<CurrencyRate> _repository;

    public CurrencyRateService(IRepository<CurrencyRate> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Select(cr => new CurrencyRateDto
            {
                Id = cr.Id,
                CurrencyCode = cr.CurrencyCode,
                BuyRate = cr.BuyRate,
                SellRate = cr.SellRate,
                RateDate = cr.RateDate
            })
            .OrderByDescending(cr => cr.RateDate)
            .ThenBy(cr => cr.CurrencyCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<CurrencyRateDto>> GetLatestRatesAsync()
    {
        var latestDate = await _repository.Query()
            .MaxAsync(cr => (DateTime?)cr.RateDate);

        if (latestDate == null)
            return Enumerable.Empty<CurrencyRateDto>();

        return await _repository.Query()
            .Where(cr => cr.RateDate.Date == latestDate.Value.Date)
            .Select(cr => new CurrencyRateDto
            {
                Id = cr.Id,
                CurrencyCode = cr.CurrencyCode,
                BuyRate = cr.BuyRate,
                SellRate = cr.SellRate,
                RateDate = cr.RateDate
            })
            .OrderBy(cr => cr.CurrencyCode)
            .ToListAsync();
    }

    public async Task<CurrencyRateDto> CreateAsync(CurrencyRateCreateDto dto)
    {
        var entity = new CurrencyRate
        {
            CurrencyCode = dto.CurrencyCode,
            BuyRate = dto.BuyRate,
            SellRate = dto.SellRate,
            RateDate = TurkeyTime.WithCurrentTime(dto.RateDate)
        };

        await _repository.AddAsync(entity);

        return new CurrencyRateDto
        {
            Id = entity.Id,
            CurrencyCode = entity.CurrencyCode,
            BuyRate = entity.BuyRate,
            SellRate = entity.SellRate,
            RateDate = entity.RateDate
        };
    }
}
