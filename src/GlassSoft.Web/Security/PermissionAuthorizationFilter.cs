using System.Security.Claims;
using GlassSoft.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using GlassSoft.Application.Licensing;

namespace GlassSoft.Web.Security;

/// <summary>
/// MVC eylemlerini Module.Action yetki parametrelerine bağlar.
/// Yeni veya eşlenmemiş yazma eylemleri güvenli tarafta kalmak için Update kabul edilir.
/// </summary>
public sealed class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;
    private readonly ILicenseService _licenseService;

    private static readonly IReadOnlyDictionary<string, string> ControllerModules =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Home"] = "Dashboard", ["Guide"] = "Dashboard",
            ["Customers"] = "Customers",
            ["Orders"] = "Sales", ["Deliveries"] = "Sales", ["OrderFeatures"] = "Sales", ["Output"] = "Sales",
            ["Products"] = "Inventory", ["ProductGroups"] = "Inventory", ["GlassPlates"] = "Inventory",
            ["Stock"] = "Inventory", ["Recipes"] = "Inventory",
            ["WorkOrders"] = "Production",
            ["PurchaseOrders"] = "Purchasing", ["Suppliers"] = "Purchasing",
            ["Transactions"] = "Accounting", ["ChequeNotes"] = "Accounting", ["CurrencyRates"] = "Accounting",
            ["Expenses"] = "Accounting", ["CashRegisters"] = "Accounting",
            ["Reports"] = "Reports",
            ["Users"] = "Administration", ["Roles"] = "Administration", ["Settings"] = "Administration",
            ["PrintTemplates"] = "Administration", ["License"] = "Administration"
        };

    public PermissionAuthorizationFilter(ApplicationDbContext context, ILicenseService licenseService)
    {
        _context = context;
        _licenseService = licenseService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.Filters.Any(f => f is IAllowAnonymousFilter) ||
            context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            return;

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
            return;

        if (context.ActionDescriptor is not ControllerActionDescriptor action ||
            !ControllerModules.TryGetValue(action.ControllerName, out var module))
            return;

        var license = await _licenseService.GetStatusAsync(context.HttpContext.RequestAborted);
        if (action.ControllerName != "License" && module != "Dashboard" && !license.HasModule(module))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Admin rolü yetki parametrelerini aşabilir; lisans modül sınırlarını aşamaz.
        if (user.IsInRole("Admin"))
            return;

        var operation = ResolveOperation(action.ActionName, context.HttpContext.Request.Method);
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var allowed = await (
            from userRole in _context.UserRoles
            join rolePermission in _context.RolePermissions on userRole.RoleId equals rolePermission.RoleId
            join permission in _context.Permissions on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId && permission.Module == module && permission.Action == operation
            select permission.Id).AnyAsync(context.HttpContext.RequestAborted);

        if (!allowed)
            context.Result = new ForbidResult();
    }

    private static string ResolveOperation(string action, string method)
    {
        if (action.Contains("Delete", StringComparison.OrdinalIgnoreCase)) return "Delete";
        if (action.Contains("Create", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Import", StringComparison.OrdinalIgnoreCase) ||
            action is "Copy" or "SaveDraft" or "AddTransaction" or "Entry") return "Create";
        if (action.StartsWith("Download", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Export", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Print", StringComparison.OrdinalIgnoreCase)) return "Export";
        if (HttpMethods.IsGet(method) &&
            (action is "Index" or "Details" || action.StartsWith("Get", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Search", StringComparison.OrdinalIgnoreCase)))
            return "Read";
        if (HttpMethods.IsGet(method) && action is not "Edit" and not "Create") return "Read";
        return "Update";
    }
}
