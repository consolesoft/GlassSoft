using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ISystemSettingService _settingService;
    private readonly ICashRegisterService _cashRegisterService;

    public SettingsController(ISystemSettingService settingService, ICashRegisterService cashRegisterService)
    {
        _settingService = settingService;
        _cashRegisterService = cashRegisterService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.MinM2 = await _settingService.GetValueAsync("MinM2") ?? "0";
        ViewBag.DefaultDeliveryDays = await _settingService.GetValueAsync("DefaultDeliveryDays") ?? "14";
        ViewBag.ProductCodePrefix = await _settingService.GetValueAsync("ProductCodePrefix") ?? "URN";
        ViewBag.CustomerCodePrefix = await _settingService.GetValueAsync("CustomerCodePrefix") ?? "CRI";

        // Firma bilgileri
        ViewBag.CompanyName = await _settingService.GetValueAsync("CompanyName") ?? "";
        ViewBag.CompanySlogan = await _settingService.GetValueAsync("CompanySlogan") ?? "";
        ViewBag.CompanyAddress = await _settingService.GetValueAsync("CompanyAddress") ?? "";
        ViewBag.CompanyCity = await _settingService.GetValueAsync("CompanyCity") ?? "";
        ViewBag.CompanyPhone = await _settingService.GetValueAsync("CompanyPhone") ?? "";
        ViewBag.CompanyEmail = await _settingService.GetValueAsync("CompanyEmail") ?? "";
        ViewBag.CompanyTaxNo = await _settingService.GetValueAsync("CompanyTaxNo") ?? "";
        ViewBag.CompanyTaxOffice = await _settingService.GetValueAsync("CompanyTaxOffice") ?? "";
        ViewBag.CompanyLogo = await _settingService.GetValueAsync("CompanyLogo") ?? "";

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string minM2, string defaultDeliveryDays, string productCodePrefix, string customerCodePrefix,
        string companyName, string companySlogan, string companyAddress, string companyCity,
        string companyPhone, string companyEmail, string companyTaxNo, string companyTaxOffice)
    {
        await _settingService.SetValueAsync("MinM2", minM2 ?? "0", "Sipariş kalemi minimum m² değeri");
        await _settingService.SetValueAsync("DefaultDeliveryDays", defaultDeliveryDays ?? "14", "Sipariş onayında otomatik termin gün sayısı");
        await _settingService.SetValueAsync("ProductCodePrefix", productCodePrefix ?? "URN", "Ürün kodu ön eki");
        await _settingService.SetValueAsync("CustomerCodePrefix", customerCodePrefix ?? "CRI", "Cari kodu ön eki");

        await _settingService.SetValueAsync("CompanyName", companyName ?? "", "Firma adı (çıktılarda kullanılır)");
        await _settingService.SetValueAsync("CompanySlogan", companySlogan ?? "", "Firma sloganı / alt başlık");
        await _settingService.SetValueAsync("CompanyAddress", companyAddress ?? "", "Firma adresi");
        await _settingService.SetValueAsync("CompanyCity", companyCity ?? "", "Firma şehir");
        await _settingService.SetValueAsync("CompanyPhone", companyPhone ?? "", "Firma telefon");
        await _settingService.SetValueAsync("CompanyEmail", companyEmail ?? "", "Firma e-posta");
        await _settingService.SetValueAsync("CompanyTaxNo", companyTaxNo ?? "", "Firma vergi no");
        await _settingService.SetValueAsync("CompanyTaxOffice", companyTaxOffice ?? "", "Firma vergi dairesi");

        TempData["Success"] = "Sistem ayarları kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadLogo(IFormFile logoFile)
    {
        if (logoFile == null || logoFile.Length == 0)
        {
            TempData["Error"] = "Dosya seçilmedi.";
            return RedirectToAction(nameof(Index));
        }

        var ext = Path.GetExtension(logoFile.FileName).ToLower();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".svg")
        {
            TempData["Error"] = "Sadece PNG, JPG veya SVG dosyaları kabul edilir.";
            return RedirectToAction(nameof(Index));
        }

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var fileName = "company_logo" + ext;
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await logoFile.CopyToAsync(stream);
        }

        var logoUrl = "/uploads/" + fileName;
        await _settingService.SetValueAsync("CompanyLogo", logoUrl, "Firma logosu URL");

        TempData["Success"] = "Logo başarıyla yüklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecalculateCashBalances(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction(nameof(Index));
        }

        var (registerCount, details) = await _cashRegisterService.RecalculateAllBalancesAsync();

        if (details.Count == 0)
        {
            TempData["Success"] = $"{registerCount} kasa kontrol edildi. Tüm bakiyeler zaten doğru, düzeltme yapılmadı.";
        }
        else
        {
            var detailLines = details.Select(d => $"{d.Name}: {d.OldBalance:N2} → {d.NewBalance:N2}");
            TempData["Success"] = $"{registerCount} kasa kontrol edildi, {details.Count} kasa düzeltildi: {string.Join(" | ", detailLines)}";
        }
        return RedirectToAction(nameof(Index));
    }
}
