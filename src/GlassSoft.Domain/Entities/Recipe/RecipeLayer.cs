using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;

namespace GlassSoft.Domain.Entities.Recipe;

public class RecipeLayer : BaseEntity
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int SortOrder { get; set; }             // Katman sırası (1, 2, 3...)
    public string LayerType { get; set; } = string.Empty; // "Glass", "Spacer"
    public int ProductItemId { get; set; }         // Hangi ürün (4mm Düz Cam, 12mm Çıta vs.)
    public ProductItem ProductItem { get; set; } = null!;
    public decimal ThicknessMm { get; set; }       // Kalınlık (mm)
    public int QuantityPerUnit { get; set; } = 1;  // Birim başına adet (cam=1, çıta genelde 1)
}
