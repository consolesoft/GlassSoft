using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Purchasing;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Entities.Recipe;
using GlassSoft.Domain.Entities.Production;
using GlassSoft.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Infrastructure.Data;

/// <summary>
/// Demo verisi yükler: Ürün grupları, ürünler, plakalar, reçeteler, müşteriler,
/// tedarikçiler, siparişler, stok hareketleri ve ön muhasebe kayıtları.
/// Idempotent: Zaten müşteri verisi varsa hiçbir şey eklemez.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (!await db.Customers.AnyAsync())
        {
            await SeedSystemSettingsAsync(db);
            var groups   = await SeedProductGroupsAsync(db);
            var products = await SeedProductsAsync(db, groups);
            var recipes  = await SeedRecipesAsync(db, products);
            await SeedOrderFeatureDefinitionsAsync(db);
            var customers = await SeedCustomersAsync(db);
            await SeedSuppliersAsync(db);
            var cashRegister = await SeedCashRegistersAsync(db);
            await SeedStockEntriesAsync(db, products);
            await SeedOrdersAsync(db, customers, products, recipes);
            await SeedAccountingAsync(db, customers, cashRegister);
        }

        // Kesim optimizasyonu testi için her başlangıçta 20 adet optimize edilmemiş demo iş emri
        await SeedDemoWorkOrdersAsync(db);

        // İş emrine henüz dönüşmemiş, optimize edilmemiş 20 sipariş kalemi
        await SeedUnoptimizedDemoOrderLinesAsync(db);
    }

    // ── 1. SİSTEM AYARLARI ────────────────────────────────────────────────────
    private static async Task SeedSystemSettingsAsync(ApplicationDbContext db)
    {
        if (await db.SystemSettings.AnyAsync()) return;
        db.SystemSettings.AddRange(
            new SystemSetting { Key = "MinM2",              Value = "0.1" },
            new SystemSetting { Key = "DefaultDeliveryDays", Value = "14" },
            new SystemSetting { Key = "CompanyName",        Value = "GlassSoft Cam San. ve Tic. Ltd. Şti." },
            new SystemSetting { Key = "CompanyPhone",       Value = "0212 555 00 11" },
            new SystemSetting { Key = "CompanyAddress",     Value = "İkitelli OSB, İstanbul" }
        );
        await db.SaveChangesAsync();
    }

    // ── 2. ÜRÜN GRUPLARI ──────────────────────────────────────────────────────
    private static async Task<Dictionary<string, ProductGroup>> SeedProductGroupsAsync(ApplicationDbContext db)
    {
        var groups = new Dictionary<string, ProductGroup>
        {
            ["DuzCam"]   = new() { Name = "Düz Cam",           Description = "Şeffaf, füme, bronz düz cam ürünleri", IsActive = true },
            ["Isicam"]   = new() { Name = "Isıcam",            Description = "Çift ve üçlü cam ısıcam sistemleri",  IsActive = true },
            ["Cita"]     = new() { Name = "Çıta / Ara Parça",  Description = "Alüminyum ve plastik çıtalar",         IsActive = true },
            ["Sarf"]     = new() { Name = "Sarf Malzeme",      Description = "Butyl, polisülfür, silikon ve diğer", IsActive = true },
            ["GuvenCam"] = new() { Name = "Güvenlik Camı",     Description = "Temperli ve lamine güvenlik camları",  IsActive = true },
        };
        db.ProductGroups.AddRange(groups.Values);
        await db.SaveChangesAsync();
        return groups;
    }

    // ── 3. ÜRÜNLER + PLAKA TANIMLAR ───────────────────────────────────────────
    private static async Task<Dictionary<string, ProductItem>> SeedProductsAsync(
        ApplicationDbContext db, Dictionary<string, ProductGroup> groups)
    {
        var p = new Dictionary<string, ProductItem>();

        // Düz Cam (plaka ürün)
        void AddCam(string key, string code, string name, decimal thick, decimal price)
        {
            var item = new ProductItem { Code = code, Name = name, ProductGroup = groups["DuzCam"], Unit = UnitType.MetreKare, UnitPrice = price, ThicknessMm = thick, IsPlate = true, IsActive = true };
            db.ProductItems.Add(item);
            p[key] = item;
        }
        AddCam("4mmDuz",  "CAM-04-SF", "4mm Düz Cam Şeffaf",  4m,  85m);
        AddCam("5mmDuz",  "CAM-05-SF", "5mm Düz Cam Şeffaf",  5m, 105m);
        AddCam("6mmDuz",  "CAM-06-SF", "6mm Düz Cam Şeffaf",  6m, 130m);
        AddCam("8mmDuz",  "CAM-08-SF", "8mm Düz Cam Şeffaf",  8m, 180m);
        AddCam("10mmDuz", "CAM-10-SF", "10mm Düz Cam Şeffaf", 10m, 250m);
        AddCam("4mmFume", "CAM-04-FM", "4mm Füme Cam",        4m, 100m);
        AddCam("6mmFume", "CAM-06-FM", "6mm Füme Cam",        6m, 155m);
        AddCam("4mmBronz","CAM-04-BR", "4mm Bronz Cam",       4m, 100m);

        // Güvenlik Camı (plaka)
        AddCam("Temp6", "TMP-06-SF", "6mm Temperli Cam", 6m, 220m);
        AddCam("Temp8", "TMP-08-SF", "8mm Temperli Cam", 8m, 290m);
        AddCam("Lam44", "LAM-04-04", "4+4 Lamine Cam",   8m, 350m);
        p["Temp6"].ProductGroup = groups["GuvenCam"];
        p["Temp8"].ProductGroup = groups["GuvenCam"];
        p["Lam44"].ProductGroup = groups["GuvenCam"];

        // Çıta (birim: Metre, IsPlate=false)
        void AddCita(string key, string code, string name, decimal price)
        {
            var item = new ProductItem { Code = code, Name = name, ProductGroup = groups["Cita"], Unit = UnitType.Metre, UnitPrice = price, IsPlate = false, IsActive = true };
            db.ProductItems.Add(item);
            p[key] = item;
        }
        AddCita("Cita6",  "CITA-06-AL", "6mm Alüminyum Çıta",  8m);
        AddCita("Cita9",  "CITA-09-AL", "9mm Alüminyum Çıta",  10m);
        AddCita("Cita12", "CITA-12-AL", "12mm Alüminyum Çıta", 12m);
        AddCita("Cita16", "CITA-16-AL", "16mm Alüminyum Çıta", 14m);
        AddCita("Cita20", "CITA-20-AL", "20mm Alüminyum Çıta", 16m);

        // Sarf Malzeme
        void AddSarf(string key, string code, string name, UnitType unit, decimal price)
        {
            var item = new ProductItem { Code = code, Name = name, ProductGroup = groups["Sarf"], Unit = unit, UnitPrice = price, IsPlate = false, IsActive = true };
            db.ProductItems.Add(item);
            p[key] = item;
        }
        AddSarf("Butyl",      "SARF-BTL", "Butyl Bant",              UnitType.Metre,    2.5m);
        AddSarf("Polisulfur", "SARF-PSL", "Polisülfür (Dış Dolgu)",  UnitType.Kilogram, 45m);
        AddSarf("Silikon",    "SARF-SLK", "Silikon (Yapıştırıcı)",   UnitType.Kilogram, 55m);
        AddSarf("MolElek",    "SARF-MOL", "Moleküler Elek",          UnitType.Kilogram, 30m);

        await db.SaveChangesAsync();

        // Plaka Tanımları
        var plateCams = new[] { p["4mmDuz"], p["5mmDuz"], p["6mmDuz"], p["8mmDuz"], p["10mmDuz"] };
        var plateSizes = new[]
        {
            ("3210x2250 Standart", 3210m, 2250m, true),
            ("3210x2550 Büyük",    3210m, 2550m, false),
            ("2550x1605 Küçük",    2550m, 1605m, false),
            ("3210x6000 Jumbo",    3210m, 6000m, false),
        };
        foreach (var (name, w, h, isDef) in plateSizes)
        {
            db.GlassPlateDefinitions.Add(new GlassPlateDefinition
            {
                Name = name, WidthMm = w, HeightMm = h, IsDefault = isDef
            });
        }
        await db.SaveChangesAsync();
        return p;
    }

    // ── 4. REÇETELer (BOM) ────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Recipe>> SeedRecipesAsync(
        ApplicationDbContext db, Dictionary<string, ProductItem> p)
    {
        var recipes = new Dictionary<string, Recipe>();

        // (key, code, name, basePrice, [(layerType,productKey,thickMm)])
        var defs = new (string key, string code, string name, decimal price, (string lt, string pk, decimal th)[] layers)[]
        {
            ("IC_4_12_4", "4+12+4",  "4+12+4 Isıcam",               210m, [("Glass","4mmDuz",4m),  ("Spacer","Cita12",12m), ("Glass","4mmDuz",4m)]),
            ("IC_4_16_4", "4+16+4",  "4+16+4 Isıcam",               230m, [("Glass","4mmDuz",4m),  ("Spacer","Cita16",16m), ("Glass","4mmDuz",4m)]),
            ("IC_5_12_5", "5+12+5",  "5+12+5 Isıcam",               250m, [("Glass","5mmDuz",5m),  ("Spacer","Cita12",12m), ("Glass","5mmDuz",5m)]),
            ("IC_6_12_6", "6+12+6",  "6+12+6 Isıcam",               290m, [("Glass","6mmDuz",6m),  ("Spacer","Cita12",12m), ("Glass","6mmDuz",6m)]),
            ("IC_6_16_6", "6+16+6",  "6+16+6 Isıcam (Low-E)",       320m, [("Glass","6mmDuz",6m),  ("Spacer","Cita16",16m), ("Glass","6mmDuz",6m)]),
            ("IC_4_9_4",  "4+9+4",   "4+9+4 Isıcam Küçük",          195m, [("Glass","4mmDuz",4m),  ("Spacer","Cita9",9m),   ("Glass","4mmDuz",4m)]),
            ("IC_5_16_5", "5+16+5",  "5+16+5 Isıcam",               265m, [("Glass","5mmDuz",5m),  ("Spacer","Cita16",16m), ("Glass","5mmDuz",5m)]),
            ("IC_4_20_4", "4+20+4",  "4+20+4 Isıcam Geniş Aralık",  245m, [("Glass","4mmDuz",4m),  ("Spacer","Cita20",20m), ("Glass","4mmDuz",4m)]),
        };

        foreach (var (key, code, name, price, layers) in defs)
        {
            var recipe = new Recipe { Code = code, Name = name, BaseUnitPrice = price, IsActive = true, Description = $"{name} — Standart ısıcam reçetesi" };
            db.Recipes.Add(recipe);
            int sort = 1;
            foreach (var (lt, pk, th) in layers)
                recipe.Layers.Add(new RecipeLayer { SortOrder = sort++, LayerType = lt, ProductItem = p[pk], ThicknessMm = th, QuantityPerUnit = 1 });

            recipe.Consumables.Add(new RecipeConsumable { ProductItem = p["Butyl"],      ConsumptionFormula = "PERIMETER * 0.01",  Description = "Çevre başına butyl bant" });
            recipe.Consumables.Add(new RecipeConsumable { ProductItem = p["Polisulfur"], ConsumptionFormula = "PERIMETER * 0.005", Description = "Dış dolgu polisülfür" });
            recipe.Consumables.Add(new RecipeConsumable { ProductItem = p["MolElek"],    ConsumptionFormula = "PERIMETER * 0.003", Description = "Çıta iç dolgu moleküler elek" });
            recipes[key] = recipe;
        }
        await db.SaveChangesAsync();
        return recipes;
    }

    // ── 5. SİPARİŞ ÖZELLİK TANIMLARI ────────────────────────────────────────
    private static async Task SeedOrderFeatureDefinitionsAsync(ApplicationDbContext db)
    {
        if (await db.OrderFeatureDefinitions.AnyAsync()) return;
        db.OrderFeatureDefinitions.AddRange(
            new OrderFeatureDefinition { Name = "Temperli",           UnitPrice = 50m,  IsActive = true, Description = "Cam temperli işlem" },
            new OrderFeatureDefinition { Name = "Lamine (PVB)",       UnitPrice = 80m,  IsActive = true, Description = "PVB lamine cam" },
            new OrderFeatureDefinition { Name = "Buzlu / Sandblast",  UnitPrice = 35m,  IsActive = true, Description = "Sandblast buzlu yüzey" },
            new OrderFeatureDefinition { Name = "Low-E Kaplama",      UnitPrice = 60m,  IsActive = true, Description = "Düşük ısı geçirgenlik kaplaması" },
            new OrderFeatureDefinition { Name = "Temizlik",           UnitPrice = 10m,  IsActive = true, Description = "Cam yüzey temizliği" },
            new OrderFeatureDefinition { Name = "Kenar İşleme",       UnitPrice = 15m,  IsActive = true, Description = "Kenar rodaj / cilalama" },
            new OrderFeatureDefinition { Name = "Delik (Adet)",       UnitPrice = 25m,  IsActive = true, Description = "Her delik için" },
            new OrderFeatureDefinition { Name = "Köşe Kesimi",        UnitPrice = 20m,  IsActive = true, Description = "Her köşe kesimi için" }
        );
        await db.SaveChangesAsync();
    }

    // ── 6. MÜŞTERİLER ────────────────────────────────────────────────────────
    private static async Task<Dictionary<string, Customer>> SeedCustomersAsync(ApplicationDbContext db)
    {
        var c = new Dictionary<string, Customer>();
        var defs = new[]
        {
            ("ARTES",  "Artes Yapı Malzemeleri A.Ş.",   "4520112233","Küçükçekmece VD","TRY","0212 678 90 12","satin@artes.com.tr","İkitelli, İstanbul","İstanbul"),
            ("OZTURK", "Öztürk Cam ve Ayna Ltd. Şti.",  "7750098765","Bağcılar VD",    "TRY","0212 342 11 22","siparis@ozturkcam.com","Bağcılar, İstanbul","İstanbul"),
            ("YAPTEK", "Yaptek İnşaat A.Ş.",             "3300456789","Ankara VD",      "TRY","0312 445 66 77","temin@yaptek.com.tr","Ostim OSB, Ankara","Ankara"),
            ("TUREKS",  "Tureks Dış Ticaret Ltd.",       "9900112233","İzmir VD",       "EUR","0232 380 90 90","info@tureks.com","Konak, İzmir","İzmir"),
            ("GULER",  "Güler Cam Sistemleri",           "5510034455","Kadıköy VD",     "TRY","0216 555 22 33","info@gulercam.com","Kadıköy, İstanbul","İstanbul"),
            ("ANADOLU","Anadolu Bina Cephe Ltd. Şti.",   "4410098234","Konya VD",       "TRY","0332 780 12 34","satis@anadolucephe.com","Selçuklu, Konya","Konya"),
            ("METAM",  "Metam Metal Cephe A.Ş.",         "2200098711","Kocaeli VD",     "USD","0262 500 40 50","procurement@metam.com","Gebze OSB, Kocaeli","Kocaeli"),
            ("BARIS",  "Barış Yapı Dekorasyon",          "9870034512","Pendik VD",      "TRY","0216 340 10 20","baris@barispy.com","Pendik, İstanbul","İstanbul"),
        };
        foreach (var (key, title, taxNo, taxOff, curr, phone, email, addr, city) in defs)
        {
            var cust = new Customer { Code = key, Title = title, TaxNumber = taxNo, TaxOffice = taxOff, Currency = curr, Phone = phone, Email = email, Address = addr, City = city, IsActive = true };
            db.Customers.Add(cust);
            c[key] = cust;
        }
        await db.SaveChangesAsync();
        return c;
    }

    // ── 7. TEDARİKÇİLER ──────────────────────────────────────────────────────
    private static async Task SeedSuppliersAsync(ApplicationDbContext db)
    {
        if (await db.Suppliers.AnyAsync()) return;
        db.Suppliers.AddRange(
            new Supplier { Code = "SISECAM", Title = "Şişecam A.Ş.",               TaxNumber = "1100012345", TaxOffice = "İstanbul VD",   Phone = "0212 350 00 00", Email = "satis@sisecam.com.tr",  IsActive = true },
            new Supplier { Code = "TRAKYA",  Title = "Trakya Cam San. A.Ş.",        TaxNumber = "2200023456", TaxOffice = "Kırklareli VD",  Phone = "0288 300 10 10", Email = "ticari@trakyacam.com",  IsActive = true },
            new Supplier { Code = "BATIKIM", Title = "Batı Kimya Sarf Malz. Ltd.",  TaxNumber = "3300034567", TaxOffice = "İzmir VD",        Phone = "0232 360 20 20", Email = "bilgi@batikim.com",     IsActive = true },
            new Supplier { Code = "ALUCITA", Title = "Alu Çıta Sanayi A.Ş.",        TaxNumber = "4400045678", TaxOffice = "Bursa VD",        Phone = "0224 480 30 30", Email = "satin@alucita.com.tr",  IsActive = true }
        );
        await db.SaveChangesAsync();
    }

    // ── 8. KASALAR ────────────────────────────────────────────────────────────
    private static async Task<CashRegister> SeedCashRegistersAsync(ApplicationDbContext db)
    {
        if (await db.CashRegisters.AnyAsync())
            return await db.CashRegisters.FirstAsync();
        var ana    = new CashRegister { Name = "Ana Kasa",          Currency = "TRY", Balance = 0, IsActive = true };
        var doviz  = new CashRegister { Name = "USD Kasası",         Currency = "USD", Balance = 0, IsActive = true };
        var banka  = new CashRegister { Name = "Banka (Vakıfbank)", Currency = "TRY", Balance = 0, IsActive = true };
        db.CashRegisters.AddRange(ana, doviz, banka);
        await db.SaveChangesAsync();
        return ana;
    }

    // ── 9. STOK GİRİŞLERİ ────────────────────────────────────────────────────
    private static async Task SeedStockEntriesAsync(ApplicationDbContext db, Dictionary<string, ProductItem> p)
    {
        var entries = new[]
        {
            ("4mmDuz",    "Şişecam mal kabul — Nisan 2026",            720.5m, 8),
            ("5mmDuz",    "Trakya Cam mal kabul — Mayıs 2026",         450.0m, 5),
            ("6mmDuz",    "Şişecam mal kabul — Mayıs 2026",            360.0m, 4),
            ("8mmDuz",    "Trakya Cam mal kabul — Haziran 2026",        180.0m, 2),
            ("10mmDuz",   "Şişecam mal kabul — Haziran 2026",           90.0m, 1),
            ("4mmFume",   "Şişecam füme mal kabul",                     90.0m, 1),
            ("6mmFume",   "Şişecam füme mal kabul",                     72.0m, 1),
            ("4mmBronz",  "Şişecam bronz mal kabul",                    36.0m, 1),
            ("Temp6",     "Temperli cam mal kabul",                     45.0m, 1),
            ("Cita12",    "Alu Çıta 12mm mal kabul (metre)",           800.0m, 0),
            ("Cita16",    "Alu Çıta 16mm mal kabul (metre)",           600.0m, 0),
            ("Cita9",     "Alu Çıta 9mm mal kabul (metre)",            400.0m, 0),
            ("Butyl",     "Batı Kimya butyl bant (metre)",             500.0m, 0),
            ("Polisulfur","Batı Kimya polisülfür (kg)",                200.0m, 0),
            ("Silikon",   "Silikon yapıştırıcı (kg)",                   50.0m, 0),
            ("MolElek",   "Moleküler elek (kg)",                        80.0m, 0),
        };
        foreach (var (key, desc, qty, plateCount) in entries)
        {
            db.StockEntries.Add(new StockEntry
            {
                ProductItem = p[key],
                MovementType = StockMovementType.Giris,
                Quantity = qty,
                PlateCount = plateCount,
                ReferenceType = "PurchaseOrder",
                Description = desc
            });
        }
        await db.SaveChangesAsync();
    }

    // ── 10. SİPARİŞLER ───────────────────────────────────────────────────────
    private static async Task SeedOrdersAsync(
        ApplicationDbContext db,
        Dictionary<string, Customer> c,
        Dictionary<string, ProductItem> p,
        Dictionary<string, Recipe> r)
    {
        var now = TurkeyTime.Now;

        Order MakeOrder(string no, Customer cust, int daysAgo, int deliveryDays, OrderStatus status, string currency, int tax, string? notes = null) =>
            new()
            {
                OrderNumber = no, Customer = cust,
                OrderDate = now.AddDays(-daysAgo),
                DeliveryDate = now.AddDays(-daysAgo + deliveryDays),
                Status = status, Currency = currency, TaxRate = tax, Notes = notes
            };

        OrderLine Line(ProductItem? prod, Recipe? recipe, int w, int h, int qty, decimal unitPrice)
        {
            var area = (w > 0 && h > 0) ? w / 1000m * h / 1000m : 1m;
            return new OrderLine { ProductItem = prod, Recipe = recipe, WidthMm = w, HeightMm = h, Quantity = qty, UnitPrice = unitPrice, TotalPrice = unitPrice * area * qty };
        }

        // SIP-202605-001 — Artes — Tamamlandı
        var o1 = MakeOrder("SIP-202605-001", c["ARTES"], 45, 14, OrderStatus.Tamamlandi, "TRY", 20, "Konut projesi - zemin kat pencereleri");
        o1.ApprovedAt = now.AddDays(-44); o1.ProductionStartedAt = now.AddDays(-40); o1.CompletedAt = now.AddDays(-30);
        o1.Lines.Add(Line(null, r["IC_4_12_4"], 1200, 1400, 20, 210));
        o1.Lines.Add(Line(null, r["IC_4_12_4"],  900, 1200, 15, 210));
        o1.Lines.Add(Line(p["6mmDuz"], null,      600,  800, 10, 130));
        o1.TotalAmount = o1.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o1);

        // SIP-202605-002 — Öztürk — Tamamlandı
        var o2 = MakeOrder("SIP-202605-002", c["OZTURK"], 38, 14, OrderStatus.Tamamlandi, "TRY", 20, "Ofis bölmesi profilsiz cam");
        o2.ApprovedAt = now.AddDays(-37); o2.ProductionStartedAt = now.AddDays(-33); o2.CompletedAt = now.AddDays(-23);
        o2.Lines.Add(Line(p["6mmDuz"], null, 2400, 2200, 8, 130));
        o2.Lines.Add(Line(p["8mmDuz"], null, 1800, 2200, 4, 180));
        o2.TotalAmount = o2.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o2);

        // SIP-202605-003 — Tureks — Tamamlandı (EUR)
        var o3 = MakeOrder("SIP-202605-003", c["TUREKS"], 32, 21, OrderStatus.Tamamlandi, "EUR", 0, "Export sipariş - Avrupa ihracatı");
        o3.ApprovedAt = now.AddDays(-31); o3.ProductionStartedAt = now.AddDays(-27); o3.CompletedAt = now.AddDays(-10);
        o3.Lines.Add(Line(null, r["IC_5_12_5"], 1600, 2000, 30, 72));
        o3.Lines.Add(Line(null, r["IC_6_16_6"], 2000, 2400, 20, 98));
        o3.TotalAmount = o3.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o3);

        // SIP-202606-001 — Yaptek — Üretimde
        var o4 = MakeOrder("SIP-202606-001", c["YAPTEK"], 14, 21, OrderStatus.Uretimde, "TRY", 20, "Ankara projesi - 3. kat pencereler");
        o4.ApprovedAt = now.AddDays(-12); o4.ProductionStartedAt = now.AddDays(-8);
        o4.Lines.Add(Line(null, r["IC_6_12_6"], 1500, 1800, 12, 290));
        o4.Lines.Add(Line(null, r["IC_4_16_4"], 1200, 1500,  8, 230));
        o4.Lines.Add(Line(p["4mmDuz"], null,      600, 1000,  6,  85));
        o4.TotalAmount = o4.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o4);

        // SIP-202606-002 — Güler Cam — Onaylandı
        var o5 = MakeOrder("SIP-202606-002", c["GULER"], 7, 14, OrderStatus.Onaylandi, "TRY", 20, "Mağaza vitrin camı - büyük ölçü");
        o5.ApprovedAt = now.AddDays(-5);
        o5.Lines.Add(Line(p["6mmDuz"],  null, 3000, 2500, 6, 130));
        o5.Lines.Add(Line(p["8mmDuz"],  null, 2500, 2000, 4, 180));
        o5.Lines.Add(Line(p["Temp6"],   null, 1200, 2400, 2, 220));
        o5.TotalAmount = o5.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o5);

        // SIP-202606-003 — Metam — Onaylandı (USD)
        var o6 = MakeOrder("SIP-202606-003", c["METAM"], 5, 28, OrderStatus.Onaylandi, "USD", 0, "Cephe projesi export — USD fiyatlı");
        o6.ApprovedAt = now.AddDays(-3);
        o6.Lines.Add(Line(null, r["IC_5_12_5"], 2000, 3000, 24, 65));
        o6.Lines.Add(Line(null, r["IC_6_16_6"], 1800, 2800, 16, 95));
        o6.TotalAmount = o6.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o6);

        // SIP-202606-004 — Anadolu — Taslak
        var o7 = MakeOrder("SIP-202606-004", c["ANADOLU"], 2, 30, OrderStatus.Taslak, "TRY", 20, "Konya rezidans projesi - fiyat teklifi");
        o7.Lines.Add(Line(null, r["IC_4_12_4"], 1100, 1400, 40, 210));
        o7.Lines.Add(Line(null, r["IC_4_16_4"], 1100, 1400, 20, 230));
        o7.Lines.Add(Line(null, r["IC_5_16_5"], 1300, 1800, 10, 265));
        o7.TotalAmount = o7.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o7);

        // SIP-202606-005 — Barış — Taslak (bugün)
        var o8 = MakeOrder("SIP-202606-005", c["BARIS"], 0, 21, OrderStatus.Taslak, "TRY", 20, "Banyo bölme camı, perakende satış");
        o8.Lines.Add(Line(p["5mmDuz"],  null, 900, 1900, 2, 105));
        o8.Lines.Add(Line(p["4mmFume"], null, 800, 1500, 3, 100));
        o8.TotalAmount = o8.Lines.Sum(l => l.TotalPrice);
        db.Orders.Add(o8);

        await db.SaveChangesAsync();
    }

    // ── 11. MUHASEBe ─────────────────────────────────────────────────────────
    private static async Task SeedAccountingAsync(
        ApplicationDbContext db,
        Dictionary<string, Customer> c,
        CashRegister cashRegister)
    {
        var now = TurkeyTime.Now;

        // Döviz Kurları
        db.CurrencyRates.AddRange(
            new CurrencyRate { RateDate = now.AddDays(-30), CurrencyCode = "USD", BuyRate = 32.50m, SellRate = 32.55m },
            new CurrencyRate { RateDate = now.AddDays(-20), CurrencyCode = "USD", BuyRate = 33.10m, SellRate = 33.15m },
            new CurrencyRate { RateDate = now.AddDays(-10), CurrencyCode = "USD", BuyRate = 33.80m, SellRate = 33.85m },
            new CurrencyRate { RateDate = now.AddDays(-5),  CurrencyCode = "USD", BuyRate = 34.20m, SellRate = 34.25m },
            new CurrencyRate { RateDate = now,              CurrencyCode = "USD", BuyRate = 34.50m, SellRate = 34.55m },
            new CurrencyRate { RateDate = now.AddDays(-30), CurrencyCode = "EUR", BuyRate = 35.20m, SellRate = 35.25m },
            new CurrencyRate { RateDate = now.AddDays(-15), CurrencyCode = "EUR", BuyRate = 36.00m, SellRate = 36.05m },
            new CurrencyRate { RateDate = now,              CurrencyCode = "EUR", BuyRate = 36.80m, SellRate = 36.85m }
        );

        // Cari Hareketler
        void Borc(Customer cust, decimal amt, string curr, decimal rate, int daysAgo, string desc) =>
            db.AccountTransactions.Add(new AccountTransaction
            {
                Customer = cust, Type = TransactionType.Borc, PaymentType = PaymentType.AcikHesap,
                Amount = amt, Currency = curr, ExchangeRate = rate, AmountTRY = amt * rate,
                TransactionDate = now.AddDays(-daysAgo), Description = desc, ReferenceType = "Order"
            });
        void Alacak(Customer cust, decimal amt, PaymentType pt, int daysAgo, string desc) =>
            db.AccountTransactions.Add(new AccountTransaction
            {
                Customer = cust, Type = TransactionType.Alacak, PaymentType = pt,
                Amount = amt, Currency = "TRY", ExchangeRate = 1, AmountTRY = amt,
                TransactionDate = now.AddDays(-daysAgo), Description = desc, ReferenceType = "Manual"
            });

        // Artes (Tamamlandı — kısmi tahsilat)
        Borc(c["ARTES"],  85000m,   "TRY", 1,      44, "Sipariş onayı: SIP-202605-001");
        Alacak(c["ARTES"], 25000m,  PaymentType.Havale,   35, "Banka havalesi — kısmi ödeme");
        Alacak(c["ARTES"], 40000m,  PaymentType.Cek,      28, "Çek tahsilatı");

        // Öztürk (Tamamlandı — tam ödeme)
        Borc(c["OZTURK"], 71280m,   "TRY", 1,      37, "Sipariş onayı: SIP-202605-002");
        Alacak(c["OZTURK"], 71280m, PaymentType.Cek,      25, "Çek ile tam ödeme");

        // Tureks (Tamamlandı — EUR)
        Borc(c["TUREKS"], 87040m,   "EUR", 36.00m, 31, "Sipariş onayı: SIP-202605-003");
        Alacak(c["TUREKS"], 87040m * 36.00m, PaymentType.Havale, 10, "Döviz havalesi — tam ödeme");

        // Yaptek (Üretimde — avans)
        Borc(c["YAPTEK"], 82000m,   "TRY", 1,      12, "Sipariş onayı: SIP-202606-001");
        Alacak(c["YAPTEK"], 15000m, PaymentType.Nakit,     10, "Nakit avans ödeme");

        // Güler (Onaylandı — borç açık)
        Borc(c["GULER"],  61200m,   "TRY", 1,       5, "Sipariş onayı: SIP-202606-002");

        // Metam (Onaylandı — USD borç)
        Borc(c["METAM"],  24480m,   "USD", 34.20m,  3, "Sipariş onayı: SIP-202606-003");

        // Kasa hareketleri (tahsilatlar)
        db.CashTransactions.AddRange(
            new CashTransaction { CashRegister = cashRegister, Type = CashTransactionType.Giris, Amount = 25000m, PaymentType = PaymentType.Havale, Description = "Artes havale", TransactionDate = now.AddDays(-35) },
            new CashTransaction { CashRegister = cashRegister, Type = CashTransactionType.Giris, Amount = 15000m, PaymentType = PaymentType.Nakit,  Description = "Yaptek avans",  TransactionDate = now.AddDays(-10) }
        );

        // Giderler
        db.Expenses.AddRange(
            new Expense { Title = "Mayıs 2026 Kira",          Category = ExpenseCategoryType.Kira,    Amount = 45000m, Currency = "TRY", ExchangeRate = 1, AmountTRY = 45000m, PaymentType = PaymentType.Havale, ExpenseDate = now.AddDays(-25) },
            new Expense { Title = "Mayıs 2026 Maaşlar",       Category = ExpenseCategoryType.Maas,    Amount = 180000m,Currency = "TRY", ExchangeRate = 1, AmountTRY = 180000m,PaymentType = PaymentType.Havale, ExpenseDate = now.AddDays(-20) },
            new Expense { Title = "Nisan Elektrik Faturası",   Category = ExpenseCategoryType.Fatura,  Amount = 8500m,  Currency = "TRY", ExchangeRate = 1, AmountTRY = 8500m,  PaymentType = PaymentType.Havale, ExpenseDate = now.AddDays(-30) },
            new Expense { Title = "Haziran Nakliye",           Category = ExpenseCategoryType.Nakliye, Amount = 3200m,  Currency = "TRY", ExchangeRate = 1, AmountTRY = 3200m,  PaymentType = PaymentType.Nakit,  ExpenseDate = now.AddDays(-8)  },
            new Expense { Title = "Atölye Bakım-Onarım",       Category = ExpenseCategoryType.Bakim,   Amount = 5500m,  Currency = "TRY", ExchangeRate = 1, AmountTRY = 5500m,  PaymentType = PaymentType.Nakit,  ExpenseDate = now.AddDays(-15) },
            new Expense { Title = "Yakıt (Araç)",              Category = ExpenseCategoryType.Yakit,   Amount = 2800m,  Currency = "TRY", ExchangeRate = 1, AmountTRY = 2800m,  PaymentType = PaymentType.KrediKarti, ExpenseDate = now.AddDays(-5) }
        );

        // Çek / Senet takibi
        db.ChequeNotes.AddRange(
            new ChequeNote { Customer = c["OZTURK"], Type = ChequeNoteType.Cek,   DocumentNumber = "CK-2026-1234", Amount = 71280m,  Currency = "TRY", IssueDate = now.AddDays(-25), DueDate = now.AddDays(30),  BankName = "Akbank",   BranchName = "Bağcılar",  Status = ChequeNoteStatus.Portfoyde },
            new ChequeNote { Customer = c["ARTES"],  Type = ChequeNoteType.Senet, DocumentNumber = "SN-2026-0055", Amount = 20000m,  Currency = "TRY", IssueDate = now.AddDays(-40), DueDate = now.AddDays(20),  BankName = "İş Bankası", BranchName = "İkitelli", Status = ChequeNoteStatus.Portfoyde },
            new ChequeNote { Customer = c["GULER"],  Type = ChequeNoteType.Cek,   DocumentNumber = "CK-2026-5678", Amount = 30000m,  Currency = "TRY", IssueDate = now.AddDays(-5),  DueDate = now.AddDays(55),  BankName = "Garanti",  BranchName = "Kadıköy",  Status = ChequeNoteStatus.Portfoyde }
        );

        await db.SaveChangesAsync();
    }

    // ── 12. DEMO İŞ EMİRLERİ (optimizasyon testi için) ───────────────────────
    private static async Task SeedDemoWorkOrdersAsync(ApplicationDbContext db)
    {
        var customers = await db.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await db.ProductItems.Where(p => p.IsPlate && p.IsActive).ToListAsync();
        var recipes = await db.Recipes
            .Include(r => r.Layers)
            .ThenInclude(l => l.ProductItem)
            .Where(r => r.IsActive)
            .ToListAsync();

        if (!customers.Any() || !products.Any()) return;

        // Önceki demo iş emirlerini ve demo siparişlerini temizle
        var oldWorkOrders = await db.WorkOrders
            .Where(w => w.WorkOrderNumber.StartsWith("IE-DEMO-"))
            .Include(w => w.Lines)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.Items)
            .Include(w => w.Orders)
            .ToListAsync();
        foreach (var wo in oldWorkOrders)
        {
            foreach (var plan in wo.CuttingPlans.ToList())
            {
                foreach (var item in plan.Items.ToList())
                    db.CuttingPlanItems.Remove(item);
                db.CuttingPlans.Remove(plan);
            }
            foreach (var line in wo.Lines.ToList())
                db.WorkOrderLines.Remove(line);
            foreach (var woo in wo.Orders.ToList())
                db.WorkOrderOrders.Remove(woo);
            db.WorkOrders.Remove(wo);
        }

        var demoOrderNumbers = Enumerable.Range(1, 20).Select(i => $"SIP-DEMO-{i:000}").ToList();
        var oldDemoOrders = await db.Orders
            .Where(o => demoOrderNumbers.Contains(o.OrderNumber))
            .Include(o => o.Lines)
            .ToListAsync();
        foreach (var o in oldDemoOrders)
        {
            foreach (var line in o.Lines.ToList())
                db.OrderLines.Remove(line);
            db.Orders.Remove(o);
        }

        await db.SaveChangesAsync();

        var random = new Random(42); // deterministik sonuçlar
        var now = TurkeyTime.Now;

        for (int i = 1; i <= 20; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            var orderNumber = $"SIP-DEMO-{i:000}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                Customer = customer,
                OrderDate = now.AddDays(-random.Next(1, 10)),
                DeliveryDate = now.AddDays(random.Next(5, 20)),
                Status = OrderStatus.Onaylandi,
                Currency = "TRY",
                TaxRate = 20,
                ApprovedAt = now.AddDays(-random.Next(0, 5)),
                Notes = $"Demo sipariş #{i} - kesim optimizasyonu testi"
            };

            // Her demo siparişe 1-2 kalem ekle
            int lineCount = random.Next(1, 3);
            for (int j = 0; j < lineCount; j++)
            {
                bool useRecipe = random.Next(2) == 0 && recipes.Any();
                int w = random.Next(400, 2500);
                int h = random.Next(400, 2500);
                int qty = random.Next(2, 12);

                if (useRecipe)
                {
                    var recipe = recipes[random.Next(recipes.Count)];
                    var area = w / 1000m * h / 1000m;
                    order.Lines.Add(new OrderLine
                    {
                        Recipe = recipe,
                        WidthMm = w,
                        HeightMm = h,
                        Quantity = qty,
                        UnitPrice = recipe.BaseUnitPrice,
                        TotalPrice = recipe.BaseUnitPrice * area * qty
                    });
                }
                else
                {
                    var prod = products[random.Next(products.Count)];
                    var area = w / 1000m * h / 1000m;
                    order.Lines.Add(new OrderLine
                    {
                        ProductItem = prod,
                        WidthMm = w,
                        HeightMm = h,
                        Quantity = qty,
                        UnitPrice = prod.UnitPrice,
                        TotalPrice = prod.UnitPrice * area * qty
                    });
                }
            }
            order.TotalAmount = order.Lines.Sum(l => l.TotalPrice);
            db.Orders.Add(order);
            await db.SaveChangesAsync();

            // Siparişi üretime al
            order.Status = OrderStatus.Uretimde;
            order.ProductionStartedAt = now.AddDays(-random.Next(0, 3));
            await db.SaveChangesAsync();

            // İş emri oluştur
            var workOrderNumber = $"IE-DEMO-{i:000}";
            var workOrder = new WorkOrder
            {
                WorkOrderNumber = workOrderNumber,
                OrderId = order.Id,
                PlannedDate = now.AddDays(random.Next(1, 7)),
                Notes = $"Demo iş emri #{i} - optimize edilmemiş",
                IsCompleted = false
            };
            db.WorkOrders.Add(workOrder);
            await db.SaveChangesAsync();

            // WorkOrderOrder junction kaydı
            db.WorkOrderOrders.Add(new WorkOrderOrder { WorkOrderId = workOrder.Id, OrderId = order.Id });

            // Reçete patlatma: sipariş kalemlerinden malzeme listesi oluştur
            foreach (var line in order.Lines)
            {
                if (line.Recipe != null)
                {
                    foreach (var layer in line.Recipe.Layers)
                    {
                        db.WorkOrderLines.Add(new WorkOrderLine
                        {
                            WorkOrderId = workOrder.Id,
                            OrderLineId = line.Id,
                            ProductItemId = layer.ProductItemId,
                            WidthMm = line.WidthMm ?? 0,
                            HeightMm = line.HeightMm ?? 0,
                            Quantity = line.Quantity * layer.QuantityPerUnit,
                            MaterialType = layer.LayerType,
                            IsOptimized = false
                        });
                    }
                }
                else if (line.ProductItem != null)
                {
                    db.WorkOrderLines.Add(new WorkOrderLine
                    {
                        WorkOrderId = workOrder.Id,
                        OrderLineId = line.Id,
                        ProductItemId = line.ProductItemId.GetValueOrDefault(),
                        WidthMm = line.WidthMm ?? 0,
                        HeightMm = line.HeightMm ?? 0,
                        Quantity = line.Quantity,
                        MaterialType = line.ProductItem.IsPlate ? "Glass" : "Other",
                        IsOptimized = false
                    });
                }
            }
            await db.SaveChangesAsync();
        }
    }

    // ── 13. OPTİMİZE EDİLMEMİŞ DEMO SİPARİŞ KALEMLERİ ───────────────────────
    private static async Task SeedUnoptimizedDemoOrderLinesAsync(ApplicationDbContext db)
    {
        var customers = await db.Customers.Where(c => c.IsActive).ToListAsync();
        var products = await db.ProductItems.Where(p => p.IsPlate && p.IsActive).ToListAsync();
        var recipes = await db.Recipes
            .Include(r => r.Layers)
            .ThenInclude(l => l.ProductItem)
            .Where(r => r.IsActive)
            .ToListAsync();

        if (!customers.Any() || (!products.Any() && !recipes.Any())) return;

        // Önceki optimize edilmemiş demo siparişleri temizle
        var unoptOrderNumbers = Enumerable.Range(1, 20).Select(i => $"SIP-UNOPT-{i:000}").ToList();
        var oldUnoptOrders = await db.Orders
            .Where(o => unoptOrderNumbers.Contains(o.OrderNumber))
            .Include(o => o.Lines)
            .ToListAsync();
        foreach (var o in oldUnoptOrders)
        {
            foreach (var line in o.Lines.ToList())
                db.OrderLines.Remove(line);
            db.Orders.Remove(o);
        }
        await db.SaveChangesAsync();

        var random = new Random(2026); // deterministik
        var now = TurkeyTime.Now;

        for (int i = 1; i <= 20; i++)
        {
            var customer = customers[random.Next(customers.Count)];
            bool useRecipe = recipes.Any() && random.Next(2) == 0;
            int w = random.Next(400, 2600);
            int h = random.Next(400, 2600);
            int qty = random.Next(2, 15);

            var order = new Order
            {
                OrderNumber = $"SIP-UNOPT-{i:000}",
                Customer = customer,
                OrderDate = now.AddDays(-random.Next(1, 8)),
                DeliveryDate = now.AddDays(random.Next(7, 25)),
                Status = OrderStatus.Onaylandi,
                Currency = "TRY",
                TaxRate = 20,
                ApprovedAt = now.AddDays(-random.Next(0, 4)),
                Notes = $"Demo optimize edilmemiş sipariş #{i}",
                TotalAmount = 0
            };

            decimal unitPrice;
            decimal totalPrice;
            var area = w / 1000m * h / 1000m;

            if (useRecipe)
            {
                var recipe = recipes[random.Next(recipes.Count)];
                unitPrice = recipe.BaseUnitPrice;
                totalPrice = unitPrice * area * qty;
                order.Lines.Add(new OrderLine
                {
                    Recipe = recipe,
                    WidthMm = w,
                    HeightMm = h,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    IsOptimized = false
                });
            }
            else
            {
                var prod = products[random.Next(products.Count)];
                unitPrice = prod.UnitPrice;
                totalPrice = unitPrice * area * qty;
                order.Lines.Add(new OrderLine
                {
                    ProductItem = prod,
                    WidthMm = w,
                    HeightMm = h,
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    TotalPrice = totalPrice,
                    IsOptimized = false
                });
            }

            order.TotalAmount = order.Lines.Sum(l => l.TotalPrice);
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }
    }
}
