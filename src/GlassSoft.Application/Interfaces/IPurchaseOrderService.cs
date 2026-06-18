using GlassSoft.Application.DTOs.Purchasing;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<IEnumerable<PurchaseOrderDto>> GetAllAsync();
    Task<PurchaseOrderDto?> GetByIdAsync(int id);
    Task<PurchaseOrderDto> CreateAsync(PurchaseOrderCreateDto dto);
    Task UpdateAsync(PurchaseOrderUpdateDto dto);
    Task UpdateStatusAsync(int id, PurchaseOrderStatus status);
    Task DeleteAsync(int id);
    Task<string> GenerateOrderNumberAsync();

    /// <summary>
    /// Tüm satın alma siparişlerini tarar, cari hareketlerini yeniden sıfırdan kurar.
    /// Onaylı/KısmiTeslim/Tamamlandı için sipariş tutarına eşit cari borç hareketi olur.
    /// Taslak/İptal için ilgili cari hareket varsa silinir.
    /// </summary>
    /// <returns>(updatedCount, deletedCount, processedCount, errors)</returns>
    Task<(int Updated, int Deleted, int Processed, List<string> Errors)> BackfillTransactionsAsync();
}
