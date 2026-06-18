using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Product;

public class ProductItem : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductGroupId { get; set; }
    public ProductGroup ProductGroup { get; set; } = null!;
    public UnitType Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPlate { get; set; } // Plaka mı (cam plaka takibi için)
    public decimal? ThicknessMm { get; set; } // Cam kalınlığı (mm)

    public ICollection<ProductFeatureDefinition> FeatureDefinitions { get; set; } = new List<ProductFeatureDefinition>();
    public ICollection<GlassPlateDefinition> PlateDefinitions { get; set; } = new List<GlassPlateDefinition>();
    public ICollection<StockEntry> StockEntries { get; set; } = new List<StockEntry>();
}
