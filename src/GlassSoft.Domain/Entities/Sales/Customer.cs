using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Sales;

public class Customer : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;       // Unvan
    public CustomerType CustomerType { get; set; } = CustomerType.Musteri;
    public string? TaxNumber { get; set; }                   // Vergi No
    public string? TaxOffice { get; set; }                   // Vergi Dairesi
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "TRY";           // Varsayılan para birimi
    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
