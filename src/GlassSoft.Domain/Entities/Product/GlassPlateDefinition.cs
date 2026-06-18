using GlassSoft.Domain.Common;

namespace GlassSoft.Domain.Entities.Product;

public class GlassPlateDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty; // Plaka adı (örn: "3210x2250 Standart")
    public decimal WidthMm { get; set; }  // Plaka eni (mm)
    public decimal HeightMm { get; set; } // Plaka boyu (mm)
    public decimal AreaM2 => (WidthMm * HeightMm) / 1_000_000m; // m² hesabı
    public bool IsDefault { get; set; } // Varsayılan plaka boyutu mu

    public string DisplayName => string.IsNullOrEmpty(Name) ? $"{WidthMm:0}x{HeightMm:0} mm" : $"{Name} ({WidthMm:0}x{HeightMm:0})";
}
