using GlassSoft.Domain.Common;
using System.Globalization;
using System.Text;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Entities.Production;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Output;

public class OutputRenderService : IOutputRenderService
{
    private readonly IRepository<PrintTemplate> _templateRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<Customer> _customerRepo;
    private readonly IRepository<WorkOrder> _workOrderRepo;
    private readonly IRepository<AccountTransaction> _transactionRepo;
    private readonly IRepository<Delivery> _deliveryRepo;
    private readonly IAccountTransactionService _transactionService;
    private readonly ISystemSettingService _settingService;

    private static readonly CultureInfo TrCulture = new("tr-TR");

    public OutputRenderService(
        IRepository<PrintTemplate> templateRepo,
        IRepository<Order> orderRepo,
        IRepository<Customer> customerRepo,
        IRepository<WorkOrder> workOrderRepo,
        IRepository<AccountTransaction> transactionRepo,
        IRepository<Delivery> deliveryRepo,
        IAccountTransactionService transactionService,
        ISystemSettingService settingService)
    {
        _templateRepo = templateRepo;
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _workOrderRepo = workOrderRepo;
        _transactionRepo = transactionRepo;
        _deliveryRepo = deliveryRepo;
        _transactionService = transactionService;
        _settingService = settingService;
    }

    public async Task<string> RenderOrderAsync(int orderId, int templateId)
    {
        var template = await GetTemplateAsync(templateId);
        var order = await GetOrderWithDetailsAsync(orderId);

        var placeholders = await BuildOrderPlaceholdersAsync(order);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderDeliveryAsync(int orderId, int templateId)
    {
        var template = await GetTemplateAsync(templateId);
        var order = await GetOrderWithDetailsAsync(orderId);

        var placeholders = await BuildOrderPlaceholdersAsync(order);
        placeholders["TeslimatTarihi"] = order.DeliveryDate?.ToString("dd.MM.yyyy") ?? TurkeyTime.Now.ToString("dd.MM.yyyy");
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderDeliveryFromDeliveryAsync(int deliveryId, int templateId, bool priceless = false)
    {
        var template = await GetTemplateAsync(templateId);

        var delivery = await _deliveryRepo.Query()
            .Include(d => d.Order).ThenInclude(o => o.Customer)
            .Include(d => d.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.ProductItem)
            .Include(d => d.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.Recipe)
            .Include(d => d.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.Features.Where(f => !f.IsDeleted)).ThenInclude(f => f.FeatureDefinition)
            .FirstOrDefaultAsync(d => d.Id == deliveryId)
            ?? throw new KeyNotFoundException($"Teslimat bulunamadı: {deliveryId}");

        // Sipariş bilgilerini doldur ama Lines yerine sadece teslimat satırlarını kullan
        var order = delivery.Order;

        // Teslimat satırlarından geçici OrderLine listesi oluştur (miktar = teslim edilen)
        var deliveryLines = delivery.Lines.Where(l => !l.IsDeleted).Select(dl => new OrderLine
        {
            Id = dl.OrderLine.Id,
            OrderId = dl.OrderLine.OrderId,
            ProductItemId = dl.OrderLine.ProductItemId,
            ProductItem = dl.OrderLine.ProductItem,
            RecipeId = dl.OrderLine.RecipeId,
            Recipe = dl.OrderLine.Recipe,
            WidthMm = dl.OrderLine.WidthMm,
            HeightMm = dl.OrderLine.HeightMm,
            Quantity = dl.Quantity, // ÖNEMLİ: teslim edilen miktarı yaz
            UnitPrice = priceless ? 0 : dl.OrderLine.UnitPrice,
            TotalPrice = priceless ? 0 : dl.OrderLine.UnitPrice * dl.Quantity,
            Notes = dl.OrderLine.Notes,
            PozNo = dl.OrderLine.PozNo,
            Features = dl.OrderLine.Features
        }).ToList();

        var tempOrder = new Order
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Customer = order.Customer,
            OrderDate = order.OrderDate,
            DeliveryDate = order.DeliveryDate,
            Status = order.Status,
            Currency = order.Currency,
            TaxRate = priceless ? 0 : order.TaxRate,
            TotalAmount = priceless ? 0 : deliveryLines.Sum(l => l.TotalPrice),
            Notes = order.Notes,
            OrderCustomerName = order.OrderCustomerName,
            Lines = deliveryLines
        };

        var placeholders = await BuildOrderPlaceholdersAsync(tempOrder);
        // Teslimat özel alanlarını override et
        placeholders["TeslimatNo"] = delivery.DeliveryNumber;
        placeholders["TeslimatTarihi"] = delivery.DeliveryDate.ToString("dd.MM.yyyy");
        placeholders["TeslimatNotlar"] = delivery.Notes ?? "";

        var html = await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);

        // Fiyatsız modda DOM-aware temizlik: thead'deki fiyat kolonlarını ve
        // body'deki aynı index'teki hücreleri senkron şekilde siler.
        if (priceless)
        {
            html = RemovePriceColumnsAndRows(html);
        }

        return html;
    }

    /// <summary>
    /// Marker'sız HTML üzerinde fiyat kolonları ve toplam satırlarını kaldırır.
    /// "Birim Fiyat" / "Toplam" başlıklı kolonlar tamamen silinir.
    /// "Ara Toplam", "KDV", "Genel Toplam" içeren toplam satırları silinir.
    /// "Toplam Adet" ve "Toplam m²" / "Toplam Alan" KORUNUR.
    /// </summary>
    private static string RemovePriceColumnsAndRows(string html)
    {
        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        // İçerik analizinde kullanılacak yardımcı: hücre metnini temizle
        static string Norm(string s) =>
            System.Text.RegularExpressions.Regex.Replace(s ?? "", @"\s+", " ").Trim().ToLowerInvariant();

        // Bir başlık fiyat kolonu mu? — "birim fiyat" veya tek başına "toplam" (m², adet, alan değilse)
        static bool IsPriceHeader(string txt)
        {
            var t = Norm(txt);
            if (t.Contains("birim fiyat") || t == "fiyat") return true;
            // "Toplam" başlığı: ama "Toplam Adet", "Toplam Alan", "Toplam m²", "Satır m²" değilse
            if (t == "toplam") return true;
            return false;
        }

        // Bir satır fiyat satırı mı? — içinde "Ara Toplam", "KDV", "Genel Toplam" geçen
        static bool IsPriceRow(string txt)
        {
            var t = Norm(txt);
            return t.Contains("ara toplam") || t.Contains("kdv") || t.Contains("genel toplam");
        }

        // Tüm tabloları gez
        foreach (var table in doc.DocumentNode.SelectNodes("//table")?.ToList() ?? new())
        {
            // Önce header'dan fiyat kolonlarının index'ini bul
            var headerRow = table.SelectSingleNode(".//thead/tr") ?? table.SelectSingleNode(".//tr[th]");
            if (headerRow != null)
            {
                var headers = headerRow.SelectNodes("./th")?.ToList() ?? new();
                var priceIdx = new List<int>();
                for (int i = 0; i < headers.Count; i++)
                {
                    if (IsPriceHeader(headers[i].InnerText))
                        priceIdx.Add(i);
                }

                if (priceIdx.Count > 0)
                {
                    // Header'dan kaldır (geriden öne — index kaymasın)
                    foreach (var i in priceIdx.OrderByDescending(x => x))
                        headers[i].Remove();

                    // Body'deki her satırdan aynı index'leri kaldır
                    var bodyRows = table.SelectNodes(".//tbody/tr")?.ToList() ?? new();
                    foreach (var row in bodyRows)
                    {
                        var tds = row.SelectNodes("./td")?.ToList() ?? new();
                        foreach (var i in priceIdx.OrderByDescending(x => x))
                            if (i < tds.Count) tds[i].Remove();
                    }
                }
            }

            // Footer/totals tablosunda fiyat satırlarını sil
            var allRows = table.SelectNodes(".//tr")?.ToList() ?? new();
            foreach (var row in allRows)
            {
                if (IsPriceRow(row.InnerText))
                    row.Remove();
            }
        }

        return doc.DocumentNode.OuterHtml;
    }

    public async Task<string> RenderWorkOrderAsync(int workOrderId, int templateId)
    {
        var template = await GetTemplateAsync(templateId);
        var workOrder = await _workOrderRepo.Query()
            .Include(w => w.Lines).ThenInclude(l => l.ProductItem)
            .Include(w => w.Lines).ThenInclude(l => l.OrderLine)
            .Include(w => w.Orders).ThenInclude(wo => wo.Order).ThenInclude(o => o.Customer)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı: {workOrderId}");

        var placeholders = BuildWorkOrderPlaceholders(workOrder);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderAccountTransactionsAsync(int customerId, int templateId, DateTime? startDate, DateTime? endDate)
    {
        var template = await GetTemplateAsync(templateId);
        var customer = await _customerRepo.GetByIdAsync(customerId)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı: {customerId}");

        var start = startDate?.Date ?? DateTime.MinValue;
        var end = endDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        var transactions = await _transactionRepo.Query()
            .Where(t => t.CustomerId == customerId && t.TransactionDate >= start && t.TransactionDate <= end)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var placeholders = BuildTransactionPlaceholders(customer, transactions, start, end);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderAccountStatementAsync(int customerId, int templateId, DateTime? startDate, DateTime? endDate)
    {
        var template = await GetTemplateAsync(templateId);
        var customer = await _customerRepo.GetByIdAsync(customerId)
            ?? throw new KeyNotFoundException($"Müşteri bulunamadı: {customerId}");

        var start = startDate?.Date ?? DateTime.MinValue;
        var end = endDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        // Devreden bakiye: startDate öncesi hareketlerin bakiyesi
        var priorTransactions = await _transactionRepo.Query()
            .Where(t => t.CustomerId == customerId && t.TransactionDate < start)
            .ToListAsync();

        var devredenBakiye = priorTransactions
            .Sum(t => t.Type == TransactionType.Borc ? t.AmountTRY : -t.AmountTRY);

        var transactions = await _transactionRepo.Query()
            .Where(t => t.CustomerId == customerId && t.TransactionDate >= start && t.TransactionDate <= end)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        var placeholders = BuildStatementPlaceholders(customer, transactions, start, end, devredenBakiye);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    private async Task<Order> GetOrderWithDetailsAsync(int orderId)
    {
        return await _orderRepo.Query()
            .Include(o => o.Customer)
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines).ThenInclude(l => l.Recipe)
            .Include(o => o.Lines).ThenInclude(l => l.Features).ThenInclude(f => f.FeatureDefinition)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {orderId}");
    }

    public async Task<string> RenderCitaReportAsync(int orderId, int templateId)
    {
        var template = await GetTemplateAsync(templateId);
        var order = await GetOrderWithDetailsAsync(orderId);

        var placeholders = await BuildOrderPlaceholdersAsync(order);
        // Çıta Raporu: aynı ürün+ölçüdeki kalemleri birleştir, fiyat yok
        var lines = order.Lines.Where(l => !l.IsDeleted).ToList();
        placeholders["Kalemler"] = BuildCitaLinesHtml(lines);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderPricelessOrderAsync(int orderId, int templateId)
    {
        var template = await GetTemplateAsync(templateId);
        var order = await GetOrderWithDetailsAsync(orderId);

        var placeholders = await BuildOrderPlaceholdersAsync(order);
        // Fiyatsız: fiyat sütunları olmadan kalemler
        var lines = order.Lines.Where(l => !l.IsDeleted).ToList();
        placeholders["Kalemler"] = BuildPricelessLinesHtml(lines);
        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderLabelAsync(int orderId, int lineId, int templateId, int copyIndex = 1, int? copyTotalOverride = null)
    {
        var template = await GetTemplateAsync(templateId);
        var order = await GetOrderWithDetailsAsync(orderId);
        var line = order.Lines.FirstOrDefault(l => l.Id == lineId && !l.IsDeleted)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı: {lineId}");

        var companyName = await _settingService.GetValueAsync("CompanyName") ?? "GLASSOFT";
        var companyLogo = await _settingService.GetValueAsync("CompanyLogo") ?? "";

        var copyTotal = copyTotalOverride ?? line.Quantity;

        var placeholders = new Dictionary<string, string>
        {
            ["FirmaAdi"] = companyName,
            ["FirmaLogo"] = !string.IsNullOrEmpty(companyLogo)
                ? $"<img src=\"{companyLogo}\" alt=\"Logo\" style=\"max-height:40px; max-width:120px;\" />"
                : "",
            ["SiparisNo"] = order.OrderNumber,
            ["SiparisTarihi"] = order.OrderDate.ToString("dd.MM.yyyy"),
            ["TeslimTarihi"] = order.DeliveryDate?.ToString("dd.MM.yyyy") ?? "-",
            ["MusteriAdi"] = order.Customer.Title,
            ["MusteriKodu"] = order.Customer.Code,
            ["UrunAdi"] = line.ProductItem?.Name ?? line.Recipe?.Code ?? "-",
            ["PozNo"] = line.PozNo ?? "-",
            ["En"] = line.IsRetail ? "-" : FormatMm(line.WidthMm),
            ["Boy"] = line.IsRetail ? "-" : FormatMm(line.HeightMm),
            ["Olcu"] = line.IsRetail ? "-" : $"{FormatMm(line.WidthMm)} x {FormatMm(line.HeightMm)}",
            ["Alan"] = line.IsRetail ? "-" : FormatDecimal(line.AreaM2, 4),
            ["Adet"] = line.Quantity.ToString(),
            ["Notlar"] = line.Notes ?? "",
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm"),
            ["KalemNo"] = line.Id.ToString(),
            // Etiket sıralama bilgileri (her parça için ayrı etiket çıktığında kullanılır)
            ["EtiketSira"] = $"{copyIndex}/{copyTotal}",
            ["EtiketNo"] = copyIndex.ToString(),
            ["EtiketToplam"] = copyTotal.ToString()
        };

        // Özellikleri ekle
        var features = line.Features.Where(f => !f.IsDeleted).ToList();
        if (features.Any())
        {
            var featureTexts = features.Select(f => $"({f.FeatureDefinition.Name} - {f.Quantity} Adet * {FormatDecimal(f.UnitPrice)} ₺)");
            placeholders["Ozellikler"] = string.Join(", ", featureTexts);
        }
        else
        {
            placeholders["Ozellikler"] = "";
        }

        // Template instance'a dokunma — local kopyada replace yap
        // Aksi halde aynı template her çağrıda mutasyona uğruyor (toplu yazdırmada hep ilk satır görünür)
        var html = template.HtmlContent;
        foreach (var (key, value) in placeholders)
        {
            html = html.Replace("{{" + key + "}}", value);
        }
        return html;
    }

    public async Task<string> RenderLabelsAsync(int orderId, int[] lineIds, int templateId)
    {
        // Etiket template'lerinin kendisinde zaten `page-break-after: always` var.
        // Aralarına ekstra page-break div'i KOYMUYORUZ — yoksa her etiketten sonra boş sayfa açılır.
        //
        // YENİ DAVRANIŞ: Her kalem için kalemin Quantity değeri kadar etiket basılır.
        // Örn. 4 adetli kalem → 4 ayrı sayfa. Her etikette {{EtiketSira}} → "1/4", "2/4" vb.
        var order = await GetOrderWithDetailsAsync(orderId);

        var sb = new StringBuilder();
        foreach (var lineId in lineIds)
        {
            var line = order.Lines.FirstOrDefault(l => l.Id == lineId && !l.IsDeleted);
            if (line == null) continue;

            var copies = Math.Max(1, line.Quantity);
            for (int i = 1; i <= copies; i++)
            {
                var html = await RenderLabelAsync(orderId, lineId, templateId, copyIndex: i, copyTotalOverride: copies);
                sb.Append(html);
            }
        }
        return sb.ToString();
    }

    private async Task<PrintTemplate> GetTemplateAsync(int templateId)
    {
        return await _templateRepo.GetByIdAsync(templateId)
            ?? throw new KeyNotFoundException($"Şablon bulunamadı: {templateId}");
    }

    private async Task<Dictionary<string, string>> BuildOrderPlaceholdersAsync(Order order)
    {
        var lines = order.Lines.Where(l => !l.IsDeleted).ToList();
        var totalQty = lines.Sum(l => l.Quantity);
        var totalArea = lines.Sum(l => l.AreaM2 * l.Quantity);

        // Cari bakiye: Cari ekstre sırasıyla (tarih+ID) bu siparişin hareketine kadar olan running balance
        var allTx = await _transactionRepo.Query()
            .Where(t => t.CustomerId == order.CustomerId)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        // Bu siparişe ait cari hareketi bul
        var orderTx = allTx.FirstOrDefault(t => t.Type == TransactionType.Borc
                                                && t.Description != null
                                                && t.Description.Contains(order.OrderNumber));

        decimal bakiye;
        if (orderTx != null)
        {
            // Ekstre sırasıyla bu harekete kadar (dahil) running balance hesapla
            bakiye = 0;
            foreach (var t in allTx)
            {
                bakiye += (t.Type == TransactionType.Borc ? t.AmountTRY : 0)
                        - (t.Type == TransactionType.Alacak ? t.AmountTRY : 0);
                if (t.Id == orderTx.Id) break;
            }
        }
        else
        {
            // Henüz cari hareketi yoksa (taslak sipariş vs.) güncel bakiyeyi göster
            bakiye = allTx.Sum(t => t.Type == TransactionType.Borc ? t.AmountTRY : -t.AmountTRY);
        }
        var bakiyeYon = bakiye >= 0 ? "Borç" : "Alacak";

        var dict = new Dictionary<string, string>
        {
            ["SiparisNo"] = order.OrderNumber,
            ["SiparisTarihi"] = order.OrderDate.ToString("dd.MM.yyyy"),
            ["TeslimTarihi"] = order.DeliveryDate?.ToString("dd.MM.yyyy") ?? "-",
            ["Durum"] = GetOrderStatusDisplay(order.Status),
            ["MusteriAdi"] = order.Customer.Title,
            ["MusteriAdres"] = order.Customer.Address ?? "-",
            ["MusteriTelefon"] = order.Customer.Phone ?? "-",
            ["MusteriEmail"] = order.Customer.Email ?? "-",
            ["MusteriVergiNo"] = order.Customer.TaxNumber ?? "-",
            ["MusteriVergiDairesi"] = order.Customer.TaxOffice ?? "-",
            ["MusteriKodu"] = order.Customer.Code,
            ["MusteriSehir"] = order.Customer.City ?? "-",
            ["SiparisMusterisi"] = order.OrderCustomerName ?? "-",
            ["ParaBirimi"] = order.Currency,
            ["ToplamTutar"] = FormatDecimal(order.TotalAmount),
            ["KdvOrani"] = order.TaxRate.ToString(),
            ["KdvTutari"] = FormatDecimal(order.TaxAmount),
            ["GenelToplam"] = FormatDecimal(order.GrandTotal),
            ["Notlar"] = order.Notes ?? "",
            ["ToplamAdet"] = totalQty.ToString(),
            ["ToplamAlan"] = FormatDecimal(totalArea),
            ["Kalemler"] = BuildOrderLinesHtml(lines),
            ["CariBakiye"] = FormatDecimal(Math.Abs(bakiye)),
            ["CariBakiyeYon"] = bakiyeYon,
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return dict;
    }

    private Dictionary<string, string> BuildWorkOrderPlaceholders(WorkOrder workOrder)
    {
        var lines = workOrder.Lines.Where(l => !l.IsDeleted).ToList();
        var totalQty = lines.Sum(l => l.Quantity);
        var totalArea = lines.Sum(l => (l.WidthMm * l.HeightMm) / 1_000_000m * l.Quantity);

        var customerNames = workOrder.Orders
            .Where(wo => !wo.IsDeleted)
            .Select(wo => wo.Order.Customer.Title)
            .Distinct()
            .ToList();

        var orderNumbers = workOrder.Orders
            .Where(wo => !wo.IsDeleted)
            .Select(wo => wo.Order.OrderNumber)
            .ToList();

        var dict = new Dictionary<string, string>
        {
            ["IsEmriNo"] = workOrder.WorkOrderNumber,
            ["IsEmriTarihi"] = workOrder.PlannedDate.ToString("dd.MM.yyyy"),
            ["Durum"] = workOrder.IsCompleted ? "Tamamlandı" : "Devam Ediyor",
            ["MusteriAdi"] = string.Join(", ", customerNames),
            ["SiparisNolari"] = string.Join(", ", orderNumbers),
            ["Notlar"] = workOrder.Notes ?? "",
            ["ToplamAdet"] = totalQty.ToString(),
            ["ToplamAlan"] = FormatDecimal(totalArea),
            ["Kalemler"] = BuildWorkOrderLinesHtml(lines),
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return dict;
    }

    private Dictionary<string, string> BuildTransactionPlaceholders(Customer customer, List<AccountTransaction> transactions, DateTime start, DateTime end)
    {
        var totalBorc = transactions.Where(t => t.Type == TransactionType.Borc).Sum(t => t.AmountTRY);
        var totalAlacak = transactions.Where(t => t.Type == TransactionType.Alacak).Sum(t => t.AmountTRY);
        var bakiye = totalBorc - totalAlacak;

        var dict = new Dictionary<string, string>
        {
            ["MusteriAdi"] = customer.Title,
            ["MusteriKodu"] = customer.Code,
            ["MusteriAdres"] = customer.Address ?? "-",
            ["MusteriTelefon"] = customer.Phone ?? "-",
            ["MusteriEmail"] = customer.Email ?? "-",
            ["MusteriVergiNo"] = customer.TaxNumber ?? "-",
            ["MusteriVergiDairesi"] = customer.TaxOffice ?? "-",
            ["MusteriSehir"] = customer.City ?? "-",
            ["BaslangicTarihi"] = start == DateTime.MinValue ? "Tümü" : start.ToString("dd.MM.yyyy"),
            ["BitisTarihi"] = end == DateTime.MaxValue ? "Tümü" : end.ToString("dd.MM.yyyy"),
            ["ToplamBorc"] = FormatDecimal(totalBorc),
            ["ToplamAlacak"] = FormatDecimal(totalAlacak),
            ["Bakiye"] = FormatDecimal(bakiye),
            ["BakiyeYon"] = bakiye >= 0 ? "Borç" : "Alacak",
            ["Hareketler"] = BuildTransactionRowsHtml(transactions),
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return dict;
    }

    private Dictionary<string, string> BuildStatementPlaceholders(Customer customer, List<AccountTransaction> transactions, DateTime start, DateTime end, decimal devredenBakiye)
    {
        var totalBorc = transactions.Where(t => t.Type == TransactionType.Borc).Sum(t => t.AmountTRY);
        var totalAlacak = transactions.Where(t => t.Type == TransactionType.Alacak).Sum(t => t.AmountTRY);
        var bakiye = devredenBakiye + totalBorc - totalAlacak;

        var dict = new Dictionary<string, string>
        {
            ["MusteriAdi"] = customer.Title,
            ["MusteriKodu"] = customer.Code,
            ["MusteriAdres"] = customer.Address ?? "-",
            ["MusteriTelefon"] = customer.Phone ?? "-",
            ["MusteriEmail"] = customer.Email ?? "-",
            ["MusteriVergiNo"] = customer.TaxNumber ?? "-",
            ["MusteriVergiDairesi"] = customer.TaxOffice ?? "-",
            ["MusteriSehir"] = customer.City ?? "-",
            ["EkstreTarihi"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["BaslangicTarihi"] = start == DateTime.MinValue ? "Tümü" : start.ToString("dd.MM.yyyy"),
            ["BitisTarihi"] = end == DateTime.MaxValue ? "Tümü" : end.ToString("dd.MM.yyyy"),
            ["DevredenBakiye"] = FormatDecimal(devredenBakiye),
            ["DevredenYon"] = devredenBakiye >= 0 ? "Borç" : "Alacak",
            ["ToplamBorc"] = FormatDecimal(totalBorc),
            ["ToplamAlacak"] = FormatDecimal(totalAlacak),
            ["Bakiye"] = FormatDecimal(bakiye),
            ["BakiyeYon"] = bakiye >= 0 ? "Borç" : "Alacak",
            ["Hareketler"] = BuildStatementRowsHtml(transactions, devredenBakiye),
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return dict;
    }

    private static string BuildOrderLinesHtml(List<OrderLine> lines)
    {
        var sb = new StringBuilder();
        int sira = 1;
        foreach (var line in lines)
        {
            var itemName = line.ProductItem?.Name ?? line.Recipe?.Code ?? "-";
            var w = line.WidthMm ?? 0;
            var h = line.HeightMm ?? 0;
            var area = (w * h) / 1_000_000m;
            var totalArea = area * line.Quantity;
            var features = line.Features?.Where(f => !f.IsDeleted).ToList();
            var featureText = features != null && features.Count > 0
                ? string.Join(", ", features.Select(f => $"({f.FeatureDefinition?.Name ?? ""} - {f.Quantity} Adet * {FormatDecimal(f.UnitPrice)} ₺)"))
                : "";
            var featureTotal = features?.Sum(f => f.Quantity * f.UnitPrice) ?? 0;

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{itemName}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatMm(w))}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatMm(h))}</td>");
            sb.AppendLine($"<td style=\"text-align:center\">{line.Quantity}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatDecimal(totalArea, 4))}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(line.UnitPrice)}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(line.TotalPrice + featureTotal)}</td>");
            sb.AppendLine($"<td>{featureText}</td>");
            sb.AppendLine($"<td>{line.Notes ?? ""}</td>");
            sb.AppendLine($"<td>{line.PozNo ?? ""}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Çıta Raporu: Aynı ürün+ölçüdeki kalemleri birleştirir, fiyat göstermez
    /// </summary>
    private static string BuildCitaLinesHtml(List<OrderLine> lines)
    {
        // Aynı ürün + en + boy olanları birleştir
        var grouped = lines
            .GroupBy(l => new {
                ProductName = l.ProductItem?.Name ?? l.Recipe?.Code ?? "-",
                WidthMm = l.WidthMm ?? 0,
                HeightMm = l.HeightMm ?? 0
            })
            .Select(g => new {
                g.Key.ProductName,
                g.Key.WidthMm,
                g.Key.HeightMm,
                IsRetail = g.Key.WidthMm == 0 && g.Key.HeightMm == 0,
                Quantity = g.Sum(x => x.Quantity),
                Features = string.Join(", ", g.SelectMany(x => x.Features?.Where(f => !f.IsDeleted).Select(f => $"({f.FeatureDefinition?.Name ?? ""} - {f.Quantity} Adet * {FormatDecimal(f.UnitPrice)} ₺)") ?? Enumerable.Empty<string>()).Distinct()),
                Notes = string.Join("; ", g.Where(x => !string.IsNullOrEmpty(x.Notes)).Select(x => x.Notes!).Distinct()),
                PozNos = string.Join(",", g.Where(x => !string.IsNullOrEmpty(x.PozNo)).Select(x => x.PozNo!).Distinct())
            })
            .OrderBy(x => x.ProductName)
            .ThenByDescending(x => x.WidthMm * x.HeightMm)
            .ToList();

        var sb = new StringBuilder();
        int sira = 1;
        foreach (var item in grouped)
        {
            var area = (item.WidthMm * item.HeightMm) / 1_000_000m;
            var totalArea = area * item.Quantity;
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{item.ProductName}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(item.IsRetail ? "-" : FormatMm(item.WidthMm))}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(item.IsRetail ? "-" : FormatMm(item.HeightMm))}</td>");
            sb.AppendLine($"<td style=\"text-align:center\">{item.Quantity}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(item.IsRetail ? "-" : FormatDecimal(totalArea, 4))}</td>");
            sb.AppendLine($"<td>{item.Features}</td>");
            sb.AppendLine($"<td>{item.Notes}</td>");
            sb.AppendLine($"<td>{item.PozNos}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    /// <summary>
    /// Fiyatsız sipariş: Sipariş fişi gibi ama fiyat sütunları yok
    /// </summary>
    private static string BuildPricelessLinesHtml(List<OrderLine> lines)
    {
        var sb = new StringBuilder();
        int sira = 1;
        foreach (var line in lines)
        {
            var itemName = line.ProductItem?.Name ?? line.Recipe?.Code ?? "-";
            var w = line.WidthMm ?? 0;
            var h = line.HeightMm ?? 0;
            var area = (w * h) / 1_000_000m;
            var totalArea = area * line.Quantity;
            var features = line.Features?.Where(f => !f.IsDeleted).ToList();
            var featureText = features != null && features.Count > 0
                ? string.Join(", ", features.Select(f => $"({f.FeatureDefinition?.Name ?? ""} - {f.Quantity} Adet * {FormatDecimal(f.UnitPrice)} ₺)"))
                : "";

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{itemName}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatMm(w))}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatMm(h))}</td>");
            sb.AppendLine($"<td style=\"text-align:center\">{line.Quantity}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(line.IsRetail ? "-" : FormatDecimal(totalArea, 4))}</td>");
            sb.AppendLine($"<td>{featureText}</td>");
            sb.AppendLine($"<td>{line.Notes ?? ""}</td>");
            sb.AppendLine($"<td>{line.PozNo ?? ""}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    private static string BuildWorkOrderLinesHtml(List<WorkOrderLine> lines)
    {
        var sb = new StringBuilder();
        int sira = 1;
        foreach (var line in lines)
        {
            var area = (line.WidthMm * line.HeightMm) / 1_000_000m;
            var totalArea = area * line.Quantity;
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{line.ProductItem?.Name ?? "-"}</td>");
            sb.AppendLine($"<td>{line.MaterialType}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatMm(line.WidthMm)}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatMm(line.HeightMm)}</td>");
            sb.AppendLine($"<td style=\"text-align:center\">{line.Quantity}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(totalArea, 4)}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    private static string BuildTransactionRowsHtml(List<AccountTransaction> transactions)
    {
        var sb = new StringBuilder();
        decimal runningBalance = 0;
        int sira = 1;
        foreach (var t in transactions)
        {
            var borc = t.Type == TransactionType.Borc ? t.AmountTRY : 0;
            var alacak = t.Type == TransactionType.Alacak ? t.AmountTRY : 0;
            runningBalance += borc - alacak;

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{t.TransactionDate:dd.MM.yyyy}</td>");
            sb.AppendLine($"<td>{GetTransactionTypeDisplay(t.Type)}</td>");
            sb.AppendLine($"<td>{GetPaymentTypeDisplay(t.PaymentType)}</td>");
            sb.AppendLine($"<td>{t.Description ?? "-"}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(borc > 0 ? FormatDecimal(borc) : "")}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(alacak > 0 ? FormatDecimal(alacak) : "")}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(Math.Abs(runningBalance))}</td>");
            sb.AppendLine($"<td>{(runningBalance >= 0 ? "B" : "A")}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    private static string BuildStatementRowsHtml(List<AccountTransaction> transactions, decimal openingBalance)
    {
        var sb = new StringBuilder();
        decimal runningBalance = openingBalance;

        // Devreden bakiye satırı
        sb.AppendLine("<tr style=\"font-weight:bold; background-color:#f8f9fa\">");
        sb.AppendLine("<td></td>");
        sb.AppendLine("<td></td>");
        sb.AppendLine("<td colspan=\"3\">Devreden Bakiye</td>");
        sb.AppendLine("<td></td>");
        sb.AppendLine("<td></td>");
        sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(Math.Abs(openingBalance))}</td>");
        sb.AppendLine($"<td>{(openingBalance >= 0 ? "B" : "A")}</td>");
        sb.AppendLine("</tr>");

        int sira = 1;
        foreach (var t in transactions)
        {
            var borc = t.Type == TransactionType.Borc ? t.AmountTRY : 0;
            var alacak = t.Type == TransactionType.Alacak ? t.AmountTRY : 0;
            runningBalance += borc - alacak;

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{sira++}</td>");
            sb.AppendLine($"<td>{t.TransactionDate:dd.MM.yyyy}</td>");
            sb.AppendLine($"<td>{GetTransactionTypeDisplay(t.Type)}</td>");
            sb.AppendLine($"<td>{GetPaymentTypeDisplay(t.PaymentType)}</td>");
            sb.AppendLine($"<td>{t.Description ?? "-"}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(borc > 0 ? FormatDecimal(borc) : "")}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{(alacak > 0 ? FormatDecimal(alacak) : "")}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(Math.Abs(runningBalance))}</td>");
            sb.AppendLine($"<td>{(runningBalance >= 0 ? "B" : "A")}</td>");
            sb.AppendLine("</tr>");
        }
        return sb.ToString();
    }

    public async Task<string> RenderProductSalesReportAsync(int templateId, DateTime startDate, DateTime endDate)
    {
        var template = await GetTemplateAsync(templateId);

        var start = startDate.Date;
        var end = endDate.Date.AddDays(1).AddTicks(-1);

        // Taslak ve İptal hariç, teslim (kapanma) tarihi olan tüm siparişler
        var orders = await _orderRepo.Query()
            .Where(o => o.CompletedAt.HasValue
                        && o.CompletedAt.Value >= start && o.CompletedAt.Value <= end
                        && o.Status != OrderStatus.Taslak
                        && o.Status != OrderStatus.IptalEdildi)
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines).ThenInclude(l => l.Recipe)
            .ToListAsync();

        var allLines = orders
            .SelectMany(o => o.Lines.Where(l => !l.IsDeleted).Select(l => new { Order = o, Line = l }))
            .ToList();

        // Ürün adı bazında grupla (ProductItem.Name veya Recipe.Code)
        var grouped = allLines
            .GroupBy(x => x.Line.ProductItem?.Name ?? x.Line.Recipe?.Name ?? x.Line.Recipe?.Code ?? "Bilinmeyen")
            .Select(g => new
            {
                UrunAdi = g.Key,
                ToplamAdet = g.Sum(x => x.Line.Quantity),
                ToplamM2 = g.Sum(x => x.Line.AreaM2 * x.Line.Quantity),
                SiparisSayisi = g.Select(x => x.Order.Id).Distinct().Count()
            })
            .OrderByDescending(g => g.ToplamM2)
            .ToList();

        var sb = new StringBuilder();
        int idx = 1;
        foreach (var g in grouped)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{idx++}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(g.UrunAdi)}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{g.ToplamAdet}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(g.ToplamM2)} m²</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{g.SiparisSayisi}</td>");
            sb.AppendLine("</tr>");
        }

        var placeholders = new Dictionary<string, string>
        {
            ["BaslangicTarihi"] = startDate.ToString("dd.MM.yyyy"),
            ["BitisTarihi"] = endDate.ToString("dd.MM.yyyy"),
            ["ToplamUrunCesidi"] = grouped.Count.ToString(),
            ["ToplamSiparis"] = orders.Count.ToString(),
            ["GenelToplamM2"] = FormatDecimal(grouped.Sum(g => g.ToplamM2)),
            ["GenelToplamAdet"] = grouped.Sum(g => g.ToplamAdet).ToString(),
            ["Urunler"] = sb.ToString(),
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    public async Task<string> RenderBalanceListAsync(int templateId)
    {
        var template = await GetTemplateAsync(templateId);

        var customers = await _customerRepo.Query()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Title)
            .ToListAsync();

        var sb = new StringBuilder();
        int idx = 1;
        decimal toplamBorc = 0, toplamAlacak = 0;

        foreach (var c in customers)
        {
            var balance = await _transactionService.GetCustomerBalanceAsync(c.Id);
            if (balance > 0) toplamBorc += balance;
            else if (balance < 0) toplamAlacak += Math.Abs(balance);

            var yon = balance > 0 ? "Borç" : balance < 0 ? "Alacak" : "Sıfır";

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td style=\"text-align:center\">{idx++}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.Code)}</td>");
            sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(c.Title)}</td>");
            sb.AppendLine($"<td style=\"text-align:right\">{FormatDecimal(Math.Abs(balance))}</td>");
            sb.AppendLine($"<td style=\"text-align:center\">{yon}</td>");
            sb.AppendLine("</tr>");
        }

        var placeholders = new Dictionary<string, string>
        {
            ["ToplamBorc"] = FormatDecimal(toplamBorc),
            ["ToplamAlacak"] = FormatDecimal(toplamAlacak),
            ["CariSayisi"] = customers.Count.ToString(),
            ["Cariler"] = sb.ToString(),
            ["Tarih"] = TurkeyTime.Now.ToString("dd.MM.yyyy"),
            ["Saat"] = TurkeyTime.Now.ToString("HH:mm")
        };

        return await ReplacePlaceholdersAsync(template.HtmlContent, placeholders);
    }

    private async Task<string> ReplacePlaceholdersAsync(string html, Dictionary<string, string> placeholders)
    {
        // Firma bilgilerini sistem ayarlarından ekle
        var companyName = await _settingService.GetValueAsync("CompanyName") ?? "GLASSOFT";
        var companySlogan = await _settingService.GetValueAsync("CompanySlogan") ?? "";
        var companyLogo = await _settingService.GetValueAsync("CompanyLogo") ?? "";

        placeholders["FirmaAdi"] = companyName;
        placeholders["FirmaSlogan"] = companySlogan;
        placeholders["FirmaLogo"] = !string.IsNullOrEmpty(companyLogo)
            ? $"<img src=\"{companyLogo}\" alt=\"Logo\" style=\"max-height:50px; max-width:150px; margin-bottom:5px;\" />"
            : "";

        foreach (var (key, value) in placeholders)
        {
            html = html.Replace("{{" + key + "}}", value);
        }
        return html;
    }

    private static string FormatDecimal(decimal value, int decimals = 2)
    {
        return value.ToString($"N{decimals}", TrCulture);
    }

    // mm değerleri için: binlik ayraçsız, ondalıksız tamsayı. "1350" gibi.
    private static string FormatMm(decimal value)
        => ((int)Math.Round(value)).ToString(CultureInfo.InvariantCulture);

    private static string FormatMm(decimal? value)
        => value.HasValue ? FormatMm(value.Value) : "-";

    private static string GetOrderStatusDisplay(OrderStatus status) => status switch
    {
        OrderStatus.Taslak => "Taslak",
        OrderStatus.Onaylandi => "Onaylandı",
        OrderStatus.Uretimde => "Üretimde",
        OrderStatus.Tamamlandi => "Tamamlandı",
        OrderStatus.IptalEdildi => "İptal Edildi",
        _ => status.ToString()
    };

    private static string GetTransactionTypeDisplay(TransactionType type) => type switch
    {
        TransactionType.Borc => "Borç",
        TransactionType.Alacak => "Alacak",
        _ => type.ToString()
    };

    private static string GetPaymentTypeDisplay(PaymentType type) => type switch
    {
        PaymentType.Nakit => "Nakit",
        PaymentType.Havale => "Havale",
        PaymentType.Cek => "Çek",
        PaymentType.Senet => "Senet",
        PaymentType.KrediKarti => "Kredi Kartı",
        _ => type.ToString()
    };
}
