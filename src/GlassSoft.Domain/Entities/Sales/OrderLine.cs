using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Recipe;

namespace GlassSoft.Domain.Entities.Sales;

public class OrderLine : BaseEntity
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int? ProductItemId { get; set; }        // Tekli ürün seçimi
    public ProductItem? ProductItem { get; set; }
    public int? RecipeId { get; set; }             // Reçeteli ürün seçimi
    public Recipe.Recipe? Recipe { get; set; }
    public decimal? WidthMm { get; set; }           // En (mm) - null ise perakende satış
    public decimal? HeightMm { get; set; }          // Boy (mm) - null ise perakende satış
    public int Quantity { get; set; }              // Adet
    public string? PozNo { get; set; }              // Poz numarası (opsiyonel)
    public decimal UnitPrice { get; set; }         // Birim fiyat (m² veya adet başına)
    public decimal TotalPrice { get; set; }        // Toplam fiyat
    public bool IsRetail => !WidthMm.HasValue || !HeightMm.HasValue || (WidthMm == 0 && HeightMm == 0);
    public decimal AreaM2 => IsRetail ? 0 : ((WidthMm!.Value * HeightMm!.Value) / 1_000_000m);
    public decimal TotalAreaM2 => AreaM2 * Quantity;
    public bool IsOptimized { get; set; }          // Kesim optimizasyonu yapıldı mı
    public string? Notes { get; set; }
    public DateTime? LabelPrintedAt { get; set; }  // Etiket yazdırılma zamanı

    public ICollection<OrderLineFeature> Features { get; set; } = new List<OrderLineFeature>();
}
