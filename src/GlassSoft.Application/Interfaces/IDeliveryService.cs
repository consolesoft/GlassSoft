using GlassSoft.Application.DTOs.Sales;

namespace GlassSoft.Application.Interfaces;

public interface IDeliveryService
{
    Task<IEnumerable<DeliveryDto>> GetAllAsync();
    Task<DeliveryDto?> GetByIdAsync(int id);
    Task<IEnumerable<DeliveryDto>> GetByOrderIdAsync(int orderId);

    Task<IEnumerable<OpenOrderDto>> GetOpenOrdersAsync();
    Task<IEnumerable<OpenOrderLineDto>> GetOpenLinesForOrderAsync(int orderId, int? excludeDeliveryId = null);

    Task<DeliveryDto> CreateAsync(DeliveryCreateDto dto);
    Task UpdateAsync(DeliveryUpdateDto dto);
    Task DeleteAsync(int id);

    Task ApproveAsync(int id);
    Task UnapproveAsync(int id);

    /// <summary>
    /// Sipariş için kalan tüm açık miktarları kapsayan ONAYLI bir teslimat oluşturur.
    /// Açık kalem yoksa null döner. Sipariş durumu Tamamlandı'ya geçirilmez burada.
    /// </summary>
    Task<DeliveryDto?> CreateFullDeliveryForOrderAsync(int orderId);

    /// <summary>
    /// Sipariş için tüm onaylı teslimatlardaki belirli bir kalemin toplam teslim edilen miktarı.
    /// </summary>
    Task<int> GetDeliveredQuantityAsync(int orderLineId);

    /// <summary>
    /// Belirli bir kalemin onaylı teslimatlarda kullanılıyor mu (silinmesini engellemek için)?
    /// </summary>
    Task<bool> IsOrderLineUsedInApprovedDeliveryAsync(int orderLineId);

    /// <summary>
    /// Sipariş için onaylı teslimat var mı (sipariş iptalini engellemek için)?
    /// </summary>
    Task<bool> HasApprovedDeliveryAsync(int orderId);

    /// <summary>
    /// Sipariş düzenlendikten sonra teslimat satırlarını uyumlu hale getirir:
    /// - Sipariş kalemi silinmişse → ilgili teslimat satırını sil
    /// - Teslimat miktarı sipariş miktarını aşıyorsa → teslimat miktarını sipariş miktarına düşür
    /// </summary>
    /// <returns>Etkilenen teslimat satırı sayısı</returns>
    Task<int> ReconcileWithOrderAsync(int orderId);
}

public interface IDeliveryNumberGenerator
{
    Task<string> GenerateAsync();
}
