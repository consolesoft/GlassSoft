using GlassSoft.Application.DTOs.Product;

namespace GlassSoft.Application.Interfaces;

public interface IStockService
{
    Task<IEnumerable<StockEntryDto>> GetEntriesByProductAsync(int productItemId);
    Task<IEnumerable<StockEntryDto>> GetRecentEntriesAsync(int count = 50);
    Task<StockEntryDto> CreateEntryAsync(StockEntryCreateDto dto);
    Task<StockEntryDto?> GetEntryByIdAsync(int id);
    Task UpdateEntryAsync(int id, StockEntryCreateDto dto);
    Task DeleteEntryAsync(int id);
    Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync();
    Task<StockSummaryDto?> GetProductStockAsync(int productItemId);

    /// <summary>
    /// Sipariş onaylandığında kalem bazında stok düşer.
    /// Tekli ürün → toplam m² stoktan çıkış yapılır.
    /// Reçeteli ürün → reçetedeki tüm katmanların stokları düşer.
    /// Stok kaydı yoksa otomatik açılır ve eksiye düşer.
    /// </summary>
    Task DeductStockForOrderAsync(int orderId);

    /// <summary>
    /// Sipariş iptal edildiğinde, o siparişe istinaden oluşturulan stok çıkışlarını siler
    /// (ReferenceType="Order", ReferenceId=orderId). Stoklar geri artar.
    /// </summary>
    Task<int> ReverseStockForOrderAsync(int orderId);

    /// <summary>
    /// Sadece sipariş kaynaklı (ReferenceType="Order") stok hareketlerini siler. Manuel hareketlere dokunmaz.
    /// </summary>
    Task<int> ResetOrderEntriesAsync();
}
