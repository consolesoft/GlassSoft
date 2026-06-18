using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Purchasing;

public class GoodsReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public DateTime ReceiptDate { get; set; } = TurkeyTime.Now;
    public string? Notes { get; set; }

    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

public class GoodsReceiptLine : BaseEntity
{
    public int GoodsReceiptId { get; set; }
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public int PurchaseOrderLineId { get; set; }
    public PurchaseOrderLine PurchaseOrderLine { get; set; } = null!;
    public decimal ReceivedQuantity { get; set; }
    public int PlateCount { get; set; }             // Plaka adedi
    public int? GlassPlateDefinitionId { get; set; } // Plaka boyutu
}
