using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Sales;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusDisplay => Status switch
    {
        OrderStatus.Taslak => "Taslak",
        OrderStatus.Onaylandi => "Onaylandı",
        OrderStatus.Uretimde => "Üretimde",
        OrderStatus.Tamamlandi => "Tamamlandı",
        OrderStatus.IptalEdildi => "İptal Edildi",
        _ => Status.ToString()
    };
    public string StatusBadgeClass => Status switch
    {
        OrderStatus.Taslak => "badge-secondary",
        OrderStatus.Onaylandi => "badge-primary",
        OrderStatus.Uretimde => "badge-warning",
        OrderStatus.Tamamlandi => "badge-success",
        OrderStatus.IptalEdildi => "badge-danger",
        _ => "badge-secondary"
    };
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount => TotalAmount * TaxRate / 100m;
    public decimal GrandTotal => TotalAmount + TaxAmount;
    public string? Notes { get; set; }
    public string? OrderCustomerName { get; set; }
    public int LineCount { get; set; }
    public decimal TotalAreaM2 { get; set; }
    public int TotalQuantity { get; set; }
    public int DeliveredQuantity { get; set; }
    public int RemainingQuantity => Math.Max(0, TotalQuantity - DeliveredQuantity);

    // Durum geçiş tarihleri
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ProductionStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<OrderLineDto> Lines { get; set; } = new();
}

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = TurkeyTime.Now;
    public DateTime? DeliveryDate { get; set; }
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 0;
    public string? Notes { get; set; }
    public string? OrderCustomerName { get; set; }
    public List<OrderLineCreateDto> Lines { get; set; } = new();
}

public class OrderUpdateDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Currency { get; set; } = "TRY";
    public int TaxRate { get; set; } = 0;
    public string? Notes { get; set; }
    public string? OrderCustomerName { get; set; }
    public List<OrderLineCreateDto> Lines { get; set; } = new();
}

public class OrderLineDto
{
    public int Id { get; set; }
    public int? ProductItemId { get; set; }
    public string? ProductItemName { get; set; }
    public int? RecipeId { get; set; }
    public string? RecipeCode { get; set; }
    public string? PozNo { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsRetail => !WidthMm.HasValue || !HeightMm.HasValue || (WidthMm == 0 && HeightMm == 0);
    public decimal AreaM2 => IsRetail ? 0 : ((WidthMm!.Value * HeightMm!.Value) / 1_000_000m);
    public decimal TotalAreaM2 => AreaM2 * Quantity;
    public bool IsOptimized { get; set; }
    public string? Notes { get; set; }
    public DateTime? LabelPrintedAt { get; set; }
    public bool IsLabelPrinted => LabelPrintedAt.HasValue;
    public string ItemName => ProductItemName ?? RecipeCode ?? "-";
    public List<OrderLineFeatureDto> Features { get; set; } = new();
    public decimal FeaturesTotalPrice => Features.Sum(f => f.TotalPrice);
}

public class OrderLineFeatureDto
{
    public int Id { get; set; }
    public int FeatureDefinitionId { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderLineCreateDto
{
    public int Id { get; set; } // 0 = yeni kalem, > 0 = mevcut kalem (update)
    public int? ProductItemId { get; set; }
    public int? RecipeId { get; set; }
    public string? PozNo { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public List<OrderLineFeatureCreateDto>? Features { get; set; }
}

public class OrderLineFeatureCreateDto
{
    public int FeatureDefinitionId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class OrderLinePriceHistoryDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
