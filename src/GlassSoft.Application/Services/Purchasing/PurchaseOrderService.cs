using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Purchasing;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Purchasing;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Purchasing;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IRepository<PurchaseOrder> _repository;
    private readonly IRepository<PurchaseOrderLine> _lineRepository;
    private readonly IRepository<StockEntry> _stockRepository;
    private readonly IRepository<AccountTransaction> _transactionRepository;

    public PurchaseOrderService(
        IRepository<PurchaseOrder> repository,
        IRepository<PurchaseOrderLine> lineRepository,
        IRepository<StockEntry> stockRepository,
        IRepository<AccountTransaction> transactionRepository)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _stockRepository = stockRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Select(o => new PurchaseOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerTitle = o.Customer.Title,
                OrderDate = o.OrderDate,
                Status = o.Status,
                Currency = o.Currency,
                TaxRate = o.TaxRate,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                LineCount = o.Lines.Count
            })
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(o => o.Id == id)
            .Include(o => o.Customer)
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .Select(o => new PurchaseOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerTitle = o.Customer.Title,
                OrderDate = o.OrderDate,
                Status = o.Status,
                Currency = o.Currency,
                TaxRate = o.TaxRate,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                LineCount = o.Lines.Count,
                Lines = o.Lines.Select(l => new PurchaseOrderLineDto
                {
                    Id = l.Id,
                    ProductItemId = l.ProductItemId,
                    ProductItemName = l.ProductItem.Name,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    TotalPrice = l.TotalPrice,
                    ReceivedQuantity = l.ReceivedQuantity
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PurchaseOrderDto> CreateAsync(PurchaseOrderCreateDto dto)
    {
        var orderNumber = await GenerateOrderNumberAsync();
        var entity = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            CustomerId = dto.CustomerId,
            OrderDate = TurkeyTime.WithCurrentTime(dto.OrderDate),
            Currency = dto.Currency,
            TaxRate = dto.TaxRate,
            Notes = dto.Notes,
            Status = PurchaseOrderStatus.Taslak
        };

        decimal totalAmount = 0;
        foreach (var lineDto in dto.Lines)
            totalAmount += lineDto.Quantity * lineDto.UnitPrice;
        entity.TotalAmount = totalAmount;
        await _repository.AddAsync(entity);

        foreach (var lineDto in dto.Lines)
        {
            var line = new PurchaseOrderLine
            {
                PurchaseOrderId = entity.Id,
                ProductItemId = lineDto.ProductItemId,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                TotalPrice = lineDto.Quantity * lineDto.UnitPrice,
                ReceivedQuantity = 0
            };
            await _lineRepository.AddAsync(line);
        }

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(PurchaseOrderUpdateDto dto)
    {
        var entity = await _repository.Query()
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .FirstOrDefaultAsync(o => o.Id == dto.Id)
            ?? throw new KeyNotFoundException($"Satın alma siparişi bulunamadı: {dto.Id}");

        // Onaylı veya sonrası durumdaysa cari & stok yeniden senkron edilmeli
        var currentStatus = entity.Status;
        bool needsSync = currentStatus == PurchaseOrderStatus.Onaylandi
                        || currentStatus == PurchaseOrderStatus.KismiTeslim
                        || currentStatus == PurchaseOrderStatus.Tamamlandi;

        // Eski cari hareketi sil — SADECE bu sipariş'in cari'sinde, çapraz cari etkisini önle
        if (needsSync)
        {
            var oldTransactions = await _transactionRepository.Query()
                .Where(t => t.CustomerId == entity.CustomerId
                            && ((t.ReferenceType == "PurchaseOrder" && t.ReferenceId == entity.Id)
                                || (t.Description != null && t.Description.StartsWith($"Satın alma {entity.OrderNumber}"))))
                .ToListAsync();
            foreach (var tx in oldTransactions)
                await _transactionRepository.DeleteAsync(tx);

            // Eski stok girişlerini sil
            var oldStocks = await _stockRepository.Query()
                .Where(s => s.ReferenceType == "PurchaseOrder" && s.ReferenceId == entity.Id
                            && s.MovementType == StockMovementType.Giris)
                .ToListAsync();
            foreach (var s in oldStocks)
                await _stockRepository.DeleteAsync(s);
        }

        entity.CustomerId = dto.CustomerId;
        entity.OrderDate = TurkeyTime.WithCurrentTime(dto.OrderDate);
        entity.Currency = dto.Currency;
        entity.TaxRate = dto.TaxRate;
        entity.Notes = dto.Notes;

        // Mevcut satırları sil
        foreach (var line in entity.Lines.ToList())
            await _lineRepository.DeleteAsync(line);

        // Yeni satırları ekle
        decimal totalAmount = 0;
        var newLines = new List<PurchaseOrderLine>();
        foreach (var lineDto in dto.Lines)
        {
            var totalPrice = lineDto.Quantity * lineDto.UnitPrice;
            totalAmount += totalPrice;
            var line = new PurchaseOrderLine
            {
                PurchaseOrderId = entity.Id,
                ProductItemId = lineDto.ProductItemId,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                TotalPrice = totalPrice,
                ReceivedQuantity = 0
            };
            await _lineRepository.AddAsync(line);
            newLines.Add(line);
        }
        entity.TotalAmount = totalAmount;
        await _repository.UpdateAsync(entity);

        // Yeni cari hareket + stok girişlerini oluştur (yeni tutar ile)
        if (needsSync)
        {
            // Stok girişleri
            foreach (var line in newLines)
            {
                var stockEntry = new StockEntry
                {
                    ProductItemId = line.ProductItemId,
                    MovementType = StockMovementType.Giris,
                    Quantity = line.Quantity,
                    ReferenceType = "PurchaseOrder",
                    ReferenceId = entity.Id,
                    Description = $"Satın alma {entity.OrderNumber} (güncelleme)"
                };
                await _stockRepository.AddAsync(stockEntry);
            }

            // Cari borç hareketi (Alacak = Tediye = biz borçlanıyoruz)
            if (totalAmount > 0)
            {
                var grandTotal = totalAmount + (totalAmount * entity.TaxRate / 100m);
                var transaction = new AccountTransaction
                {
                    CustomerId = entity.CustomerId,
                    Type = TransactionType.Alacak,
                    PaymentType = PaymentType.AcikHesap,
                    Amount = grandTotal,
                    Currency = entity.Currency,
                    ExchangeRate = 1,
                    AmountTRY = grandTotal,
                    TransactionDate = entity.OrderDate,
                    ReferenceType = "PurchaseOrder",
                    ReferenceId = entity.Id,
                    Description = $"Satın alma {entity.OrderNumber} - Tedarikçi borcu (güncellendi)"
                };
                await _transactionRepository.AddAsync(transaction);
            }
        }
    }

    public async Task UpdateStatusAsync(int id, PurchaseOrderStatus status)
    {
        var entity = await _repository.Query()
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Satın alma siparişi bulunamadı: {id}");

        entity.Status = status;
        await _repository.UpdateAsync(entity);

        // Onay sonrası: stok girişi + cari borç hareketi
        if (status == PurchaseOrderStatus.Onaylandi)
        {
            // Her kalem için stok giriş hareketi
            foreach (var line in entity.Lines)
            {
                var stockEntry = new StockEntry
                {
                    ProductItemId = line.ProductItemId,
                    MovementType = StockMovementType.Giris,
                    Quantity = line.Quantity,
                    ReferenceType = "PurchaseOrder",
                    ReferenceId = entity.Id,
                    Description = $"Satın alma {entity.OrderNumber} - {line.ProductItem.Name}"
                };
                await _stockRepository.AddAsync(stockEntry);
            }

            // Cari borç hareketi (Alacak = Tediye = biz borçlanıyoruz)
            var grandTotal = entity.TotalAmount + (entity.TotalAmount * entity.TaxRate / 100m);
            var transaction = new AccountTransaction
            {
                CustomerId = entity.CustomerId,
                Type = TransactionType.Alacak, // Tediye - biz borçlanıyoruz
                PaymentType = PaymentType.AcikHesap,
                Amount = grandTotal,
                Currency = entity.Currency,
                ExchangeRate = 1,
                AmountTRY = grandTotal,
                TransactionDate = entity.OrderDate,
                ReferenceType = "PurchaseOrder",
                ReferenceId = entity.Id,
                Description = $"Satın alma {entity.OrderNumber} - Tedarikçi borcu"
            };
            await _transactionRepository.AddAsync(transaction);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Satın alma siparişi bulunamadı: {id}");

        foreach (var line in entity.Lines.ToList())
            await _lineRepository.DeleteAsync(line);
        await _repository.DeleteAsync(entity);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        // Format: SAT-yyyyMM-XXX (ay değişse de sıra yıl içinde devam eder, sadece yıl değişince sıfırlanır)
        var now = TurkeyTime.Now;
        var prefix = $"SAT-{now:yyyyMM}-";
        var yearAnyMonth = $"SAT-{now.Year}"; // bu yılın tüm ayları (SAT-2026...)

        var thisYearNumbers = await _repository.Query()
            .Where(o => o.OrderNumber.StartsWith(yearAnyMonth))
            .Select(o => o.OrderNumber)
            .ToListAsync();

        int maxSeq = 0;
        foreach (var num in thisYearNumbers)
        {
            var lastDash = num.LastIndexOf('-');
            if (lastDash > 0 && int.TryParse(num[(lastDash + 1)..], out var seq))
            {
                if (seq > maxSeq) maxSeq = seq;
            }
        }

        return prefix + (maxSeq + 1).ToString("D3");
    }

    public async Task<(int Updated, int Deleted, int Processed, List<string> Errors)> BackfillTransactionsAsync()
    {
        var orders = await _repository.Query()
            .Include(o => o.Customer)
            .ToListAsync();

        int updated = 0, deleted = 0, processed = 0;
        var errors = new List<string>();

        // GÜVENLİ BACKFILL: Hiçbir kaydı silmiyor, hiçbir kayda dokunmuyor.
        // Sadece "Onaylı/KısmiTeslim/Tamamlandı" durumdaki siparişler için
        // EKSİK cari hareketleri tespit edip ekler. Var olan kayıtlar olduğu gibi kalır.
        //
        // Bir sipariş için "var" sayılma kriteri:
        //   (CustomerId eşleşmesi) + (ReferenceType="PurchaseOrder" && ReferenceId=order.Id)
        //   VEYA (CustomerId eşleşmesi) + (Description="Satın alma {OrderNumber} ..." ile başlıyorsa)

        foreach (var order in orders)
        {
            processed++;
            try
            {
                bool shouldHaveTx = order.Status == PurchaseOrderStatus.Onaylandi
                                    || order.Status == PurchaseOrderStatus.KismiTeslim
                                    || order.Status == PurchaseOrderStatus.Tamamlandi;

                if (!shouldHaveTx || order.TotalAmount <= 0)
                    continue;

                // SADECE doğru cari'de bu sipariş için bir hareket var mı kontrol et
                var existsForThisOrder = await _transactionRepository.Query()
                    .AnyAsync(t => t.CustomerId == order.CustomerId
                                   && ((t.ReferenceType == "PurchaseOrder" && t.ReferenceId == order.Id)
                                       || (t.Description != null
                                           && t.Description.StartsWith($"Satın alma {order.OrderNumber}"))));

                if (existsForThisOrder)
                    continue; // zaten var, dokunma

                // Eksik — ekle
                var grandTotal = order.TotalAmount + (order.TotalAmount * order.TaxRate / 100m);
                var transaction = new AccountTransaction
                {
                    CustomerId = order.CustomerId,
                    Type = TransactionType.Alacak,
                    PaymentType = PaymentType.AcikHesap,
                    Amount = grandTotal,
                    Currency = order.Currency,
                    ExchangeRate = 1,
                    AmountTRY = grandTotal,
                    TransactionDate = order.OrderDate,
                    ReferenceType = "PurchaseOrder",
                    ReferenceId = order.Id,
                    Description = $"Satın alma {order.OrderNumber} - Tedarikçi borcu"
                };
                await _transactionRepository.AddAsync(transaction);
                updated++;
            }
            catch (Exception ex)
            {
                errors.Add($"{order.OrderNumber}: {ex.Message}");
            }
        }

        return (updated, deleted, processed, errors);
    }
}
