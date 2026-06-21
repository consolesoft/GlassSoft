using GlassSoft.Application.Licensing;

namespace GlassSoft.Web.Security;

public sealed class LicenseMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILicenseService licenseService)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments("/License") || path.StartsWithSegments("/Account") ||
            path.StartsWithSegments("/css") || path.StartsWithSegments("/js") || path.StartsWithSegments("/lib") ||
            path.StartsWithSegments("/favicon.ico"))
        {
            await next(context);
            return;
        }

        var status = await licenseService.GetStatusAsync(context.RequestAborted);
        if (status.IsValid)
        {
            await next(context);
            return;
        }

        if (context.Request.Headers.Accept.Any(v => v?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true))
        {
            context.Response.Redirect("/License");
            return;
        }

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new { error = "license_invalid", message = status.Message }, context.RequestAborted);
    }
}
