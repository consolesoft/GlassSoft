using GlassSoft.Application.Services.Output;
using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GlassSoft.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

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

        // Admin kullanıcısı
        var adminEmail = "admin@glassoft.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Sistem",
                LastName = "Admin",
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Yazdırma şablonları
        await SeedPrintTemplatesAsync(context);
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
