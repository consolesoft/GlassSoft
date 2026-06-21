using GlassSoft.Application.Services.Output;
using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GlassSoft.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

        // Roller
        string[] roles = ["Admin", "Satış", "Üretim", "Muhasebe", "SatınAlma"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole
                {
                    Name = role,
                    Description = $"{role} rolü"
                });
            }
        }

        await SeedPermissionsAsync(context, roleManager);

        // Admin kullanıcısı
        var adminEmail = configuration["BootstrapAdmin:Email"] ?? "admin@glassoft.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var adminPassword = configuration["BootstrapAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword) && environment.IsDevelopment())
                adminPassword = "Admin123!";
            if (string.IsNullOrWhiteSpace(adminPassword))
                throw new InvalidOperationException("İlk admin kullanıcısı için BootstrapAdmin:Password secret'ı tanımlanmalıdır.");

            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (!result.Succeeded)
                throw new InvalidOperationException("İlk admin kullanıcısı oluşturulamadı: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
                throw new InvalidOperationException("Admin rolü atanamadı: " + string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        }

        // Yazdırma şablonları
        await SeedPrintTemplatesAsync(context);

        // Demo verisi (ürünler, müşteriler, siparişler vb.)
        await DemoDataSeeder.SeedAsync(context);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        string[] modules = ["Dashboard", "Customers", "Sales", "Inventory", "Production", "Purchasing", "Accounting", "Reports", "Administration"];
        string[] actions = ["Read", "Create", "Update", "Delete", "Export"];

        var existing = await context.Permissions
            .Select(p => p.Module + "." + p.Action)
            .ToHashSetAsync();
        var additions = (from module in modules
                         from action in actions
                         let code = module + "." + action
                         where !existing.Contains(code)
                         select new Permission
                         {
                             Module = module,
                             Action = action,
                             Description = $"{module} modülü {action} yetkisi"
                         }).ToList();
        if (additions.Count > 0)
        {
            context.Permissions.AddRange(additions);
            await context.SaveChangesAsync();
        }

        var roleModules = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = modules,
            ["Satış"] = ["Dashboard", "Customers", "Sales", "Inventory", "Reports"],
            ["Üretim"] = ["Dashboard", "Inventory", "Production", "Reports"],
            ["Muhasebe"] = ["Dashboard", "Customers", "Accounting", "Reports"],
            ["SatınAlma"] = ["Dashboard", "Inventory", "Purchasing", "Reports"]
        };

        foreach (var (roleName, allowedModules) in roleModules)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var permissionIds = await context.Permissions
                .Where(p => allowedModules.Contains(p.Module))
                .Select(p => p.Id)
                .ToListAsync();
            var assignedIds = await context.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();
            // İlk kurulumda varsayılanları ata; yönetici sonradan yaptığı seçimi koruyabilsin.
            if (assignedIds.Count > 0) continue;
            context.RolePermissions.AddRange(permissionIds
                .Except(assignedIds)
                .Select(id => new RolePermission { RoleId = role.Id, PermissionId = id }));
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedPrintTemplatesAsync(ApplicationDbContext context)
    {
        // Eski m³ / Metre Tül içeren varsayılan şablonları güncelle (otomatik yeniden seed için)
        var oldDefaults = await context.PrintTemplates
            .Where(t => t.IsDefault && (t.HtmlContent.Contains("ToplamM3") || t.HtmlContent.Contains("m³") || t.HtmlContent.Contains("Metre Tül") || t.HtmlContent.Contains("ToplamMetreTul")))
            .ToListAsync();
        if (oldDefaults.Count > 0)
        {
            context.PrintTemplates.RemoveRange(oldDefaults);
            await context.SaveChangesAsync();
        }

        if (await context.PrintTemplates.AnyAsync(t => t.IsDefault))
            return;

        var templates = new List<PrintTemplate>
        {
            new()
            {
                Name = "Modern Sipariş Fişi",
                OutputType = OutputType.Siparis,
                IsDefault = true,
                Description = "Modern tasarım - gradient başlık, renkli vurgular",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.Siparis, "Modern")
            },
            new()
            {
                Name = "Modern Teslimat İrsaliyesi",
                OutputType = OutputType.Teslimat,
                IsDefault = true,
                Description = "Modern tasarım - teslimat irsaliyesi",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.Teslimat, "Modern")
            },
            new()
            {
                Name = "Modern Üretim Emri",
                OutputType = OutputType.Uretim,
                IsDefault = true,
                Description = "Modern tasarım - üretim/iş emri çıktısı",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.Uretim, "Modern")
            },
            new()
            {
                Name = "Modern Cari Hareket Raporu",
                OutputType = OutputType.CariHareketleri,
                IsDefault = true,
                Description = "Modern tasarım - müşteri cari hareket listesi",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.CariHareketleri, "Modern")
            },
            new()
            {
                Name = "Modern Cari Ekstre",
                OutputType = OutputType.CariEkstre,
                IsDefault = true,
                Description = "Modern tasarım - hesap ekstresi, devreden bakiye ile",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.CariEkstre, "Modern")
            },
            new()
            {
                Name = "Modern Çıta Raporu",
                OutputType = OutputType.CitaRaporu,
                IsDefault = true,
                Description = "Modern tasarım - çıta raporu, aynı ürünler birleşik",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.CitaRaporu, "Modern")
            },
            new()
            {
                Name = "Modern Fiyatsız Sipariş Fişi",
                OutputType = OutputType.FiyatsizSiparis,
                IsDefault = true,
                Description = "Modern tasarım - fiyatsız sipariş fişi",
                HtmlContent = TemplateGallery.GetTemplate(OutputType.FiyatsizSiparis, "Modern")
            }
        };

        context.PrintTemplates.AddRange(templates);
        await context.SaveChangesAsync();
    }
}
