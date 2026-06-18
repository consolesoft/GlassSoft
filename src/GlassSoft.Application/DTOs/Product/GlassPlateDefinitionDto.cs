namespace GlassSoft.Application.DTOs.Product;

public class GlassPlateDefinitionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public decimal AreaM2 => (WidthMm * HeightMm) / 1_000_000m;
    public bool IsDefault { get; set; }
    public string DisplayName => string.IsNullOrEmpty(Name) ? $"{WidthMm:0}x{HeightMm:0} mm" : $"{Name} ({WidthMm:0}x{HeightMm:0})";
}

public class GlassPlateDefinitionCreateDto
{
    public string Name { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool IsDefault { get; set; }
}

public class GlassPlateDefinitionUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public bool IsDefault { get; set; }
}
