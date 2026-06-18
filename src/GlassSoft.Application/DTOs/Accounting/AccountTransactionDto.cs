using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Accounting;

public class AccountTransactionDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public string TypeDisplay => Type switch
    {
        TransactionType.Borc => "Borç",
        TransactionType.Alacak => "Alacak",
        _ => Type.ToString()
    };
    public string TypeBadgeClass => Type switch
    {
        TransactionType.Borc => "badge-success",
        TransactionType.Alacak => "badge-danger",
        _ => "badge-secondary"
    };
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
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal AmountTRY { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Description { get; set; }
}

public class AccountTransactionCreateDto
{
    public int CustomerId { get; set; }
    public TransactionType Type { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public DateTime TransactionDate { get; set; } = TurkeyTime.Now;
    public string? Description { get; set; }
}

public class AccountTransactionUpdateDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public TransactionType Type { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public decimal ExchangeRate { get; set; } = 1;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
}
