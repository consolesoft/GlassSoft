namespace GlassSoft.Application.Interfaces;

public interface IOutputRenderService
{
    Task<string> RenderOrderAsync(int orderId, int templateId);
    Task<string> RenderDeliveryAsync(int orderId, int templateId);
    Task<string> RenderDeliveryFromDeliveryAsync(int deliveryId, int templateId, bool priceless = false);
    Task<string> RenderWorkOrderAsync(int workOrderId, int templateId);
    Task<string> RenderAccountTransactionsAsync(int customerId, int templateId, DateTime? startDate, DateTime? endDate);
    Task<string> RenderAccountStatementAsync(int customerId, int templateId, DateTime? startDate, DateTime? endDate);
    Task<string> RenderCitaReportAsync(int orderId, int templateId);
    Task<string> RenderPricelessOrderAsync(int orderId, int templateId);
    Task<string> RenderLabelAsync(int orderId, int lineId, int templateId, int copyIndex = 1, int? copyTotalOverride = null);
    Task<string> RenderLabelsAsync(int orderId, int[] lineIds, int templateId);
    Task<string> RenderProductSalesReportAsync(int templateId, DateTime startDate, DateTime endDate);
    Task<string> RenderBalanceListAsync(int templateId);
}
