using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Accounting;

public class ExpenseDto
{
    public int Id { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseCategoryType Category { get; set; }
    public string CategoryDisplay => Category switch
    {
        ExpenseCategoryType.Kira => "Kira",
        ExpenseCategoryType.Fatura => "Fatura",
        ExpenseCategoryType.Maas => "Maaş",
        ExpenseCategoryType.Malzeme => "Malzeme",
        ExpenseCategoryType.Nakliye => "Nakliye",
        ExpenseCategoryType.Bakim => "Bakım/Onarım",
        ExpenseCategoryType.Vergi => "Vergi/Harç",
        ExpenseCategoryType.Sigorta => "Sigorta",
        ExpenseCategoryType.Reklam => "Reklam/Pazarlama",
        ExpenseCategoryType.Diger => "Diğer",
        ExpenseCategoryType.Yakit => "Yakıt",
        _ => Category.ToString()
    };
    public string CategoryBadgeClass => Category switch
    {
        ExpenseCategoryType.Kira => "badge-primary",
        ExpenseCategoryType.Fatura => "badge-warning",
        ExpenseCategoryType.Maas => "badge-info",
        ExpenseCategoryType.Malzeme => "badge-secondary",
        ExpenseCategoryType.Nakliye => "badge-dark",
        ExpenseCategoryType.Bakim => "badge-danger",
        ExpenseCategoryType.Vergi => "badge-success",
        ExpenseCategoryType.Sigorta => "badge-light",
        ExpenseCategoryType.Reklam => "badge-purple",
        ExpenseCategoryType.Yakit => "badge-warning",
        _ => "badge-secondary"
    };
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal AmountTRY { get; set; }
    public PaymentType PaymentType { get; set; }
    public string PaymentTypeDisplay => PaymentType switch
    {
        PaymentType.Nakit => "Nakit",
        PaymentType.Havale => "Havale",
        PaymentType.Cek => "Çek",
        PaymentType.Senet => "Senet",
        PaymentType.KrediKarti => "Kredi Kartı",
        _ => PaymentType.ToString()
    };
    public string? Description { get; set; }
    public string? ReceiptNo { get; set; }
}

public class ExpenseCreateDto
{
    public DateTime ExpenseDate { get; set; } = TurkeyTime.Now;
    public ExpenseCategoryType Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNo { get; set; }
}

public class ExpenseUpdateDto
{
    public int Id { get; set; }
    public DateTime ExpenseDate { get; set; }
    public ExpenseCategoryType Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
    public string? ReceiptNo { get; set; }
}
