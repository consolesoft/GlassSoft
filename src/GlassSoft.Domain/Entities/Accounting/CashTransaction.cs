using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Accounting;

public class CashTransaction : BaseEntity
{
    public int CashRegisterId { get; set; }
    public CashRegister CashRegister { get; set; } = null!;
    public CashTransactionType Type { get; set; } // Giriş / Çıkış
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = TurkeyTime.Now;
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; } // "Expense", "Order", "Manual"
    public int? ReferenceId { get; set; }
}
