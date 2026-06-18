using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Sales;

public class OrderLineFeature : BaseEntity
{
    public int OrderLineId { get; set; }
    public OrderLine OrderLine { get; set; } = null!;
    public int FeatureDefinitionId { get; set; }
    public OrderFeatureDefinition FeatureDefinition { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
