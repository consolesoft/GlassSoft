using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IOrderService _orderService;
    private readonly IStockService _stockService;
    private readonly IPrintTemplateService _templateService;
    private readonly IProductItemService _productItemService;
    private readonly IProductGroupService _productGroupService;
    private readonly IRecipeService _recipeService;

    public ReportsController(IOrderService orderService, IStockService stockService, IPrintTemplateService templateService, IProductItemService productItemService, IProductGroupService productGroupService, IRecipeService recipeService)
    {
        _orderService = orderService;
        _stockService = stockService;
        _templateService = templateService;
        _productItemService = productItemService;
        _productGroupService = productGroupService;
        _recipeService = recipeService;
    }

    // ═══════════ ÜRÜN SATIŞ RAPORU ═══════════
    public async Task<IActionResult> ProductSales(DateTime? startDate, DateTime? endDate, int? productGroupId, string? productCode)
    {
        var start = (startDate ?? TurkeyTime.Now.AddMonths(-1)).Date;
        var end = (endDate ?? TurkeyTime.Now).Date;

        // Ürünleri tek seferde çek → ProductItemId → (Code, GroupId, GroupName) haritası
        var allProducts = (await _productItemService.GetAllAsync()).ToList();
        var productMap = allProducts.ToDictionary(p => p.Id, p => (p.Code, p.Name, p.ProductGroupId, p.ProductGroupName));

        // Reçeteleri çek → RecipeId → (Code, Name, ProductItemId) haritası
        // Reçete bir ürüne bağlıysa o ürünün grup bilgisini kullanırız
        var allRecipes = (await _recipeService.GetAllAsync()).ToList();
        var recipeMap = allRecipes.ToDictionary(r => r.Id, r => (r.Code, r.Name, r.ProductItemId));

        // Taslak ve İptal hariç, teslim (kapanma) tarihi olan tüm siparişleri dahil et
        var orders = (await _orderService.GetAllAsync())
            .Where(o => o.Status != Domain.Enums.OrderStatus.Taslak
                        && o.Status != Domain.Enums.OrderStatus.IptalEdildi
                        && o.CompletedAt.HasValue
                        && o.CompletedAt.Value.Date >= start
                        && o.CompletedAt.Value.Date <= end)
            .ToList();

        var codeFilter = (productCode ?? "").Trim();

        var allLines = new List<(int OrderId, string UrunKodu, string UrunAdi, string UrunGrubu, int Quantity, decimal AreaM2)>();
        foreach (var o in orders)
        {
            var detail = await _orderService.GetByIdAsync(o.Id);
            if (detail?.Lines == null) continue;
            foreach (var l in detail.Lines)
            {
                string code, name, groupName;
                int? groupId = null;

                if (l.ProductItemId.HasValue && productMap.TryGetValue(l.ProductItemId.Value, out var pm))
                {
                    // Doğrudan ürün
                    code = pm.Code;
                    name = pm.Name;
                    groupId = pm.ProductGroupId;
                    groupName = pm.ProductGroupName;
                }
                else if (l.RecipeId.HasValue && recipeMap.TryGetValue(l.RecipeId.Value, out var rm))
                {
                    // Reçete — code/name reçeteden, grup bilgisi reçetenin bağlı olduğu üründen gelir
                    code = rm.Code;
                    name = string.IsNullOrEmpty(rm.Name) ? rm.Code : rm.Name;
                    if (rm.ProductItemId.HasValue && productMap.TryGetValue(rm.ProductItemId.Value, out var rpm))
                    {
                        groupId = rpm.ProductGroupId;
                        groupName = rpm.ProductGroupName;
                    }
                    else
                    {
                        groupName = "Reçete (Gruplandırılmamış)";
                    }
                }
                else
                {
                    code = l.RecipeCode ?? "-";
                    name = l.ProductItemName ?? l.RecipeCode ?? "Bilinmeyen";
                    groupName = "Diğer";
                }

                // Ürün grubu filtresi
                if (productGroupId.HasValue)
                {
                    if (productGroupId.Value == -1)
                    {
                        // Özel filtre: Sadece reçeteler
                        if (!l.RecipeId.HasValue) continue;
                    }
                    else if (productGroupId.Value == -2)
                    {
                        // Özel filtre: Sadece direkt ürünler (reçete olmayan)
                        if (!l.ProductItemId.HasValue || l.RecipeId.HasValue) continue;
                    }
                    else if (productGroupId.Value > 0)
                    {
                        if (!groupId.HasValue || groupId.Value != productGroupId.Value)
                            continue;
                    }
                }

                // Ürün kodu filtresi (contains, case-insensitive)
                if (codeFilter.Length > 0)
                {
                    if (code == null || code.IndexOf(codeFilter, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                var area = ((l.WidthMm ?? 0) * (l.HeightMm ?? 0)) / 1_000_000m;
                allLines.Add((o.Id, code, name, groupName, l.Quantity, area));
            }
        }

        var grouped = allLines
            .GroupBy(x => new { x.UrunKodu, x.UrunAdi, x.UrunGrubu })
            .Select(g => new ProductSalesRow
            {
                UrunKodu = g.Key.UrunKodu,
                UrunAdi = g.Key.UrunAdi,
                UrunGrubu = g.Key.UrunGrubu,
                ToplamAdet = g.Sum(x => x.Quantity),
                ToplamM2 = g.Sum(x => x.AreaM2 * x.Quantity),
                SiparisSayisi = g.Select(x => x.OrderId).Distinct().Count()
            })
            .OrderByDescending(g => g.ToplamM2)
            .ToList();

        ViewBag.StartDate = start;
        ViewBag.EndDate = end;
        ViewBag.SelectedGroupId = productGroupId;
        ViewBag.SelectedCode = productCode;
        ViewBag.ToplamSiparis = orders.Count;
        ViewBag.ToplamM2 = grouped.Sum(g => g.ToplamM2);
        ViewBag.ToplamAdet = grouped.Sum(g => g.ToplamAdet);

        // Ürün grubu dropdown'u için liste — özel filtreler de eklenir
        var groups = await _productGroupService.GetAllAsync();
        var groupOptions = new List<object>
        {
            new { Id = 0, Name = "Tüm Gruplar" },
            new { Id = -1, Name = "🧬 Sadece Reçeteler" },
            new { Id = -2, Name = "📦 Sadece Ürünler (reçete dışı)" }
        };
        groupOptions.AddRange(groups.Where(g => g.IsActive).Select(g => (object)new { Id = g.Id, Name = g.Name }));
        ViewBag.ProductGroups = new SelectList(groupOptions, "Id", "Name", productGroupId ?? 0);

        var templates = await _templateService.GetByOutputTypeAsync(OutputType.UrunSatisRaporu);
        var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault) ?? templates.FirstOrDefault();
        ViewBag.PrintTemplateId = defaultTemplate?.Id ?? 0;

        return View(grouped);
    }

    // ═══════════ SİPARİŞ DURUM RAPORU ═══════════
    public async Task<IActionResult> OrderStatus(DateTime? startDate, DateTime? endDate, Domain.Enums.OrderStatus? status, string? dateField)
    {
        var start = (startDate ?? TurkeyTime.Now.AddMonths(-1)).Date;
        var end = (endDate ?? TurkeyTime.Now).Date;
        var field = dateField ?? "siparis"; // varsayılan: sipariş tarihi

        // Tarih alanına göre filtrele
        Func<Application.DTOs.Sales.OrderDto, bool> dateFilter = field switch
        {
            "teslim" => o => o.CompletedAt.HasValue && o.CompletedAt.Value.Date >= start && o.CompletedAt.Value.Date <= end,
            "termin" => o => o.DeliveryDate.HasValue && o.DeliveryDate.Value.Date >= start && o.DeliveryDate.Value.Date <= end,
            _ => o => o.OrderDate.Date >= start && o.OrderDate.Date <= end
        };

        var orders = (await _orderService.GetAllAsync())
            .Where(dateFilter)
            .ToList();

        if (status.HasValue)
            orders = orders.Where(o => o.Status == status.Value).ToList();

        // Durum özeti (aynı tarih filtresiyle)
        var allInRange = (await _orderService.GetAllAsync())
            .Where(dateFilter)
            .ToList();

        ViewBag.StartDate = start;
        ViewBag.EndDate = end;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedDateField = field;
        ViewBag.DateFieldList = new SelectList(new[]
        {
            new { Value = "siparis", Text = "Sipariş Tarihi" },
            new { Value = "termin", Text = "Termin Tarihi" },
            new { Value = "teslim", Text = "Teslim Tarihi" }
        }, "Value", "Text", field);
        ViewBag.TaslakCount = allInRange.Count(o => o.Status == Domain.Enums.OrderStatus.Taslak);
        ViewBag.OnaylandiCount = allInRange.Count(o => o.Status == Domain.Enums.OrderStatus.Onaylandi);
        ViewBag.UretimdeCount = allInRange.Count(o => o.Status == Domain.Enums.OrderStatus.Uretimde);
        ViewBag.TamamlandiCount = allInRange.Count(o => o.Status == Domain.Enums.OrderStatus.Tamamlandi);
        ViewBag.IptalCount = allInRange.Count(o => o.Status == Domain.Enums.OrderStatus.IptalEdildi);
        ViewBag.ToplamTutar = orders.Sum(o => o.GrandTotal);

        ViewBag.StatusList = new SelectList(new[]
        {
            new { Value = "", Text = "Tüm Durumlar" },
            new { Value = "0", Text = "Taslak" },
            new { Value = "1", Text = "Onaylandı" },
            new { Value = "2", Text = "Üretimde" },
            new { Value = "3", Text = "Tamamlandı" },
            new { Value = "4", Text = "İptal Edildi" }
        }, "Value", "Text", status?.ToString("d") ?? "");

        return View(orders);
    }

    // ═══════════ STOK HAREKET RAPORU ═══════════
    public async Task<IActionResult> StockMovements(DateTime? startDate, DateTime? endDate, StockMovementType? movementType, int? productItemId)
    {
        var start = (startDate ?? TurkeyTime.Now.AddMonths(-1)).Date;
        var end = (endDate ?? TurkeyTime.Now).Date;

        var entries = (await _stockService.GetRecentEntriesAsync(10000))
            .Where(e => e.CreatedAt.Date >= start && e.CreatedAt.Date <= end)
            .ToList();

        if (movementType.HasValue)
            entries = entries.Where(e => e.MovementType == movementType.Value).ToList();

        if (productItemId.HasValue && productItemId.Value > 0)
            entries = entries.Where(e => e.ProductItemId == productItemId.Value).ToList();

        // Özetler (filtreleme sonrası — seçili ürün ve tür dahil)
        ViewBag.StartDate = start;
        ViewBag.EndDate = end;
        ViewBag.SelectedType = movementType;
        ViewBag.SelectedProductItemId = productItemId;
        ViewBag.GirisCount = entries.Count(e => e.MovementType == StockMovementType.Giris);
        ViewBag.CikisCount = entries.Count(e => e.MovementType == StockMovementType.Cikis);
        ViewBag.GirisM2 = entries.Where(e => e.MovementType == StockMovementType.Giris).Sum(e => e.Quantity);
        ViewBag.CikisM2 = entries.Where(e => e.MovementType == StockMovementType.Cikis).Sum(e => e.Quantity);

        ViewBag.TypeList = new SelectList(new[]
        {
            new { Value = "", Text = "Tüm Hareketler" },
            new { Value = "0", Text = "Giriş" },
            new { Value = "1", Text = "Çıkış" }
        }, "Value", "Text", movementType?.ToString("d") ?? "");

        // Ürün listesi dropdown
        var products = await _productItemService.GetAllAsync();
        ViewBag.ProductList = new SelectList(
            new[] { new { Id = 0, Name = "Tüm Ürünler" } }
                .Concat(products.Select(p => new { Id = p.Id, Name = p.Name })),
            "Id", "Name", productItemId ?? 0);

        return View(entries);
    }

    // ═══════════ MODEL CLASSES ═══════════
    public class ProductSalesRow
    {
        public string UrunKodu { get; set; } = string.Empty;
        public string UrunAdi { get; set; } = string.Empty;
        public string UrunGrubu { get; set; } = string.Empty;
        public int ToplamAdet { get; set; }
        public decimal ToplamM2 { get; set; }
        public int SiparisSayisi { get; set; }
    }
}
