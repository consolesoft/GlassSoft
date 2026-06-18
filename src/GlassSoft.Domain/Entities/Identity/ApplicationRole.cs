using GlassSoft.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace GlassSoft.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = TurkeyTime.Now;
}
