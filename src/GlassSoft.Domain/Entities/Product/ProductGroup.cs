using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Product;

public class ProductGroup : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductItem> Products { get; set; } = new List<ProductItem>();
}
