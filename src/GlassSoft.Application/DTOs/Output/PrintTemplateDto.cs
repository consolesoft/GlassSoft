using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Output;

public class PrintTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OutputType OutputType { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public string OutputTypeDisplay => OutputType switch
    {
        OutputType.Siparis => "Sipariş",
        OutputType.Teslimat => "Teslimat",
        OutputType.Uretim => "Üretim",
        OutputType.CariHareketleri => "Cari Hareketleri",
        OutputType.CariEkstre => "Cari Ekstre",
        OutputType.CitaRaporu => "Çıta Raporu",
        OutputType.FiyatsizSiparis => "Fiyatsız Sipariş",
        OutputType.Etiket => "Etiket",
        _ => OutputType.ToString()
    };

    public string OutputTypeBadgeClass => OutputType switch
    {
        OutputType.Siparis => "badge-primary",
        OutputType.Teslimat => "badge-info",
        OutputType.Uretim => "badge-warning",
        OutputType.CariHareketleri => "badge-success",
        OutputType.CariEkstre => "badge-secondary",
        OutputType.CitaRaporu => "badge-dark",
        OutputType.FiyatsizSiparis => "badge-light",
        OutputType.Etiket => "badge-danger",
        _ => "badge-secondary"
    };
}

public class PrintTemplateCreateDto
{
    public string Name { get; set; } = string.Empty;
    public OutputType OutputType { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
}

public class PrintTemplateUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public OutputType OutputType { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
}

public class PrintTemplateSaveRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
}
