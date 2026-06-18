using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;

namespace GlassSoft.Domain.Entities.Production;

public class WorkOrder : BaseEntity
{
    public string WorkOrderNumber { get; set; } = string.Empty;
    public int? OrderId { get; set; }
    public Order? Order { get; set; }
    public DateTime PlannedDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<WorkOrderOrder> Orders { get; set; } = new List<WorkOrderOrder>();
    public ICollection<WorkOrderLine> Lines { get; set; } = new List<WorkOrderLine>();
    public ICollection<CuttingPlan> CuttingPlans { get; set; } = new List<CuttingPlan>();
}
