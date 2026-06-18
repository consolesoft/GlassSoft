using GlassSoft.Domain.Common;
namespace GlassSoft.Application.DTOs.Accounting;

public class CurrencyRateDto
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public DateTime RateDate { get; set; }
}

public class CurrencyRateCreateDto
{
    public string CurrencyCode { get; set; } = "USD";
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public DateTime RateDate { get; set; } = TurkeyTime.Now;
}
