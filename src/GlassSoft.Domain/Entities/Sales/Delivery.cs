using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Sales;

public class Delivery : BaseEntity
{
    public string DeliveryNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public DateTime DeliveryDate { get; set; } = TurkeyTime.Now;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Taslak;
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<DeliveryLine> Lines { get; set; } = new List<DeliveryLine>();
}
