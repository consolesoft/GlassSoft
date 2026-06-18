using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class CurrencyRatesController : Controller
{
    private readonly ICurrencyRateService _currencyRateService;

    public CurrencyRatesController(ICurrencyRateService currencyRateService)
    {
        _currencyRateService = currencyRateService;
    }

    public async Task<IActionResult> Index()
    {
        var rates = await _currencyRateService.GetAllAsync();
        return View(rates);
    }

    public IActionResult Create()
    {
        return View(new CurrencyRateCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CurrencyRateCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _currencyRateService.CreateAsync(dto);
        TempData["Success"] = "Döviz kuru başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }
}
