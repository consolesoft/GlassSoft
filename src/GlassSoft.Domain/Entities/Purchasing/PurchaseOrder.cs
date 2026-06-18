using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Purchasing;

public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; } = TurkeyTime.Now;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Taslak;
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 20;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
}
