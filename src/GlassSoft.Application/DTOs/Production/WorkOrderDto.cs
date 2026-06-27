using GlassSoft.Domain.Common;
namespace GlassSoft.Application.DTOs.Production;

public class WorkOrderDto
{
    public int Id { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
    public DateTime PlannedDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int LineCount { get; set; }
    public int CuttingPlanCount { get; set; }
    public List<WorkOrderLineDto> Lines { get; set; } = new();
    public List<CuttingPlanDto> CuttingPlans { get; set; } = new();
    public List<WorkOrderOrderDto> RelatedOrders { get; set; } = new();
}

public class WorkOrderCreateDto
{
    public int OrderId { get; set; }
    public DateTime PlannedDate { get; set; } = TurkeyTime.Now.AddDays(1);
    public string? Notes { get; set; }
}

public class WorkOrderLineDto
{
    public int Id { get; set; }
    public int? OrderLineId { get; set; }
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public int Quantity { get; set; }
    public string MaterialType { get; set; } = string.Empty;
    public string MaterialTypeDisplay => MaterialType switch
    {
        "Glass" => "Cam",
        "Spacer" => "Ara Parça",
        "Consumable" => "Sarf",
        _ => MaterialType
    };
    public bool IsOptimized { get; set; }
    public decimal AreaM2 => (WidthMm * HeightMm) / 1_000_000m;
    public decimal TotalAreaM2 => AreaM2 * Quantity;
}

public class CuttingPlanDto
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public int GlassPlateDefinitionId { get; set; }
    public string PlateDisplayName { get; set; } = string.Empty;
    public decimal PlateWidthMm { get; set; }
    public decimal PlateHeightMm { get; set; }
    public int PlateIndex { get; set; }
    public decimal WastePercentage { get; set; }
    public decimal UsedAreaM2 { get; set; }
    public decimal WasteAreaM2 { get; set; }
    public string? CsvOutput { get; set; }
    public string? DxfOutput { get; set; }
    public bool IsApproved { get; set; }
    public List<CuttingPlanItemDto> Items { get; set; } = new();
}

public class CuttingPlanItemDto
{
    public int Id { get; set; }
    public int? OrderLineId { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool IsRotated { get; set; }
    public decimal AreaM2 => (WidthMm * HeightMm) / 1_000_000m;
}

public class WorkOrderCreateFromLinesDto
{
    public List<int> SelectedOrderLineIds { get; set; } = new();
    public DateTime PlannedDate { get; set; } = TurkeyTime.Now.AddDays(1);
    public string? Notes { get; set; }
}

public class UnoptimizedOrderLineDto
{
    public int OrderLineId { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
    public int? ProductItemId { get; set; }
    public string? ProductItemName { get; set; }
    public int? RecipeId { get; set; }
    public string? RecipeCode { get; set; }
    public string ItemName => ProductItemName ?? RecipeCode ?? "-";
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public int Quantity { get; set; }
    public decimal AreaM2 => (WidthMm * HeightMm) / 1_000_000m;
}

public class WorkOrderOrderDto
{
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerTitle { get; set; } = string.Empty;
}

public class CuttingPlanUpdateDto
{
    public List<CuttingPlanItemUpdateDto> Items { get; set; } = new();
}

public class CuttingPlanItemUpdateDto
{
    public int Id { get; set; }
    public decimal X { get; set; }
    public decimal Y { get; set; }
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool IsRotated { get; set; }
}
