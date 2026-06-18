using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Sales;

namespace GlassSoft.Domain.Entities.Production;

public class WorkOrderLine : BaseEntity
{
    public int WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public int? OrderLineId { get; set; }
    public OrderLine? OrderLine { get; set; }
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public int Quantity { get; set; }
    public string MaterialType { get; set; } = string.Empty; // "Glass", "Spacer", "Consumable"
    public bool IsOptimized { get; set; }
}
