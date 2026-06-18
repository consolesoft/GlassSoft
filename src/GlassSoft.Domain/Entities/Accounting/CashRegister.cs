using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Accounting;

public class CashRegister : BaseEntity
{
    public string Name { get; set; } = string.Empty; // "Ana Kasa", "Döviz Kasası" vb.
    public string Currency { get; set; } = "TRY";
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
}
