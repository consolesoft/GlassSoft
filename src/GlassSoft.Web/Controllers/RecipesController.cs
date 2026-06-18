using GlassSoft.Application.DTOs.Recipe;
using GlassSoft.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class RecipesController : Controller
{
    private readonly IRecipeService _service;
    private readonly IProductItemService _productService;

    public RecipesController(IRecipeService service, IProductItemService productService)
    {
        _service = service;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var recipes = await _service.GetAllAsync();
        return View(recipes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var recipe = await _service.GetByIdAsync(id);
        if (recipe == null) return NotFound();
        return View(recipe);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectLists();
        return View(new RecipeCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] RecipeCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _service.CreateAsync(dto);
        TempData["Success"] = "Reçete başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await _service.GetByIdAsync(id);
        if (recipe == null) return NotFound();
        await LoadSelectLists();
        return View(new RecipeUpdateDto
        {
            Id = recipe.Id,
            Code = recipe.Code,
            Name = recipe.Name,
            Description = recipe.Description,
            BaseUnitPrice = recipe.BaseUnitPrice,
            IsActive = recipe.IsActive,
            ProductItemId = recipe.ProductItemId,
            Layers = recipe.Layers.Select(l => new RecipeLayerCreateDto
            {
                SortOrder = l.SortOrder,
                LayerType = l.LayerType,
                ProductItemId = l.ProductItemId,
                ThicknessMm = l.ThicknessMm,
                QuantityPerUnit = l.QuantityPerUnit
            }).ToList(),
            Consumables = recipe.Consumables.Select(c => new RecipeConsumableCreateDto
            {
                ProductItemId = c.ProductItemId,
                ConsumptionFormula = c.ConsumptionFormula,
                Description = c.Description
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] RecipeUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _service.UpdateAsync(dto);
        TempData["Success"] = "Reçete başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        TempData["Success"] = "Reçete başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectLists()
    {
        var allProducts = await _productService.GetAllAsync();
        var activeProducts = allProducts.Where(p => p.IsActive);
        ViewBag.Products = new SelectList(activeProducts, "Id", "Name");
        ViewBag.GlassProducts = new SelectList(activeProducts.Where(p => p.IsPlate || p.UnitDisplay == "m²"), "Id", "Name");
    }
}
