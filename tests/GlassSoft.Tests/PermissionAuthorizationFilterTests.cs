using System.Security.Claims;
using GlassSoft.Application.Licensing;
using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Infrastructure.Data;
using GlassSoft.Web.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Tests;

public class PermissionAuthorizationFilterTests
{
    [Fact]
    public async Task ReadPermission_AllowsRead_ButForbidsUpdate()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        await using var context = new ApplicationDbContext(options);
        const string userId = "user-1";
        const string roleId = "role-1";
        context.Users.Add(new ApplicationUser { Id = userId, UserName = "sales@test.local", FirstName = "Test", LastName = "Sales" });
        context.Roles.Add(new ApplicationRole { Id = roleId, Name = "Satış", NormalizedName = "SATIŞ" });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = userId, RoleId = roleId });
        var permission = new Permission { Module = "Customers", Action = "Read" };
        context.Permissions.Add(permission);
        await context.SaveChangesAsync();
        context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permission.Id });
        await context.SaveChangesAsync();

        var filter = new PermissionAuthorizationFilter(context, new ValidLicenseService());
        var readContext = CreateContext(userId, "Index", HttpMethods.Get);
        await filter.OnAuthorizationAsync(readContext);
        Assert.Null(readContext.Result);

        var updateContext = CreateContext(userId, "Edit", HttpMethods.Post);
        await filter.OnAuthorizationAsync(updateContext);
        Assert.IsType<ForbidResult>(updateContext.Result);
    }

    private static AuthorizationFilterContext CreateContext(string userId, string actionName, string method)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Satış")
        ], "test"));
        var descriptor = new ControllerActionDescriptor { ControllerName = "Customers", ActionName = actionName };
        return new AuthorizationFilterContext(new ActionContext(httpContext, new(), descriptor), []);
    }

    private sealed class ValidLicenseService : ILicenseService
    {
        private static readonly LicenseStatus Status = new() { IsValid = true, IsDevelopmentBypass = true };
        public Task<LicenseStatus> GetStatusAsync(CancellationToken cancellationToken = default) => Task.FromResult(Status);
        public Task<LicenseStatus> ValidateAsync(string licenseText, CancellationToken cancellationToken = default) => Task.FromResult(Status);
        public Task<LicenseStatus> ActivateAsync(string licenseText, CancellationToken cancellationToken = default) => Task.FromResult(Status);
    }
}
