using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IProductItemService _productService;
    private readonly IRecipeService _recipeService;
    private readonly IExcelImportService _excelService;
    private readonly IAccountTransactionService _transactionService;
    private readonly ISystemSettingService _settingService;
    private readonly IStockService _stockService;
    private readonly IDeliveryService _deliveryService;
    private readonly IRepository<OrderFeatureDefinition> _featureDefRepo;

    public OrdersController(
        IOrderService orderService,
        ICustomerService customerService,
        IProductItemService productService,
        IRecipeService recipeService,
        IExcelImportService excelService,
        IAccountTransactionService transactionService,
        ISystemSettingService settingService,
        IStockService stockService,
        IDeliveryService deliveryService,
        IRepository<OrderFeatureDefinition> featureDefRepo)
    {
        _orderService = orderService;
        _customerService = customerService;
        _productService = productService;
        _recipeService = recipeService;
        _excelService = excelService;
        _transactionService = transactionService;
        _settingService = settingService;
        _deliveryService = deliveryService;
        _stockService = stockService;
        _featureDefRepo = featureDefRepo;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();
        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();

        // Açık kalan kalem bilgisini view'a ilet (manuel "Tamamla" butonu + uyarı için)
        var openLines = (await _deliveryService.GetOpenLinesForOrderAsync(id)).ToList();
        ViewBag.OpenLineCount = openLines.Count;
        ViewBag.OpenRemainingTotal = openLines.Sum(l => l.RemainingQuantity);
        ViewBag.HasOpenLines = openLines.Any();

        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectLists();
        return View(new OrderCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] OrderCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        await _orderService.CreateAsync(dto);
        TempData["Success"] = "Sipariş başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        await LoadSelectLists();
        ViewBag.HasApprovedDelivery = await _deliveryService.HasApprovedDeliveryAsync(id);
        return View(new OrderUpdateDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            CompletedAt = order.CompletedAt,
            Currency = order.Currency,
            TaxRate = order.TaxRate,
            Notes = order.Notes,
            OrderCustomerName = order.OrderCustomerName,
            Lines = order.Lines.Select(l => new OrderLineCreateDto
            {
                Id = l.Id, // ID korunur — ID-based UpdateAsync için kritik
                ProductItemId = l.ProductItemId,
                RecipeId = l.RecipeId,
                PozNo = l.PozNo,
                WidthMm = l.WidthMm,
                HeightMm = l.HeightMm,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Notes = l.Notes,
                Features = l.Features.Select(f => new OrderLineFeatureCreateDto
                {
                    FeatureDefinitionId = f.FeatureDefinitionId,
                    Quantity = f.Quantity,
                    UnitPrice = f.UnitPrice
                }).ToList()
            }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] OrderUpdateDto dto)
    {
        // Boş satırları temizle (ürün/reçete seçilmemiş)
        dto.Lines = dto.Lines.Where(l => l.ProductItemId.HasValue || l.RecipeId.HasValue).ToList();

        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }
        try
        {
            // Güncelleme öncesi mevcut durumu al
            var orderBefore = await _orderService.GetByIdAsync(dto.Id);
            if (orderBefore == null) return NotFound();
            var currentStatus = orderBefore.Status;

            await _orderService.UpdateAsync(dto);

            // Cari ve stok hareketlerini senkronize et
            await SyncTransactionAndStockAsync(dto.Id, currentStatus);

            // Teslimat satırlarını yeni sipariş yapısına göre uyumlu hale getir
            // (silinen kalemlerin teslimat satırı silinir, miktar aşımları düşürülür)
            var reconciledCount = await _deliveryService.ReconcileWithOrderAsync(dto.Id);

            // ───────── Düzenleme sonrası TESLİM DURUMU otomatik kontrolü ─────────
            // 1) Tamamlanmış siparişe yeni kalem eklenmiş ve açık kalan varsa → Üretime düşür
            // 2) Üretimdeki sipariş için tüm kalemler teslim edildiyse → Tamamlandı'ya yükselt
            var openLines = (await _deliveryService.GetOpenLinesForOrderAsync(dto.Id)).ToList();
            var hasOpen = openLines.Any();
            string? statusChangeMsg = null;

            if (currentStatus == OrderStatus.Tamamlandi && hasOpen)
            {
                // Tamamlanmıştı ama düzenleme sonrası açık kalem var → Üretime al
                await _orderService.UpdateStatusAsync(dto.Id, OrderStatus.Uretimde);
                statusChangeMsg = $"Düzenleme sonucu {openLines.Count} kalem teslim edilmemiş kaldı. Sipariş 'Üretimde' durumuna alındı. " +
                                   "Kalan kalemleri teslim ettiğinizde 'Tamamla' butonuna basabilirsiniz.";
            }
            else if (currentStatus == OrderStatus.Uretimde && !hasOpen)
            {
                // Üretimde'ydi ve açık kalan yok → otomatik Tamamlandı
                await _orderService.UpdateStatusAsync(dto.Id, OrderStatus.Tamamlandi);
                await _orderService.SetCompletedAtAsync(dto.Id, TurkeyTime.Now);
                statusChangeMsg = "Tüm kalemler teslim edilmiş olduğu için sipariş otomatik 'Tamamlandı' olarak işaretlendi.";
            }

            var baseMsg = reconciledCount > 0
                ? $"Sipariş güncellendi. {reconciledCount} teslimat satırı yeni kalem yapısına göre uyarlandı."
                : "Sipariş başarıyla güncellendi.";
            TempData["Success"] = statusChangeMsg != null ? $"{baseMsg} {statusChangeMsg}" : baseMsg;
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Güncelleme hatası: {ex.Message}";
            await LoadSelectLists();
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, DateTime? completedAt = null)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();

        var previousStatus = order.Status;

        // İptal denemesinde onaylı teslimat varsa engelle
        if (status == OrderStatus.IptalEdildi && await _deliveryService.HasApprovedDeliveryAsync(id))
        {
            TempData["Error"] = "Bu siparişte onaylanmış teslimatlar var. Sipariş iptal edilemez. Önce teslimatları silin veya onayı geri alın.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Tamamlandı'ya geçişte (Üretimde'den VEYA düzenleme sonrası Tamamlandı'da kalmış açık kalemler için)
        // otomatik tam teslimat oluştur — servis açık kalan yoksa zaten null döner.
        if (status == OrderStatus.Tamamlandi
            && (previousStatus == OrderStatus.Uretimde || previousStatus == OrderStatus.Tamamlandi))
        {
            try
            {
                await _deliveryService.CreateFullDeliveryForOrderAsync(id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Otomatik teslimat oluşturulamadı: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        await _orderService.UpdateStatusAsync(id, status);

        // Tamamlandı durumunda kullanıcı teslim tarihi seçmişse onu CompletedAt'e yaz
        if (status == OrderStatus.Tamamlandi && completedAt.HasValue)
        {
            await _orderService.SetCompletedAtAsync(id, completedAt.Value);
        }

        // İptal veya onay geri alma → cari borç sil + stok iade et
        bool isUnapproving = previousStatus == OrderStatus.Onaylandi && status == OrderStatus.Taslak;
        bool isReversingFromUretim = (previousStatus == OrderStatus.Uretimde || previousStatus == OrderStatus.Onaylandi) && status == OrderStatus.Taslak;
        if (status == OrderStatus.IptalEdildi || isReversingFromUretim)
        {
            var existing = await _transactionService.GetByCustomerAsync(order.CustomerId);
            var related = existing.Where(t => t.Type == TransactionType.Borc &&
                                              t.Description != null &&
                                              t.Description.Contains(order.OrderNumber)).ToList();
            foreach (var tx in related)
            {
                await _transactionService.DeleteAsync(tx.Id);
            }

            // Stok iadesi: bu siparişe istinaden yapılan tüm çıkışları sil → stoklar geri artar
            await _stockService.ReverseStockForOrderAsync(id);
        }

        // Sipariş onaylandığında termin tarihi otomatik ata
        if (status == OrderStatus.Onaylandi)
        {
            var deliveryDays = (int)await _settingService.GetDecimalAsync("DefaultDeliveryDays", 14);
            await _orderService.SetDeliveryDateAsync(id, TurkeyTime.Now.AddDays(deliveryDays));
        }

        // Sipariş onaylandığında müşteriye borç hareketi oluştur (idempotent)
        if (status == OrderStatus.Onaylandi && order.TotalAmount > 0)
        {
            var existing = await _transactionService.GetByCustomerAsync(order.CustomerId);
            var alreadyHas = existing.Any(t => t.Type == TransactionType.Borc &&
                                                t.Description != null &&
                                                t.Description.Contains(order.OrderNumber));
            if (!alreadyHas)
            {
                await _transactionService.CreateAsync(new AccountTransactionCreateDto
                {
                    CustomerId = order.CustomerId,
                    Type = TransactionType.Borc,
                    PaymentType = PaymentType.Nakit,
                    Amount = order.GrandTotal,
                    Currency = order.Currency,
                    ExchangeRate = 1,
                    TransactionDate = TurkeyTime.Now,
                    Description = $"Sipariş onayı: {order.OrderNumber}"
                });
            }
        }

        // Üretime alındığında stok düşme işlemi (onayda değil, üretimde düşer)
        if (status == OrderStatus.Uretimde && previousStatus == OrderStatus.Onaylandi)
        {
            await _stockService.DeductStockForOrderAsync(id);
        }

        var statusText = status switch
        {
            OrderStatus.Onaylandi => "onaylandı (cari hareket oluşturuldu)",
            OrderStatus.Uretimde => "üretime alındı (stok çıkışı oluşturuldu)",
            OrderStatus.Tamamlandi => "tamamlandı",
            OrderStatus.IptalEdildi => "iptal edildi (cari hareket silindi, stok iade edildi)",
            OrderStatus.Taslak when isReversingFromUretim => "taslağa geri alındı (cari hareket silindi, stok iade edildi)",
            _ => "güncellendi"
        };
        TempData["Success"] = $"Sipariş durumu {statusText}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackfillTransactions(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction("Index", "Settings");
        }

        var allOrders = (await _orderService.GetAllAsync()).ToList();

        // 1) Önce: İPTAL EDİLMİŞ siparişlerin cari hareketlerini sil + stoklarını iade et
        int deleted = 0, stockReversed = 0;
        var allTxBefore = (await _transactionService.GetAllAsync()).ToList();
        foreach (var o in allOrders.Where(x => x.Status == OrderStatus.IptalEdildi))
        {
            var related = allTxBefore.Where(t => t.CustomerId == o.CustomerId
                                                  && t.Type == TransactionType.Borc
                                                  && t.Description != null
                                                  && t.Description.Contains(o.OrderNumber)).ToList();
            foreach (var tx in related)
            {
                await _transactionService.DeleteAsync(tx.Id);
                deleted++;
            }

            // Stok iadesi
            stockReversed += await _stockService.ReverseStockForOrderAsync(o.Id);
        }

        // 2) Sonra: aktif siparişler için eksik cari hareketleri oluştur
        var orders = allOrders
            .Where(x => x.Status != OrderStatus.IptalEdildi && x.GrandTotal > 0)
            .ToList();

        // Tüm cari hareketleri TEK seferde çek (her sipariş için ayrı sorgu yapma)
        var allTx = (await _transactionService.GetAllAsync()).ToList();

        int created = 0, skipped = 0, processed = 0;
        var details = new List<string>();

        foreach (var o in orders)
        {
            processed++;
            // KDV dahil (GrandTotal) tutarı kullan — müşterinin bize gerçekte borçlandığı tutar bu
            var amount = o.GrandTotal;

            var alreadyHas = allTx.Any(t => t.CustomerId == o.CustomerId
                                            && t.Type == TransactionType.Borc
                                            && t.Description != null
                                            && t.Description.Contains(o.OrderNumber));
            if (alreadyHas)
            {
                skipped++;
                continue;
            }

            var newTx = await _transactionService.CreateAsync(new AccountTransactionCreateDto
            {
                CustomerId = o.CustomerId,
                Type = TransactionType.Borc,
                PaymentType = PaymentType.Nakit,
                Amount = amount,
                Currency = o.Currency,
                ExchangeRate = 1,
                TransactionDate = o.OrderDate,
                Description = $"Sipariş onayı: {o.OrderNumber}"
            });
            // Cache'e ekle ki aynı müşterinin birden fazla siparişinde duplicate oluşmasın
            allTx.Add(newTx);
            created++;
            details.Add($"{o.OrderNumber} → {o.CustomerTitle} ({amount:N2} {o.Currency})");
        }

        var msg = $"İşlenen: {processed}, Oluşturulan: {created}, Atlanılan: {skipped}, İptal Cari Temizlenen: {deleted}, İptal Stok İadesi: {stockReversed}";
        if (created > 0)
            msg += " | İlk 5: " + string.Join(", ", details.Take(5));
        TempData["Success"] = msg;
        return RedirectToAction("Index", "Settings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BackfillCompletedAt(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction("Index", "Settings");
        }

        // Tamamlandı durumundaki ama CompletedAt'i null olan siparişleri bul ve doldur.
        // CompletedAt = DeliveryDate ?? OrderDate (tarih bilgisine göre)
        var allOrders = (await _orderService.GetAllAsync()).ToList();
        var target = allOrders
            .Where(o => o.Status == OrderStatus.Tamamlandi && !o.CompletedAt.HasValue)
            .ToList();

        int fixedCount = 0;
        foreach (var o in target)
        {
            var completedAt = o.DeliveryDate ?? o.OrderDate;
            await _orderService.SetCompletedAtAsync(o.Id, completedAt);
            fixedCount++;
        }

        TempData["Success"] = $"Tamamlandı durumundaki {allOrders.Count(x => x.Status == OrderStatus.Tamamlandi)} sipariş kontrol edildi. {fixedCount} siparişin tamamlanma tarihi dolduruldu. Bu siparişler artık Ürün Satış Raporunda görünecek.";
        return RedirectToAction("Index", "Settings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetAndRecalculateStock(string? syncPassword)
    {
        if (syncPassword != "2702")
        {
            TempData["Error"] = "Senkronizasyon şifresi hatalı. İşlem yapılmadı.";
            return RedirectToAction("Index", "Settings");
        }

        // 1) Sadece sipariş kaynaklı stok hareketlerini sil (manuel hareketlere dokunma)
        int deleted = await _stockService.ResetOrderEntriesAsync();

        // 2) Üretimde veya Tamamlandı durumundaki siparişler için stok çıkışlarını yeniden oluştur
        var allOrders = (await _orderService.GetAllAsync()).ToList();
        var activeOrders = allOrders
            .Where(o => o.Status == OrderStatus.Uretimde || o.Status == OrderStatus.Tamamlandi)
            .ToList();

        int recalculated = 0;
        var errors = new List<string>();
        foreach (var o in activeOrders)
        {
            try
            {
                await _stockService.DeductStockForOrderAsync(o.Id);
                recalculated++;
            }
            catch (Exception ex)
            {
                errors.Add($"{o.OrderNumber}: {ex.Message}");
            }
        }

        var msg = $"Stok sıfırlandı! Silinen hareket: {deleted}, Yeniden hesaplanan sipariş: {recalculated}/{activeOrders.Count}";
        if (errors.Any())
            msg += $" | Hatalar ({errors.Count}): " + string.Join(", ", errors.Take(3));
        TempData["Success"] = msg;
        return RedirectToAction("Index", "Settings");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _orderService.DeleteAsync(id);
            TempData["Success"] = "Sipariş başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Sipariş silinemedi: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> QuickCreate([FromBody] OrderCreateDto dto)
    {
        if (dto.CustomerId <= 0)
            return Json(new { success = false, error = "Müşteri seçilmedi." });

        dto.Lines ??= new List<OrderLineCreateDto>();
        var order = await _orderService.CreateAsync(dto);
        return Json(new { success = true, id = order.Id, orderNumber = order.OrderNumber });
    }

    [HttpPost]
    public async Task<IActionResult> SaveDraft([FromBody] OrderCreateDto dto)
    {
        if (dto.Lines == null || dto.Lines.Count == 0)
            return Json(new { success = false, error = "Kalem eklenmedi." });

        var order = await _orderService.CreateAsync(dto);
        return Json(new { success = true, id = order.Id, orderNumber = order.OrderNumber });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateDraft([FromBody] OrderUpdateDto dto)
    {
        try
        {
            // Boş satırları temizle
            dto.Lines = dto.Lines.Where(l => l.ProductItemId.HasValue || l.RecipeId.HasValue).ToList();

            var orderBefore = await _orderService.GetByIdAsync(dto.Id);
            if (orderBefore == null) return Json(new { success = false, error = "Sipariş bulunamadı." });
            var currentStatus = orderBefore.Status;

            await _orderService.UpdateAsync(dto);

            // Cari ve stok hareketlerini senkronize et
            await SyncTransactionAndStockAsync(dto.Id, currentStatus);

            // Teslimat satırlarını yeni sipariş yapısına göre uyumlu hale getir
            await _deliveryService.ReconcileWithOrderAsync(dto.Id);

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    public IActionResult DownloadTemplate()
    {
        var bytes = _excelService.GenerateOrderLineTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SiparisKalemi_Sablon.xlsx");
    }

    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return Json(new { success = false, error = "Dosya seçilmedi." });
        using var stream = file.OpenReadStream();
        var lines = await _excelService.ParseOrderLinesFromExcelAsync(stream);
        return Json(new { success = true, lines });
    }

    [HttpGet]
    public async Task<IActionResult> GetPriceHistory(string type, int id)
    {
        int? productItemId = type == "P" ? id : null;
        int? recipeId = type == "R" ? id : null;
        var history = await _orderService.GetPriceHistoryAsync(productItemId, recipeId);
        return Json(history.Select(h => new
        {
            orderNumber = h.OrderNumber,
            orderDate = h.OrderDate.ToString("dd.MM.yyyy"),
            customerTitle = h.CustomerTitle,
            widthMm = h.WidthMm,
            heightMm = h.HeightMm,
            quantity = h.Quantity,
            unitPrice = h.UnitPrice
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetItemPrice(string type, int id)
    {
        if (type == "P")
        {
            var product = await _productService.GetByIdAsync(id);
            return Json(new { price = product?.UnitPrice ?? 0 });
        }
        else if (type == "R")
        {
            var recipe = await _recipeService.GetByIdAsync(id);
            return Json(new { price = recipe?.BaseUnitPrice ?? 0 });
        }
        return Json(new { price = 0 });
    }

    [HttpPost]
    public async Task<IActionResult> MarkLabelPrinted([FromBody] int[] lineIds)
    {
        if (lineIds == null || lineIds.Length == 0)
            return Json(new { success = false });

        foreach (var lineId in lineIds)
        {
            var line = await _orderService.GetLineByIdAsync(lineId);
            if (line != null)
                await _orderService.SetLabelPrintedAsync(lineId);
        }
        return Json(new { success = true });
    }

    /// <summary>
    /// Sipariş güncellendikten sonra ilişkili cari hareket ve stok hareketlerini senkronize eder.
    /// </summary>
    private async Task SyncTransactionAndStockAsync(int orderId, OrderStatus statusBeforeUpdate)
    {
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null) return;

        // ─── CARİ HAREKET GÜNCELLE ───
        // Sipariş onaylanmış veya sonrasıysa, mevcut borç hareketini yeni tutarla güncelle
        if (statusBeforeUpdate != OrderStatus.Taslak && statusBeforeUpdate != OrderStatus.IptalEdildi)
        {
            var existing = await _transactionService.GetByCustomerAsync(order.CustomerId);
            var related = existing.Where(t => t.Type == TransactionType.Borc &&
                                               t.Description != null &&
                                               t.Description.Contains(order.OrderNumber)).ToList();
            if (related.Any())
            {
                foreach (var tx in related)
                    await _transactionService.DeleteAsync(tx.Id);

                if (order.GrandTotal > 0)
                {
                    await _transactionService.CreateAsync(new AccountTransactionCreateDto
                    {
                        CustomerId = order.CustomerId,
                        Type = TransactionType.Borc,
                        PaymentType = PaymentType.Nakit,
                        Amount = order.GrandTotal,
                        Currency = order.Currency,
                        ExchangeRate = 1,
                        TransactionDate = TurkeyTime.Now,
                        Description = $"Sipariş onayı: {order.OrderNumber}"
                    });
                }
            }
        }

        // ─── STOK HAREKETİ GÜNCELLE ───
        // Sipariş üretimde veya tamamlandıysa, stok çıkışlarını yeniden hesapla
        if (statusBeforeUpdate == OrderStatus.Uretimde || statusBeforeUpdate == OrderStatus.Tamamlandi)
        {
            await _stockService.ReverseStockForOrderAsync(orderId);
            await _stockService.DeductStockForOrderAsync(orderId);
        }
    }

    private async Task LoadSelectLists()
    {
        var customers = await _customerService.GetAllAsync();
        ViewBag.Customers = new SelectList(customers.Where(c => c.IsActive), "Id", "Title");

        var products = await _productService.GetAllAsync();
        ViewBag.Products = new SelectList(products.Where(p => p.IsActive), "Id", "Name");
        ViewBag.ProductPrices = products.Where(p => p.IsActive).ToDictionary(p => p.Id, p => p.UnitPrice);

        var recipes = await _recipeService.GetAllAsync();
        ViewBag.Recipes = new SelectList(recipes.Where(r => r.IsActive), "Id", "Code");
        ViewBag.RecipePrices = recipes.Where(r => r.IsActive).ToDictionary(r => r.Id, r => r.BaseUnitPrice);

        ViewBag.MinM2 = await _settingService.GetDecimalAsync("MinM2");

        var featureDefs = await _featureDefRepo.Query()
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .Select(f => new { id = f.Id, name = f.Name, unitPrice = f.UnitPrice })
            .ToListAsync();
        ViewBag.FeatureDefinitions = featureDefs;
    }
}
