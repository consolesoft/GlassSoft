using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;

namespace GlassSoft.Domain.Entities.Purchasing;

public class PurchaseOrderLine : BaseEntity
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ReceivedQuantity { get; set; } // Teslim alınan miktar
}
