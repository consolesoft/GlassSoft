using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Sales;

/// <summary>
/// Sipariş kalemi özellik tanımları (Temperli, Lamine, Buzlu cam vb.)
/// Birim fiyatı olan ve sipariş kalemlerine eklenebilen parametreler.
/// </summary>
public class OrderFeatureDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
