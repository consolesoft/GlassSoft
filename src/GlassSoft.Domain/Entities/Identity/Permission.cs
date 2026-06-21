using GlassSoft.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace GlassSoft.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    [NotMapped]
    public string Code => $"{Module}.{Action}";
}

public class RolePermission
{
    public string RoleId { get; set; } = string.Empty;
    public ApplicationRole Role { get; set; } = null!;

    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
