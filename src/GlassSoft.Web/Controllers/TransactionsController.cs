using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GlassSoft.Web.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private readonly IAccountTransactionService _transactionService;
    private readonly ICustomerService _customerService;
    private readonly ICashRegisterService _cashRegisterService;
    private readonly ICashTransactionService _cashTransactionService;
    private readonly IPrintTemplateService _templateService;

    public TransactionsController(
        IAccountTransactionService transactionService,
        ICustomerService customerService,
        ICashRegisterService cashRegisterService,
        ICashTransactionService cashTransactionService,
        IPrintTemplateService templateService)
    {
        _transactionService = transactionService;
        _customerService = customerService;
        _cashRegisterService = cashRegisterService;
        _cashTransactionService = cashTransactionService;
        _templateService = templateService;
    }

    public async Task<IActionResult> Index(int? customerId, string? orderNumber)
    {
        IEnumerable<AccountTransactionDto> transactions;
        if (customerId.HasValue)
        {
            transactions = await _transactionService.GetByCustomerAsync(customerId.Value);
            var customer = await _customerService.GetByIdAsync(customerId.Value);
            ViewBag.CustomerTitle = customer?.Title;
            ViewBag.CustomerId = customerId.Value;
            ViewBag.CustomerBalance = await _transactionService.GetCustomerBalanceAsync(customerId.Value);
        }
        else
        {
            transactions = await _transactionService.GetAllAsync();
        }

        // Sipariş numarasına göre filtre (sipariş detayındaki "Cari Hareketler" butonu için)
        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            transactions = transactions.Where(t => t.Description != null && t.Description.Contains(orderNumber));
            ViewBag.OrderNumberFilter = orderNumber;
        }

        return View(transactions);
    }

    public async Task<IActionResult> Create(int? customerId)
    {
        await LoadSelectLists();
        var dto = new AccountTransactionCreateDto();
        if (customerId.HasValue)
            dto.CustomerId = customerId.Value;
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tx = await _transactionService.GetByIdAsync(id);
        if (tx == null) return NotFound();
        await LoadSelectLists();

        // Bu cari harekete bağlı kasa hareketi var mı?
        var allCash = await _cashTransactionService.GetAllAsync();
        var linkedCash = allCash.FirstOrDefault(c => c.ReferenceType == "AccountTransaction" && c.ReferenceId == id);
        ViewBag.CurrentCashRegisterId = linkedCash?.CashRegisterId;

        return View(new AccountTransactionUpdateDto
        {
            Id = tx.Id,
            CustomerId = tx.CustomerId,
            Type = tx.Type,
            PaymentType = tx.PaymentType,
            Amount = tx.Amount,
            Currency = tx.Currency,
            ExchangeRate = tx.ExchangeRate,
            TransactionDate = tx.TransactionDate,
            Description = tx.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AccountTransactionUpdateDto dto, int? cashRegisterId)
    {
        // Kasa seçimi zorunlu
        if (!cashRegisterId.HasValue || cashRegisterId.Value <= 0)
        {
            TempData["Error"] = "Kasa seçmediniz, devam edemezsiniz. Lütfen bir kasa seçin.";
            await LoadSelectLists();
            return View(dto);
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }

        // Mevcut bağlı kasa hareketini sil (hem kasa değişimi hem amount/type senkronu için en temizi)
        var allCash = await _cashTransactionService.GetAllAsync();
        var linkedCash = allCash.FirstOrDefault(c => c.ReferenceType == "AccountTransaction" && c.ReferenceId == dto.Id);
        if (linkedCash != null)
            await _cashTransactionService.DeleteAsync(linkedCash.Id);

        await _transactionService.UpdateAsync(dto);

        // Kasa seçildiyse yeni kasa hareketi oluştur
        if (cashRegisterId.HasValue && cashRegisterId.Value > 0)
        {
            var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;
            var kasaTuru = dto.Type == TransactionType.Alacak ? CashTransactionType.Giris : CashTransactionType.Cikis;
            var aciklama = dto.Type == TransactionType.Alacak ? "Tahsilat: Cari hareket" : "Tediye: Cari hareket";
            await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
            {
                CashRegisterId = cashRegisterId.Value,
                Type = kasaTuru,
                Amount = amountTRY,
                TransactionDate = dto.TransactionDate,
                PaymentType = dto.PaymentType,
                Description = aciklama
            }, "AccountTransaction", dto.Id);
        }

        TempData["Success"] = "Cari hareket güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _transactionService.DeleteAsync(id);
            TempData["Success"] = "Cari hareket silindi. Bağlı kasa hareketi varsa o da silindi, bakiyeler güncellendi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Silme hatası: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Virman()
    {
        await LoadSelectLists();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Virman(int sourceCustomerId, int targetCustomerId, decimal amount, DateTime transactionDate, int? cashRegisterId, string? description)
    {
        if (sourceCustomerId == targetCustomerId)
        {
            TempData["Error"] = "Kaynak ve hedef cari aynı olamaz.";
            return RedirectToAction(nameof(Virman));
        }
        if (amount <= 0)
        {
            TempData["Error"] = "Tutar sıfırdan büyük olmalıdır.";
            return RedirectToAction(nameof(Virman));
        }

        var sourceCustomer = await _customerService.GetByIdAsync(sourceCustomerId);
        var targetCustomer = await _customerService.GetByIdAsync(targetCustomerId);
        if (sourceCustomer == null || targetCustomer == null)
        {
            TempData["Error"] = "Cari bulunamadı.";
            return RedirectToAction(nameof(Virman));
        }

        var desc = string.IsNullOrWhiteSpace(description) ? "" : $" - {description}";

        // Kaynak cari: Alacak (bakiyesi azalır)
        var sourceTx = await _transactionService.CreateAsync(new AccountTransactionCreateDto
        {
            CustomerId = sourceCustomerId,
            Type = TransactionType.Alacak,
            PaymentType = PaymentType.Nakit,
            Amount = amount,
            Currency = "TRY",
            ExchangeRate = 1,
            TransactionDate = transactionDate,
            Description = $"Virman → {targetCustomer.Title}{desc}"
        });

        // Hedef cari: Borç (bakiyesi artar)
        var targetTx = await _transactionService.CreateAsync(new AccountTransactionCreateDto
        {
            CustomerId = targetCustomerId,
            Type = TransactionType.Borc,
            PaymentType = PaymentType.Nakit,
            Amount = amount,
            Currency = "TRY",
            ExchangeRate = 1,
            TransactionDate = transactionDate,
            Description = $"Virman ← {sourceCustomer.Title}{desc}"
        });

        // Kasa seçildiyse: kaynak için giriş, hedef için çıkış (net sıfır ama izlenebilir)
        if (cashRegisterId.HasValue && cashRegisterId.Value > 0)
        {
            await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
            {
                CashRegisterId = cashRegisterId.Value,
                Type = CashTransactionType.Giris,
                Amount = amount,
                TransactionDate = transactionDate,
                PaymentType = PaymentType.Nakit,
                Description = $"Virman girişi: {sourceCustomer.Title} → {targetCustomer.Title}{desc}"
            }, "AccountTransaction", sourceTx.Id);

            await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
            {
                CashRegisterId = cashRegisterId.Value,
                Type = CashTransactionType.Cikis,
                Amount = amount,
                TransactionDate = transactionDate,
                PaymentType = PaymentType.Nakit,
                Description = $"Virman çıkışı: {sourceCustomer.Title} → {targetCustomer.Title}{desc}"
            }, "AccountTransaction", targetTx.Id);
        }

        TempData["Success"] = $"Virman başarıyla gerçekleşti: {sourceCustomer.Title} → {targetCustomer.Title} ({amount:N2} TRY)";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountTransactionCreateDto dto, int? cashRegisterId, bool isOpeningBalance = false)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(dto);
        }

        // Cari hesap devri değilse kasa seçimi zorunlu
        if (!isOpeningBalance && (!cashRegisterId.HasValue || cashRegisterId.Value <= 0))
        {
            TempData["Error"] = "Kasa seçmediniz, devam edemezsiniz. Lütfen bir kasa seçin.";
            await LoadSelectLists();
            return View(dto);
        }

        var transaction = await _transactionService.CreateAsync(dto);

        // Cari hesap devri ise kasayı etkileme
        if (isOpeningBalance)
        {
            TempData["Success"] = "Cari hesap devri başarıyla kaydedildi. (Kasayı etkilemedi)";
            return RedirectToAction(nameof(Index));
        }

        // Kasa seçildiyse otomatik kasa hareketi oluştur
        if (cashRegisterId.HasValue && cashRegisterId.Value > 0)
        {
            var amountTRY = dto.Currency == "TRY" ? dto.Amount : dto.Amount * dto.ExchangeRate;
            // Borç = cariye borç yazıyoruz (tedarikçiye ödeme, borç kapatma) → kasadan çıkış
            // Alacak = cariden tahsilat alıyoruz (müşteri ödeme yapıyor) → kasaya giriş
            var kasaTuru = dto.Type == TransactionType.Alacak ? CashTransactionType.Giris : CashTransactionType.Cikis;
            var aciklama = dto.Type == TransactionType.Alacak ? "Tahsilat: Cari hareket" : "Tediye: Cari hareket";
            await _cashTransactionService.CreateWithReferenceAsync(new CashTransactionCreateDto
            {
                CashRegisterId = cashRegisterId.Value,
                Type = kasaTuru,
                Amount = amountTRY,
                TransactionDate = dto.TransactionDate,
                PaymentType = dto.PaymentType,
                Description = aciklama
            }, "AccountTransaction", transaction.Id);
        }

        var msg = "Cari hareket başarıyla oluşturuldu.";
        if (cashRegisterId.HasValue && cashRegisterId.Value > 0)
            msg += dto.Type == TransactionType.Alacak ? " (Kasa girişi oluşturuldu)" : " (Kasa çıkışı oluşturuldu)";
        TempData["Success"] = msg;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> BalanceList(string? balanceFilter)
    {
        var customers = (await _customerService.GetAllAsync()).Where(c => c.IsActive).ToList();
        var balances = new List<CustomerBalanceRow>();

        foreach (var c in customers)
        {
            var balance = await _transactionService.GetCustomerBalanceAsync(c.Id);
            balances.Add(new CustomerBalanceRow
            {
                CustomerId = c.Id,
                CustomerTitle = c.Title,
                CustomerCode = c.Code,
                Balance = balance,
                BalanceDirection = balance > 0 ? "Borç" : balance < 0 ? "Alacak" : "Sıfır"
            });
        }

        // Toplamları filtresiz hesapla
        ViewBag.ToplamBorc = balances.Where(b => b.Balance > 0).Sum(b => b.Balance);
        ViewBag.ToplamAlacak = balances.Where(b => b.Balance < 0).Sum(b => Math.Abs(b.Balance));

        // Borç / Alacak filtresi uygula
        if (balanceFilter == "borc")
            balances = balances.Where(b => b.Balance > 0).ToList();
        else if (balanceFilter == "alacak")
            balances = balances.Where(b => b.Balance < 0).ToList();

        ViewBag.BalanceFilter = balanceFilter ?? "";
        ViewBag.FilterList = new SelectList(new[]
        {
            new { Value = "", Text = "Tümü" },
            new { Value = "borc", Text = "Borçlular" },
            new { Value = "alacak", Text = "Alacaklılar" }
        }, "Value", "Text", balanceFilter ?? "");

        var templates = await _templateService.GetByOutputTypeAsync(OutputType.CariBakiyeListesi);
        var defTemplate = templates.FirstOrDefault(t => t.IsDefault) ?? templates.FirstOrDefault();
        ViewBag.PrintTemplateId = defTemplate?.Id ?? 0;

        return View(balances);
    }

    public class CustomerBalanceRow
    {
        public int CustomerId { get; set; }
        public string CustomerTitle { get; set; } = string.Empty;
        public string CustomerCode { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string BalanceDirection { get; set; } = string.Empty;
    }

    private async Task LoadSelectLists()
    {
        var customers = await _customerService.GetAllAsync();
        ViewBag.Customers = new SelectList(customers.Where(c => c.IsActive), "Id", "Title");

        var registers = await _cashRegisterService.GetAllAsync();
        ViewBag.CashRegisters = new SelectList(registers, "Id", "Name");
    }
}
