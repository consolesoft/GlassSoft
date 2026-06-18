using GlassSoft.Application.DTOs.Output;
using GlassSoft.Application.Interfaces;
using GlassSoft.Application.Services.Output;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class PrintTemplatesController : Controller
{
    private readonly IPrintTemplateService _service;

    public PrintTemplatesController(IPrintTemplateService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var templates = await _service.GetAllAsync();
        return View(templates);
    }

    public IActionResult Create()
    {
        return View(new PrintTemplateCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrintTemplateCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _service.CreateAsync(dto);
        TempData["Success"] = "Şablon başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var template = await _service.GetByIdAsync(id);
        if (template == null) return NotFound();

        var dto = new PrintTemplateUpdateDto
        {
            Id = template.Id,
            Name = template.Name,
            OutputType = template.OutputType,
            HtmlContent = template.HtmlContent,
            IsDefault = template.IsDefault,
            Description = template.Description
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PrintTemplateUpdateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        await _service.UpdateAsync(dto);
        TempData["Success"] = "Şablon başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Şablon silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(int id)
    {
        var copy = await _service.CopyAsync(id);
        TempData["Success"] = $"Şablon kopyalandı: {copy.Name}";
        return RedirectToAction(nameof(Edit), new { id = copy.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateDefaults()
    {
        var existing = await _service.GetAllAsync();
        var existingTypes = existing.Select(t => t.OutputType).Distinct().ToHashSet();
        int count = 0;

        var typeNames = new Dictionary<OutputType, string>
        {
            [OutputType.Siparis] = "Sipariş Fişi",
            [OutputType.Teslimat] = "Teslimat İrsaliyesi",
            [OutputType.Uretim] = "Üretim Emri",
            [OutputType.CariHareketleri] = "Cari Hareket Raporu",
            [OutputType.CariEkstre] = "Hesap Ekstresi",
            [OutputType.CitaRaporu] = "Çıta Raporu",
            [OutputType.FiyatsizSiparis] = "Fiyatsız Sipariş Fişi",
            [OutputType.Etiket] = "Etiket"
        };

        foreach (var (type, name) in typeNames)
        {
            foreach (var style in TemplateGallery.Styles)
            {
                var templateName = $"{style} {name}";
                if (existing.Any(t => t.OutputType == type && t.Name == templateName))
                    continue;

                await _service.CreateAsync(new PrintTemplateCreateDto
                {
                    Name = templateName,
                    OutputType = type,
                    IsDefault = style == "Modern" && !existingTypes.Contains(type),
                    Description = $"{style} tasarım - {name.ToLower()}",
                    HtmlContent = TemplateGallery.GetTemplate(type, style)
                });
                count++;
            }
            existingTypes.Add(type);
        }

        TempData["Success"] = count > 0
            ? $"{count} varsayılan şablon oluşturuldu."
            : "Tüm varsayılan şablonlar zaten mevcut.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDefaults()
    {
        var existing = (await _service.GetAllAsync()).ToList();
        int updated = 0, created = 0;

        var typeNames = new Dictionary<OutputType, string>
        {
            [OutputType.Siparis] = "Sipariş Fişi",
            [OutputType.Teslimat] = "Teslimat İrsaliyesi",
            [OutputType.Uretim] = "Üretim Emri",
            [OutputType.CariHareketleri] = "Cari Hareket Raporu",
            [OutputType.CariEkstre] = "Hesap Ekstresi",
            [OutputType.CitaRaporu] = "Çıta Raporu",
            [OutputType.FiyatsizSiparis] = "Fiyatsız Sipariş Fişi",
            [OutputType.Etiket] = "Etiket"
        };

        foreach (var (type, name) in typeNames)
        {
            foreach (var style in TemplateGallery.Styles)
            {
                var templateName = $"{style} {name}";
                var html = TemplateGallery.GetTemplate(type, style);
                var match = existing.FirstOrDefault(t => t.OutputType == type && t.Name == templateName);

                if (match != null)
                {
                    await _service.UpdateAsync(new PrintTemplateUpdateDto
                    {
                        Id = match.Id,
                        Name = match.Name,
                        OutputType = match.OutputType,
                        HtmlContent = html,
                        IsDefault = match.IsDefault,
                        Description = match.Description
                    });
                    updated++;
                }
                else
                {
                    await _service.CreateAsync(new PrintTemplateCreateDto
                    {
                        Name = templateName,
                        OutputType = type,
                        IsDefault = style == "Modern" && !existing.Any(t => t.OutputType == type && t.IsDefault),
                        Description = $"{style} tasarım - {name.ToLower()}",
                        HtmlContent = html
                    });
                    created++;
                }
            }
        }

        TempData["Success"] = $"Varsayılan şablonlar güncellendi. ({updated} güncellendi, {created} yeni oluşturuldu)";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> LabelDesigner(int? id)
    {
        PrintTemplateUpdateDto? dto = null;
        if (id.HasValue)
        {
            var template = await _service.GetByIdAsync(id.Value);
            if (template != null)
            {
                dto = new PrintTemplateUpdateDto
                {
                    Id = template.Id,
                    Name = template.Name,
                    OutputType = template.OutputType,
                    HtmlContent = template.HtmlContent,
                    IsDefault = template.IsDefault,
                    Description = template.Description
                };
            }
        }
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> SaveLabel([FromBody] PrintTemplateSaveRequest request)
    {
        if (request.Id > 0)
        {
            await _service.UpdateAsync(new PrintTemplateUpdateDto
            {
                Id = request.Id,
                Name = request.Name,
                OutputType = OutputType.Etiket,
                HtmlContent = request.HtmlContent,
                IsDefault = request.IsDefault,
                Description = request.Description
            });
            return Json(new { success = true, id = request.Id });
        }
        else
        {
            var created = await _service.CreateAsync(new PrintTemplateCreateDto
            {
                Name = request.Name,
                OutputType = OutputType.Etiket,
                HtmlContent = request.HtmlContent,
                IsDefault = request.IsDefault,
                Description = request.Description
            });
            return Json(new { success = true, id = created.Id });
        }
    }

    [HttpGet]
    public IActionResult Preview(int id)
    {
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public IActionResult Placeholders(OutputType type)
    {
        var placeholders = type switch
        {
            OutputType.Siparis or OutputType.Teslimat or OutputType.FiyatsizSiparis => new[]
            {
                "SiparisNo", "SiparisTarihi", "TeslimTarihi", "TeslimatTarihi", "Durum",
                "MusteriAdi", "MusteriKodu", "MusteriAdres", "MusteriSehir", "MusteriTelefon", "MusteriEmail",
                "MusteriVergiNo", "MusteriVergiDairesi",
                "ParaBirimi", "ToplamTutar", "Notlar", "ToplamAdet", "ToplamAlan",
                "CariBakiye", "CariBakiyeYon",
                "Kalemler", "Tarih", "Saat"
            },
            OutputType.CitaRaporu => new[]
            {
                "SiparisNo", "SiparisTarihi", "TeslimTarihi", "Durum",
                "MusteriAdi", "MusteriKodu", "MusteriAdres", "MusteriSehir", "MusteriTelefon", "MusteriEmail",
                "MusteriVergiNo", "MusteriVergiDairesi",
                "Notlar", "ToplamAdet", "ToplamAlan",
                "CariBakiye", "CariBakiyeYon",
                "Kalemler", "Tarih", "Saat"
            },
            OutputType.Uretim => new[]
            {
                "IsEmriNo", "IsEmriTarihi", "Durum",
                "MusteriAdi", "SiparisNolari", "Notlar",
                "ToplamAdet", "ToplamAlan", "Kalemler", "Tarih", "Saat"
            },
            OutputType.CariHareketleri or OutputType.CariEkstre => new[]
            {
                "MusteriAdi", "MusteriKodu", "MusteriAdres", "MusteriSehir", "MusteriTelefon", "MusteriEmail",
                "MusteriVergiNo", "MusteriVergiDairesi",
                "BaslangicTarihi", "BitisTarihi", "EkstreTarihi",
                "DevredenBakiye", "DevredenYon",
                "ToplamBorc", "ToplamAlacak", "Bakiye", "BakiyeYon",
                "Hareketler", "Tarih", "Saat"
            },
            OutputType.Etiket => new[]
            {
                "FirmaAdi", "FirmaLogo", "SiparisNo", "SiparisTarihi", "TeslimTarihi",
                "MusteriAdi", "MusteriKodu",
                "UrunAdi", "PozNo", "En", "Boy", "Olcu", "Alan", "Adet",
                "Ozellikler", "Notlar", "KalemNo", "Tarih", "Saat"
            },
            _ => Array.Empty<string>()
        };

        return Json(placeholders);
    }

    /// <summary>
    /// Hazır şablon galerisi - stil listesi döndürür
    /// </summary>
    [HttpGet]
    public IActionResult GalleryStyles(OutputType type)
    {
        var items = TemplateGallery.GetGalleryItems(type);
        return Json(items);
    }

    /// <summary>
    /// Seçilen hazır şablonun HTML'ini döndürür
    /// </summary>
    [HttpGet]
    public IActionResult GalleryTemplate(OutputType type, string style)
    {
        var html = TemplateGallery.GetTemplate(type, style);
        return Content(html, "text/html");
    }
}
