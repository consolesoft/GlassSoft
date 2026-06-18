using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status);
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(OrderCreateDto dto);
    Task UpdateAsync(OrderUpdateDto dto);
    Task UpdateStatusAsync(int id, OrderStatus status);
    Task DeleteAsync(int id);
    Task SetDeliveryDateAsync(int id, DateTime date);
    Task SetCompletedAtAsync(int id, DateTime date);
    Task<string> GenerateOrderNumberAsync();
    Task<IEnumerable<OrderLinePriceHistoryDto>> GetPriceHistoryAsync(int? productItemId, int? recipeId, int count = 10);
    Task<OrderLineDto?> GetLineByIdAsync(int lineId);
    Task SetLabelPrintedAsync(int lineId);
}
