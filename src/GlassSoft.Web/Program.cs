using System.Globalization;
using GlassSoft.Infrastructure;
using GlassSoft.Infrastructure.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using GlassSoft.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<PermissionAuthorizationFilter>();
builder.Services.AddControllersWithViews(options =>
    options.Filters.AddService<PermissionAuthorizationFilter>());
builder.Services.AddInfrastructure(builder.Configuration);

// Model binding için InvariantCulture kullan (HTML number input nokta gönderir)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var culture = new CultureInfo("tr-TR");
    culture.NumberFormat.NumberDecimalSeparator = ".";
    culture.NumberFormat.NumberGroupSeparator = ",";
    options.DefaultRequestCulture = new RequestCulture(culture);
    options.SupportedCultures = [culture];
    options.SupportedUICultures = [culture];
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseRouting();
app.UseRequestLocalization();
app.UseStaticFiles();

app.UseMiddleware<LicenseMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
