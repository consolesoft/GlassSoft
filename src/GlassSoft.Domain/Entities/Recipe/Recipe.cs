using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;

namespace GlassSoft.Domain.Entities.Recipe;

public class Recipe : BaseEntity
{
    public string Code { get; set; } = string.Empty;     // "4+12+4", "4+12+4F" gibi kısa kod
    public string Name { get; set; } = string.Empty;     // "4mm Düz + 12mm Çıta + 4mm Düz Isıcam"
    public string? Description { get; set; }
    public decimal BaseUnitPrice { get; set; }            // Temel m² birim fiyatı
    public bool IsActive { get; set; } = true;
    public int? ProductItemId { get; set; }               // Bu reçetenin ait olduğu ürün
    public ProductItem? ProductItem { get; set; }

    public ICollection<RecipeLayer> Layers { get; set; } = new List<RecipeLayer>();
    public ICollection<RecipeConsumable> Consumables { get; set; } = new List<RecipeConsumable>();
}
