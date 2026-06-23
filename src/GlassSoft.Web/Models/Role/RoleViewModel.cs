using System.ComponentModel.DataAnnotations;

namespace GlassSoft.Web.Models.Role;

public class RoleListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
}

public class RoleCreateViewModel
{
    [Required(ErrorMessage = "Rol adı gereklidir.")]
    [Display(Name = "Rol Adı")]
    [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    public string? Description { get; set; }
}

public class RoleEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Rol adı gereklidir.")]
    [Display(Name = "Rol Adı")]
    [StringLength(50, ErrorMessage = "Rol adı en fazla 50 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Açıklama")]
    [StringLength(200, ErrorMessage = "Açıklama en fazla 200 karakter olabilir.")]
    public string? Description { get; set; }

    public List<int> SelectedPermissionIds { get; set; } = new();
    public List<PermissionGroupViewModel> PermissionGroups { get; set; } = new();
}

public class PermissionGroupViewModel
{
    public string ModuleCode { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public List<PermissionOptionViewModel> Permissions { get; set; } = new();
}

public class PermissionOptionViewModel
{
    public int Id { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
