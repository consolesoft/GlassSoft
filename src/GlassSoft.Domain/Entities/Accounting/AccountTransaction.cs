using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Accounting;

public class AccountTransaction : BaseEntity
{
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public TransactionType Type { get; set; }     // Borç / Alacak
    public PaymentType PaymentType { get; set; }  // Nakit, Havale, Çek, Senet...
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1; // Kur
    public decimal AmountTRY { get; set; }        // TRY karşılığı
    public DateTime TransactionDate { get; set; } = TurkeyTime.Now;
    public string? ReferenceType { get; set; }    // "Order", "Manual", "Collection"
    public int? ReferenceId { get; set; }
    public string? Description { get; set; }
}
