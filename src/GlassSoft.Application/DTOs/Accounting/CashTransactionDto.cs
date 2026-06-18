using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Accounting;

public class CashTransactionDto
{
    public int Id { get; set; }
    public int CashRegisterId { get; set; }
    public string CashRegisterName { get; set; } = string.Empty;
    public CashTransactionType Type { get; set; }
    public string TypeDisplay => Type switch
    {
        CashTransactionType.Giris => "Giriş",
        CashTransactionType.Cikis => "Çıkış",
        _ => Type.ToString()
    };
    public string TypeBadgeClass => Type switch
    {
        CashTransactionType.Giris => "badge-success",
        CashTransactionType.Cikis => "badge-danger",
        _ => "badge-secondary"
    };
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
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
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public decimal RunningBalance { get; set; } // Anlık bakiye
}

public class CashTransactionCreateDto
{
    public int CashRegisterId { get; set; }
    public CashTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; } = TurkeyTime.Now;
    public PaymentType PaymentType { get; set; }
    public string? Description { get; set; }
}
