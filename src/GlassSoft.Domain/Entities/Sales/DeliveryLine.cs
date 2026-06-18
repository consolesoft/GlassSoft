using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Sales;

public class DeliveryLine : BaseEntity
{
    public int DeliveryId { get; set; }
    public Delivery Delivery { get; set; } = null!;
    public int OrderLineId { get; set; }
    public OrderLine OrderLine { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}
