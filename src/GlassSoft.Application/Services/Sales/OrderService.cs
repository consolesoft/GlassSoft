using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Sales;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _repository;
    private readonly IRepository<OrderLine> _lineRepository;
    private readonly IRepository<OrderLineFeature> _featureRepository;
    private readonly IRepository<DeliveryLine> _deliveryLineRepository;
    private readonly ISystemSettingService _settingService;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IRepository<Order> repository,
        IRepository<OrderLine> lineRepository,
        IRepository<OrderLineFeature> featureRepository,
        IRepository<DeliveryLine> deliveryLineRepository,
        ISystemSettingService settingService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _featureRepository = featureRepository;
        _deliveryLineRepository = deliveryLineRepository;
        _settingService = settingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await _repository.Query()
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerTitle = o.Customer.Title,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                Currency = o.Currency,
                TaxRate = o.TaxRate,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                OrderCustomerName = o.OrderCustomerName,
                LineCount = o.Lines.Count,
                TotalQuantity = o.Lines.Sum(l => l.Quantity),
                TotalAreaM2 = o.Lines.Where(l => l.WidthMm.HasValue && l.HeightMm.HasValue && l.WidthMm.Value > 0 && l.HeightMm.Value > 0)
                                    .Sum(l => (l.WidthMm!.Value * l.HeightMm!.Value / 1_000_000m) * l.Quantity),
                ApprovedAt = o.ApprovedAt,
                ProductionStartedAt = o.ProductionStartedAt,
                CompletedAt = o.CompletedAt,
                CancelledAt = o.CancelledAt,
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        // Teslim edilen miktarları (onaylı teslimatlardan) tek sorguda çek ve eşleştir
        var orderIds = orders.Select(o => o.Id).ToList();
        var deliveredByOrder = await _deliveryLineRepository.Query()
            .Where(dl => !dl.IsDeleted && !dl.Delivery.IsDeleted
                         && dl.Delivery.Status == DeliveryStatus.Onaylandi
                         && orderIds.Contains(dl.Delivery.OrderId))
            .GroupBy(dl => dl.Delivery.OrderId)
            .Select(g => new { OrderId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.OrderId, x => x.Total);

        foreach (var o in orders)
        {
            o.DeliveredQuantity = deliveredByOrder.TryGetValue(o.Id, out var d) ? d : 0;
        }

        return orders;
    }

    public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatus status)
    {
        return await _repository.Query()
            .Where(o => o.Status == status)
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerTitle = o.Customer.Title,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                Currency = o.Currency,
                TaxRate = o.TaxRate,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                OrderCustomerName = o.OrderCustomerName,
                LineCount = o.Lines.Count,
                ApprovedAt = o.ApprovedAt,
                ProductionStartedAt = o.ProductionStartedAt,
                CompletedAt = o.CompletedAt,
                CancelledAt = o.CancelledAt,
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(o => o.Id == id)
            .Include(o => o.Customer)
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines).ThenInclude(l => l.Recipe)
            .Include(o => o.Lines).ThenInclude(l => l.Features).ThenInclude(f => f.FeatureDefinition)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerId = o.CustomerId,
                CustomerTitle = o.Customer.Title,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                Status = o.Status,
                Currency = o.Currency,
                TaxRate = o.TaxRate,
                TotalAmount = o.TotalAmount,
                Notes = o.Notes,
                OrderCustomerName = o.OrderCustomerName,
                LineCount = o.Lines.Count,
                Lines = o.Lines.Select(l => new OrderLineDto
                {
                    Id = l.Id,
                    ProductItemId = l.ProductItemId,
                    ProductItemName = l.ProductItem != null ? l.ProductItem.Name : null,
                    RecipeId = l.RecipeId,
                    RecipeCode = l.Recipe != null ? l.Recipe.Code : null,
                    PozNo = l.PozNo,
                    WidthMm = l.WidthMm,
                    HeightMm = l.HeightMm,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    TotalPrice = l.TotalPrice,
                    IsOptimized = l.IsOptimized,
                    Notes = l.Notes,
                    LabelPrintedAt = l.LabelPrintedAt,
                    Features = l.Features.Select(f => new OrderLineFeatureDto
                    {
                        Id = f.Id,
                        FeatureDefinitionId = f.FeatureDefinitionId,
                        FeatureName = f.FeatureDefinition.Name,
                        Quantity = f.Quantity,
                        UnitPrice = f.UnitPrice,
                        TotalPrice = f.UnitPrice * f.Quantity
                    }).ToList()
                }).ToList(),
                ApprovedAt = o.ApprovedAt,
                ProductionStartedAt = o.ProductionStartedAt,
                CompletedAt = o.CompletedAt,
                CancelledAt = o.CancelledAt,
                CreatedAt = o.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OrderDto> CreateAsync(OrderCreateDto dto)
    {
        var minM2 = await _settingService.GetDecimalAsync("MinM2");
        var orderNumber = await GenerateOrderNumberAsync();
        var entity = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = dto.CustomerId,
            OrderDate = TurkeyTime.WithCurrentTime(dto.OrderDate),
            DeliveryDate = dto.DeliveryDate,
            Currency = dto.Currency,
            TaxRate = dto.TaxRate,
            Notes = dto.Notes,
            OrderCustomerName = dto.OrderCustomerName,
            Status = OrderStatus.Taslak
        };

        decimal totalAmount = 0;
        foreach (var lineDto in dto.Lines)
        {
            var isRetail = !lineDto.WidthMm.HasValue || !lineDto.HeightMm.HasValue || (lineDto.WidthMm == 0 && lineDto.HeightMm == 0);
            var areaM2 = isRetail ? 0 : ((lineDto.WidthMm!.Value * lineDto.HeightMm!.Value) / 1_000_000m);
            var effectiveM2 = (!isRetail && minM2 > 0 && areaM2 > 0 && areaM2 < minM2) ? minM2 : areaM2;
            var totalPrice = isRetail ? lineDto.UnitPrice * lineDto.Quantity : lineDto.UnitPrice * effectiveM2 * lineDto.Quantity;
            totalAmount += totalPrice;
        }
        entity.TotalAmount = totalAmount;
        await _repository.AddAsync(entity);

        foreach (var lineDto in dto.Lines)
        {
            var isRetail = !lineDto.WidthMm.HasValue || !lineDto.HeightMm.HasValue || (lineDto.WidthMm == 0 && lineDto.HeightMm == 0);
            var areaM2 = isRetail ? 0 : ((lineDto.WidthMm!.Value * lineDto.HeightMm!.Value) / 1_000_000m);
            var effectiveM2 = (!isRetail && minM2 > 0 && areaM2 > 0 && areaM2 < minM2) ? minM2 : areaM2;
            var totalPrice = isRetail ? lineDto.UnitPrice * lineDto.Quantity : lineDto.UnitPrice * effectiveM2 * lineDto.Quantity;
            var line = new OrderLine
            {
                OrderId = entity.Id,
                ProductItemId = lineDto.ProductItemId,
                RecipeId = lineDto.RecipeId,
                PozNo = lineDto.PozNo,
                WidthMm = lineDto.WidthMm,
                HeightMm = lineDto.HeightMm,
                Quantity = lineDto.Quantity,
                UnitPrice = lineDto.UnitPrice,
                TotalPrice = totalPrice,
                Notes = lineDto.Notes
            };
            await _lineRepository.AddAsync(line);

            // Kalem özelliklerini kaydet
            if (lineDto.Features != null)
            {
                foreach (var featureDto in lineDto.Features)
                {
                    var feature = new OrderLineFeature
                    {
                        OrderLineId = line.Id,
                        FeatureDefinitionId = featureDto.FeatureDefinitionId,
                        Quantity = featureDto.Quantity,
                        UnitPrice = featureDto.UnitPrice
                    };
                    await _featureRepository.AddAsync(feature);
                    totalAmount += feature.TotalPrice;
                }
            }
        }
        // Toplam tutarı özelliklerle güncelle
        entity.TotalAmount = totalAmount;
        await _repository.UpdateAsync(entity);

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(OrderUpdateDto dto)
    {
        var minM2 = await _settingService.GetDecimalAsync("MinM2");
        var entity = await _repository.Query()
            .Include(o => o.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.Features.Where(f => !f.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == dto.Id)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {dto.Id}");

        entity.CustomerId = dto.CustomerId;
        entity.OrderDate = TurkeyTime.WithCurrentTime(dto.OrderDate);
        entity.DeliveryDate = dto.DeliveryDate;
        // CompletedAt: dto null ise mevcut değeri KORU.
        // Autosave/eksik form submit'lerinde CompletedAt boş gelirse mevcut teslim tarihi kaybolmasın.
        if (dto.CompletedAt.HasValue) entity.CompletedAt = dto.CompletedAt;
        entity.Currency = dto.Currency;
        entity.TaxRate = dto.TaxRate;
        entity.Notes = dto.Notes;
        entity.OrderCustomerName = dto.OrderCustomerName;

        // ID-based eşleme: mevcut kalemler korunur (id sabit kalır),
        // böylece teslimat satırlarındaki OrderLineId referansları bozulmaz.
        var now = TurkeyTime.Now;
        var existingLines = entity.Lines.Where(l => !l.IsDeleted).ToDictionary(l => l.Id);
        var keepIds = dto.Lines.Where(l => l.Id > 0).Select(l => l.Id).ToHashSet();

        // 1) DTO'da olmayan eski kalemleri soft-delete et
        foreach (var (id, line) in existingLines)
        {
            if (!keepIds.Contains(id))
            {
                foreach (var f in line.Features.Where(f => !f.IsDeleted).ToList())
                {
                    f.IsDeleted = true;
                    f.DeletedAt = now;
                }
                line.IsDeleted = true;
                line.DeletedAt = now;
            }
        }

        // 2) DTO satırlarını işle: mevcut id'leri UPDATE, id=0 olanları ADD
        decimal totalAmount = 0;
        foreach (var lineDto in dto.Lines)
        {
            var isRetail = !lineDto.WidthMm.HasValue || !lineDto.HeightMm.HasValue || (lineDto.WidthMm == 0 && lineDto.HeightMm == 0);
            var areaM2 = isRetail ? 0 : ((lineDto.WidthMm!.Value * lineDto.HeightMm!.Value) / 1_000_000m);
            var effectiveM2 = (!isRetail && minM2 > 0 && areaM2 > 0 && areaM2 < minM2) ? minM2 : areaM2;
            var totalPrice = isRetail ? lineDto.UnitPrice * lineDto.Quantity : lineDto.UnitPrice * effectiveM2 * lineDto.Quantity;
            totalAmount += totalPrice;

            OrderLine line;
            if (lineDto.Id > 0 && existingLines.TryGetValue(lineDto.Id, out var existing))
            {
                // UPDATE: mevcut kalemi güncelle (id korunur — teslimat referansları sağlam kalır)
                line = existing;
                line.ProductItemId = lineDto.ProductItemId;
                line.RecipeId = lineDto.RecipeId;
                line.PozNo = lineDto.PozNo;
                line.WidthMm = lineDto.WidthMm;
                line.HeightMm = lineDto.HeightMm;
                line.Quantity = lineDto.Quantity;
                line.UnitPrice = lineDto.UnitPrice;
                line.TotalPrice = totalPrice;
                line.Notes = lineDto.Notes;

                // Mevcut feature'ları soft-delete, yenilerini ekle (feature id'leri sabit tutmaya gerek yok)
                foreach (var f in line.Features.Where(f => !f.IsDeleted).ToList())
                {
                    f.IsDeleted = true;
                    f.DeletedAt = now;
                }
            }
            else
            {
                // ADD: yeni kalem
                line = new OrderLine
                {
                    OrderId = entity.Id,
                    ProductItemId = lineDto.ProductItemId,
                    RecipeId = lineDto.RecipeId,
                    PozNo = lineDto.PozNo,
                    WidthMm = lineDto.WidthMm,
                    HeightMm = lineDto.HeightMm,
                    Quantity = lineDto.Quantity,
                    UnitPrice = lineDto.UnitPrice,
                    TotalPrice = totalPrice,
                    Notes = lineDto.Notes
                };
                entity.Lines.Add(line);
            }

            if (lineDto.Features != null)
            {
                foreach (var featureDto in lineDto.Features)
                {
                    if (featureDto.FeatureDefinitionId <= 0) continue;
                    line.Features.Add(new OrderLineFeature
                    {
                        FeatureDefinitionId = featureDto.FeatureDefinitionId,
                        Quantity = featureDto.Quantity,
                        UnitPrice = featureDto.UnitPrice
                    });
                    totalAmount += featureDto.UnitPrice * featureDto.Quantity;
                }
            }
        }
        entity.TotalAmount = totalAmount;

        // 3) Tek SaveChanges: EF tracking zaten entity'yi ve Lines collection'ını izliyor.
        //    DbSet.Update(entity) ÇAĞIRMIYORUZ — aksi halde navigation'daki mevcut kalemleri
        //    yeniden Modified olarak işaretleyip hatalı davranışa sebep olabilir.
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, OrderStatus status)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {id}");
        entity.Status = status;

        // Durum geçiş tarihlerini kaydet
        var now = TurkeyTime.Now;
        switch (status)
        {
            case OrderStatus.Onaylandi:
                entity.ApprovedAt ??= now;
                break;
            case OrderStatus.Uretimde:
                entity.ProductionStartedAt ??= now;
                if (entity.ApprovedAt == null) entity.ApprovedAt = now;
                break;
            case OrderStatus.Tamamlandi:
                entity.CompletedAt ??= now;
                break;
            case OrderStatus.IptalEdildi:
                entity.CancelledAt ??= now;
                break;
            case OrderStatus.Taslak:
                // Taslağa geri dönerse önceki durum tarihlerini koru (audit için)
                break;
        }

        await _repository.UpdateAsync(entity);
    }

    public async Task SetDeliveryDateAsync(int id, DateTime date)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {id}");
        entity.DeliveryDate = date;
        await _repository.UpdateAsync(entity);
    }

    public async Task SetCompletedAtAsync(int id, DateTime date)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {id}");
        entity.CompletedAt = date;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(o => o.Lines).ThenInclude(l => l.Features)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {id}");

        // Soft delete: doğrudan IsDeleted işaretle, tek SaveChanges ile kaydet
        // EF Remove() kullanırsak Restrict FK yüzünden "association severed" hatası alırız
        var now = TurkeyTime.Now;
        foreach (var line in entity.Lines)
        {
            foreach (var feature in line.Features)
            {
                feature.IsDeleted = true;
                feature.DeletedAt = now;
            }
            line.IsDeleted = true;
            line.DeletedAt = now;
        }
        entity.IsDeleted = true;
        entity.DeletedAt = now;

        await _repository.UpdateAsync(entity);
    }

    public async Task<IEnumerable<OrderLinePriceHistoryDto>> GetPriceHistoryAsync(int? productItemId, int? recipeId, int count = 10)
    {
        IQueryable<OrderLine> query;

        if (productItemId.HasValue)
            query = _lineRepository.Query().Where(l => l.ProductItemId == productItemId.Value);
        else if (recipeId.HasValue)
            query = _lineRepository.Query().Where(l => l.RecipeId == recipeId.Value);
        else
            return Enumerable.Empty<OrderLinePriceHistoryDto>();

        return await query
            .Include(l => l.Order).ThenInclude(o => o.Customer)
            .OrderByDescending(l => l.Order.OrderDate)
            .Take(count)
            .Select(l => new OrderLinePriceHistoryDto
            {
                OrderNumber = l.Order.OrderNumber,
                OrderDate = l.Order.OrderDate,
                CustomerTitle = l.Order.Customer.Title,
                WidthMm = l.WidthMm,
                HeightMm = l.HeightMm,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice
            })
            .ToListAsync();
    }

    public async Task<OrderLineDto?> GetLineByIdAsync(int lineId)
    {
        var line = await _lineRepository.Query()
            .Include(l => l.ProductItem)
            .Include(l => l.Recipe)
            .Include(l => l.Order)
            .FirstOrDefaultAsync(l => l.Id == lineId);
        if (line == null) return null;
        return new OrderLineDto
        {
            Id = line.Id,
            ProductItemId = line.ProductItemId,
            ProductItemName = line.ProductItem?.Name,
            RecipeId = line.RecipeId,
            RecipeCode = line.Recipe?.Code,
            PozNo = line.PozNo,
            WidthMm = line.WidthMm,
            HeightMm = line.HeightMm,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            TotalPrice = line.TotalPrice,
            IsOptimized = line.IsOptimized,
            Notes = line.Notes,
            LabelPrintedAt = line.LabelPrintedAt
        };
    }

    public async Task SetLabelPrintedAsync(int lineId)
    {
        var line = await _lineRepository.GetByIdAsync(lineId)
            ?? throw new KeyNotFoundException($"Sipariş kalemi bulunamadı: {lineId}");
        line.LabelPrintedAt = TurkeyTime.Now;
        await _lineRepository.UpdateAsync(line);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        // Format: SIP-yyyyMM-XXX (ay değişse de sıra yıl içinde devam eder, sadece yıl değişince sıfırlanır)
        var now = TurkeyTime.Now;
        var prefix = $"SIP-{now:yyyyMM}-";
        var yearAnyMonth = $"SIP-{now.Year}"; // bu yılın tüm ayları (SIP-2026...)

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
}
