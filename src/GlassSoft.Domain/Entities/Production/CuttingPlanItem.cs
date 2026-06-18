using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;

namespace GlassSoft.Domain.Entities.Production;

public class CuttingPlanItem : BaseEntity
{
    public int CuttingPlanId { get; set; }
    public CuttingPlan CuttingPlan { get; set; } = null!;
    public int? OrderLineId { get; set; }          // Hangi sipariş kalemine ait
    public OrderLine? OrderLine { get; set; }
    public decimal X { get; set; }                 // Plaka üzerinde X koordinat (mm)
    public decimal Y { get; set; }                 // Plaka üzerinde Y koordinat (mm)
    public decimal WidthMm { get; set; }           // Parça eni (mm)
    public decimal HeightMm { get; set; }          // Parça boyu (mm)
    public bool IsRotated { get; set; }            // Döndürülmüş mü
}
