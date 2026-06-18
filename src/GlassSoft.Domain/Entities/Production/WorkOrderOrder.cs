using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;

namespace GlassSoft.Domain.Entities.Production;

public class WorkOrderOrder : BaseEntity
{
    public int WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
