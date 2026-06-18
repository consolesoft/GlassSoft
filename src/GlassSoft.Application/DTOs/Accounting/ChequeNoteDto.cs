using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Accounting;

public class ChequeNoteDto
{
    public int Id { get; set; }
    public ChequeNoteType Type { get; set; }
    public string TypeDisplay => Type switch
    {
        ChequeNoteType.Cek => "Çek",
        ChequeNoteType.Senet => "Senet",
        _ => Type.ToString()
    };
    public string TypeBadgeClass => Type switch
    {
        ChequeNoteType.Cek => "badge-info",
        ChequeNoteType.Senet => "badge-warning",
        _ => "badge-secondary"
    };
    public string? DocumentNumber { get; set; }
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public ChequeNoteStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        ChequeNoteStatus.Portfoyde => "Portföyde",
        ChequeNoteStatus.Tahsilde => "Tahsilde",
        ChequeNoteStatus.TahsilEdildi => "Tahsil Edildi",
        ChequeNoteStatus.Karsiliksiz => "Karşılıksız",
        ChequeNoteStatus.IadeEdildi => "İade Edildi",
        ChequeNoteStatus.Cirolandi => "Cirolandı",
        _ => Status.ToString()
    };
    public string StatusBadgeClass => Status switch
    {
        ChequeNoteStatus.Portfoyde => "badge-primary",
        ChequeNoteStatus.Tahsilde => "badge-warning",
        ChequeNoteStatus.TahsilEdildi => "badge-success",
        ChequeNoteStatus.Karsiliksiz => "badge-danger",
        ChequeNoteStatus.IadeEdildi => "badge-secondary",
        ChequeNoteStatus.Cirolandi => "badge-info",
        _ => "badge-secondary"
    };
    public string? Notes { get; set; }
}

public class ChequeNoteCreateDto
{
    public ChequeNoteType Type { get; set; }
    public string? DocumentNumber { get; set; }
    public int CustomerId { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime IssueDate { get; set; } = TurkeyTime.Now;
    public DateTime DueDate { get; set; } = TurkeyTime.Now.AddMonths(1);
    public string? Notes { get; set; }
}

public class ChequeNoteUpdateDto
{
    public int Id { get; set; }
    public ChequeNoteType Type { get; set; }
    public string? DocumentNumber { get; set; }
    public int CustomerId { get; set; }
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
}
