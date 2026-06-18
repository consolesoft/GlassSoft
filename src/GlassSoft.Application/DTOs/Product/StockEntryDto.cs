using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Product;

public class StockEntryDto
{
    public int Id { get; set; }
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public string MovementTypeDisplay => MovementType == StockMovementType.Giris ? "Giriş" : "Çıkış";
    public decimal Quantity { get; set; }
    public int PlateCount { get; set; }
    public int? GlassPlateDefinitionId { get; set; }
    public string? PlateDefinitionDisplay { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockEntryCreateDto
{
    public int ProductItemId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public int PlateCount { get; set; }
    public int? GlassPlateDefinitionId { get; set; }
    public string? Description { get; set; }
}

public class StockSummaryDto
{
    public int ProductItemId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductGroupName { get; set; } = string.Empty;
    public string UnitDisplay { get; set; } = string.Empty;
    public bool IsPlate { get; set; }
    public decimal TotalQuantity { get; set; }
    public int TotalPlateCount { get; set; }
}
