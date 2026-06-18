using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Product;

namespace GlassSoft.Domain.Entities.Production;

public class CuttingPlan : BaseEntity
{
    public int WorkOrderId { get; set; }
    public WorkOrder WorkOrder { get; set; } = null!;
    public int ProductItemId { get; set; }            // Cam türü (4mm Düz, 4mm Füme vs.)
    public ProductItem ProductItem { get; set; } = null!;
    public int GlassPlateDefinitionId { get; set; }   // Kullanılan plaka boyutu
    public GlassPlateDefinition GlassPlateDefinition { get; set; } = null!;
    public int PlateIndex { get; set; }               // Kaçıncı plaka (aynı türden birden fazla olabilir)
    public decimal WastePercentage { get; set; }      // Fire oranı (%)
    public decimal UsedAreaM2 { get; set; }           // Kullanılan alan m²
    public decimal WasteAreaM2 { get; set; }          // Fire alanı m²
    public string? CsvOutput { get; set; }            // Makine çıktısı (CSV)
    public string? DxfOutput { get; set; }            // Makine çıktısı (DXF)
    public bool IsApproved { get; set; }

    public ICollection<CuttingPlanItem> Items { get; set; } = new List<CuttingPlanItem>();
}
