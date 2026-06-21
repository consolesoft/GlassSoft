using GlassSoft.Application.Licensing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize(Roles = "Admin")]
public class LicenseController(ILicenseService licenseService) : Controller
{
    public async Task<IActionResult> Index() => View(await licenseService.GetStatusAsync(HttpContext.RequestAborted));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string licenseText)
    {
        if (string.IsNullOrWhiteSpace(licenseText))
        {
            TempData["Error"] = "Lisans paketi boş olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var status = await licenseService.ActivateAsync(licenseText, HttpContext.RequestAborted);
        TempData[status.IsValid ? "Success" : "Error"] = status.Message;
        return RedirectToAction(nameof(Index));
    }
}
