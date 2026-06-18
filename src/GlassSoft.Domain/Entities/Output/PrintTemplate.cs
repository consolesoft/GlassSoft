using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Domain.Entities.Output;

public class PrintTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public OutputType OutputType { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? Description { get; set; }
}
