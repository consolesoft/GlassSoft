using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Accounting;

public class ChequeNote : BaseEntity
{
    public ChequeNoteType Type { get; set; }         // Çek / Senet
    public string? DocumentNumber { get; set; }      // Çek/Senet numarası
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string? BankName { get; set; }
    public string? BranchName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public DateTime IssueDate { get; set; }          // Düzenleme tarihi
    public DateTime DueDate { get; set; }            // Vade tarihi
    public ChequeNoteStatus Status { get; set; } = ChequeNoteStatus.Portfoyde;
    public string? Notes { get; set; }
}
