using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Sales;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDate { get; set; } = TurkeyTime.Now;
    public DateTime? DeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Taslak;

    // Durum geçiş tarihleri - hangi durumun ne zaman yapıldığını takip eder
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ProductionStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 0; // KDV oranı (%) - varsayılan 0
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount => TotalAmount * TaxRate / 100m;
    public decimal GrandTotal => TotalAmount + TaxAmount;
    public string? Notes { get; set; }

    // Cari/firmadan farklı, siparişi veren özel müşteri (alt müşteri, kontak, vs)
    public string? OrderCustomerName { get; set; }

    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
