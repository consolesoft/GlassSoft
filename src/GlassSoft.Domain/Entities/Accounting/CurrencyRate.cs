using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Accounting;

public class CurrencyRate : BaseEntity
{
    public string CurrencyCode { get; set; } = string.Empty; // USD, EUR vs.
    public decimal BuyRate { get; set; }
    public decimal SellRate { get; set; }
    public DateTime RateDate { get; set; }
}
