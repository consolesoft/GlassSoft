using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Accounting;

public class Expense : BaseEntity
{
    public DateTime ExpenseDate { get; set; } = TurkeyTime.Now;
    public ExpenseCategoryType Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal AmountTRY { get; set; }
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNo { get; set; } // Fiş/fatura no
}
