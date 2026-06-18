using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Product;

public class ProductFeatureDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Renk, İşlem, Kaplama vs.
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;

    public ICollection<ProductFeatureOption> Options { get; set; } = new List<ProductFeatureOption>();
}

public class ProductFeatureOption : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Şeffaf, Füme, Buzlu vs.
    public decimal AdditionalPrice { get; set; } // Ek birim fiyat
    public int FeatureDefinitionId { get; set; }
    public ProductFeatureDefinition FeatureDefinition { get; set; } = null!;
}
