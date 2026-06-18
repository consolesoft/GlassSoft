using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Product;

public class StockEntry : BaseEntity
{
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;
    public StockMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }       // Miktar (birime göre: adet veya m²)
    public int PlateCount { get; set; }         // Plaka adedi (plaka ürünler için)
    public int? GlassPlateDefinitionId { get; set; } // Hangi plaka boyutundan
    public GlassPlateDefinition? GlassPlateDefinition { get; set; }
    public string? ReferenceType { get; set; }  // "PurchaseOrder", "WorkOrder" vb.
    public int? ReferenceId { get; set; }       // İlgili kayıt Id'si
    public string? Description { get; set; }
}
