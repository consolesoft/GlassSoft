using GlassSoft.Application.DTOs.Production;

namespace GlassSoft.Application.Interfaces;

public interface IWorkOrderService
{
    Task<IEnumerable<WorkOrderDto>> GetAllAsync();
    Task<WorkOrderDto?> GetByIdAsync(int id);
    Task<WorkOrderDto> CreateFromOrderAsync(WorkOrderCreateDto dto);
    Task CompleteAsync(int id);
    Task DeleteAsync(int id);
    Task<string> GenerateWorkOrderNumberAsync();
    Task<List<CuttingPlanDto>> RunCuttingOptimizationAsync(int workOrderId);
    Task UpdateCuttingPlanAsync(int workOrderId, int planId, CuttingPlanUpdateDto dto);
    Task<List<UnoptimizedOrderLineDto>> GetUnoptimizedOrderLinesAsync();
    Task<WorkOrderDto> CreateFromLinesAsync(WorkOrderCreateFromLinesDto dto);
}
