using GlassSoft.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace GlassSoft.Web.Security;

/// <summary>
/// Sadece Development ortamında çalışır. Giriş yapılmamış istekleri otomatik olarak
// bootstrap admin kullanıcısı ile imzalar. Üretimde kullanılmamalıdır.
/// </summary>
public sealed class DevelopmentAutoLoginMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment() &&
            context.User.Identity?.IsAuthenticated != true &&
            !IsAnonymousAllowed(context))
        {
            var adminEmail = configuration["BootstrapAdmin:Email"] ?? "admin@glassoft.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null && adminUser.IsActive)
            {
                await signInManager.SignInAsync(adminUser, isPersistent: false);
                // Kullanıcı bilgisi yeniden oluşturulsun
                context.User = await signInManager.CreateUserPrincipalAsync(adminUser);
            }
        }

        await next(context);
    }

    private static bool IsAnonymousAllowed(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null) return false;

        return endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;
    }
}
