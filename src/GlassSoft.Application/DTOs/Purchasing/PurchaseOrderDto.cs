using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Purchasing;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        PurchaseOrderStatus.Taslak => "Taslak",
        PurchaseOrderStatus.Onaylandi => "Onaylandı",
        PurchaseOrderStatus.KismiTeslim => "Kısmi Teslim",
        PurchaseOrderStatus.Tamamlandi => "Tamamlandı",
        PurchaseOrderStatus.IptalEdildi => "İptal Edildi",
        _ => Status.ToString()
    };
    public string StatusBadgeClass => Status switch
    {
        PurchaseOrderStatus.Taslak => "badge-secondary",
        PurchaseOrderStatus.Onaylandi => "badge-primary",
        PurchaseOrderStatus.KismiTeslim => "badge-warning",
        PurchaseOrderStatus.Tamamlandi => "badge-success",
        PurchaseOrderStatus.IptalEdildi => "badge-danger",
        _ => "badge-secondary"
    };
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount => TotalAmount * TaxRate / 100m;
    public decimal GrandTotal => TotalAmount + TaxAmount;
    public string? Notes { get; set; }
    public int LineCount { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = new();
}

public class PurchaseOrderCreateDto
{
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = TurkeyTime.Now;
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 20;
    public string? Notes { get; set; }
    public List<PurchaseOrderLineCreateDto> Lines { get; set; } = new();
}

public class PurchaseOrderUpdateDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 20;
    public string? Notes { get; set; }
    public List<PurchaseOrderLineCreateDto> Lines { get; set; } = new();
}

public class PurchaseOrderLineDto
{
    public int Id { get; set; }
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal ReceivedQuantity { get; set; }
}

public class PurchaseOrderLineCreateDto
{
    public int ProductItemId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
