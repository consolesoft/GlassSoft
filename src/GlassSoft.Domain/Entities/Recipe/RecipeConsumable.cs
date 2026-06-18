using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;

namespace GlassSoft.Domain.Entities.Recipe;

public class RecipeConsumable : BaseEntity
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int ProductItemId { get; set; }         // Sarf malzeme (Butyl, Polisülfür vs.)
    public ProductItem ProductItem { get; set; } = null!;
    public string ConsumptionFormula { get; set; } = string.Empty; // "PERIMETER * 0.01" gibi formül
    public string? Description { get; set; }
}
