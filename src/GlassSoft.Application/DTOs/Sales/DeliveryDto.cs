using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Sales;

public class DeliveryDto
{
    public int Id { get; set; }
    public string DeliveryNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public DeliveryStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }
    public int LineCount { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAreaM2 { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<DeliveryLineDto> Lines { get; set; } = new();

    public string StatusDisplay => Status switch
    {
        DeliveryStatus.Taslak => "Taslak",
        DeliveryStatus.Onaylandi => "Onaylandı",
        _ => Status.ToString()
    };

    public string StatusBadgeClass => Status switch
    {
        DeliveryStatus.Taslak => "badge-secondary",
        DeliveryStatus.Onaylandi => "badge-success",
        _ => "badge-secondary"
    };
}

public class DeliveryLineDto
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }
    public int OrderLineId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? RecipeCode { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public int OrderLineQuantity { get; set; }
    public int Quantity { get; set; }
    public string? PozNo { get; set; }
    public string? Notes { get; set; }

    public bool IsRetail => !WidthMm.HasValue || !HeightMm.HasValue || (WidthMm == 0 && HeightMm == 0);
    public decimal AreaM2 => IsRetail ? 0 : ((WidthMm!.Value * HeightMm!.Value) / 1_000_000m);
    public decimal TotalAreaM2 => AreaM2 * Quantity;
}

public class DeliveryCreateDto
{
    public int OrderId { get; set; }
    public DateTime DeliveryDate { get; set; } = DateTime.Today;
    public string? Notes { get; set; }
    public List<DeliveryLineCreateDto> Lines { get; set; } = new();
}

public class DeliveryUpdateDto
{
    public int Id { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<DeliveryLineCreateDto> Lines { get; set; } = new();
}

public class DeliveryLineCreateDto
{
    public int OrderLineId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class OpenOrderLineDto
{
    public int OrderLineId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? RecipeCode { get; set; }
    public decimal? WidthMm { get; set; }
    public decimal? HeightMm { get; set; }
    public int OrderQuantity { get; set; }
    public int DeliveredQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public string? PozNo { get; set; }

    public bool IsRetail => !WidthMm.HasValue || !HeightMm.HasValue || (WidthMm == 0 && HeightMm == 0);
    public decimal AreaM2 => IsRetail ? 0 : ((WidthMm!.Value * HeightMm!.Value) / 1_000_000m);
}

public class OpenOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerTitle { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public OrderStatus Status { get; set; }
    public int TotalRemainingQuantity { get; set; }
}
