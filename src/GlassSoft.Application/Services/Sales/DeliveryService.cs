using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Sales;

public class DeliveryService : IDeliveryService
{
    private readonly IRepository<Delivery> _repository;
    private readonly IRepository<DeliveryLine> _lineRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderLine> _orderLineRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeliveryService(
        IRepository<Delivery> repository,
        IRepository<DeliveryLine> lineRepository,
        IRepository<Order> orderRepository,
        IRepository<OrderLine> orderLineRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DeliveryDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(d => d.Order).ThenInclude(o => o.Customer)
            .Include(d => d.Lines).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.ProductItem)
            .Include(d => d.Lines).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.Recipe)
            .Select(d => new DeliveryDto
            {
                Id = d.Id,
                DeliveryNumber = d.DeliveryNumber,
                OrderId = d.OrderId,
                OrderNumber = d.Order.OrderNumber,
                CustomerId = d.Order.CustomerId,
                CustomerTitle = d.Order.Customer.Title,
                DeliveryDate = d.DeliveryDate,
                Status = d.Status,
                ApprovedAt = d.ApprovedAt,
                Notes = d.Notes,
                CreatedAt = d.CreatedAt,
                LineCount = d.Lines.Count,
                TotalQuantity = d.Lines.Sum(l => l.Quantity),
                TotalAreaM2 = d.Lines.Where(l => l.OrderLine.WidthMm.HasValue && l.OrderLine.HeightMm.HasValue
                                                && l.OrderLine.WidthMm.Value > 0 && l.OrderLine.HeightMm.Value > 0)
                                     .Sum(l => (l.OrderLine.WidthMm!.Value * l.OrderLine.HeightMm!.Value / 1_000_000m) * l.Quantity)
            })
            .OrderByDescending(d => d.DeliveryDate)
            .ThenByDescending(d => d.Id)
            .ToListAsync();
    }

    public async Task<DeliveryDto?> GetByIdAsync(int id)
    {
        var delivery = await _repository.Query()
            .Include(d => d.Order).ThenInclude(o => o.Customer)
            .Include(d => d.Lines).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.ProductItem)
            .Include(d => d.Lines).ThenInclude(l => l.OrderLine).ThenInclude(ol => ol.Recipe)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (delivery == null) return null;

        return new DeliveryDto
        {
            Id = delivery.Id,
            DeliveryNumber = delivery.DeliveryNumber,
            OrderId = delivery.OrderId,
            OrderNumber = delivery.Order.OrderNumber,
            CustomerId = delivery.Order.CustomerId,
            CustomerTitle = delivery.Order.Customer.Title,
            DeliveryDate = delivery.DeliveryDate,
            Status = delivery.Status,
            ApprovedAt = delivery.ApprovedAt,
            Notes = delivery.Notes,
            CreatedAt = delivery.CreatedAt,
            LineCount = delivery.Lines.Count(l => !l.IsDeleted),
            TotalQuantity = delivery.Lines.Where(l => !l.IsDeleted).Sum(l => l.Quantity),
            TotalAreaM2 = delivery.Lines.Where(l => !l.IsDeleted
                                                  && l.OrderLine.WidthMm.HasValue && l.OrderLine.HeightMm.HasValue
                                                  && l.OrderLine.WidthMm.Value > 0 && l.OrderLine.HeightMm.Value > 0)
                                          .Sum(l => (l.OrderLine.WidthMm!.Value * l.OrderLine.HeightMm!.Value / 1_000_000m) * l.Quantity),
            Lines = delivery.Lines.Where(l => !l.IsDeleted).Select(l => new DeliveryLineDto
            {
                Id = l.Id,
                DeliveryId = l.DeliveryId,
                OrderLineId = l.OrderLineId,
                ProductName = l.OrderLine.ProductItem != null ? l.OrderLine.ProductItem.Name :
                              (l.OrderLine.Recipe != null ? l.OrderLine.Recipe.Code : "-"),
                RecipeCode = l.OrderLine.Recipe != null ? l.OrderLine.Recipe.Code : null,
                WidthMm = l.OrderLine.WidthMm,
                HeightMm = l.OrderLine.HeightMm,
                OrderLineQuantity = l.OrderLine.Quantity,
                Quantity = l.Quantity,
                PozNo = l.OrderLine.PozNo,
                Notes = l.Notes
            }).ToList()
        };
    }

    public async Task<IEnumerable<DeliveryDto>> GetByOrderIdAsync(int orderId)
    {
        var all = await GetAllAsync();
        return all.Where(d => d.OrderId == orderId);
    }

    public async Task<IEnumerable<OpenOrderDto>> GetOpenOrdersAsync()
    {
        // Açık sipariş: Onaylandi veya Uretimde durumda (taslak/iptal/tamamlandı hariç)
        var orders = await _orderRepository.Query()
            .Include(o => o.Customer)
            .Include(o => o.Lines.Where(l => !l.IsDeleted))
            .Where(o => o.Status == OrderStatus.Onaylandi || o.Status == OrderStatus.Uretimde)
            .ToListAsync();

        var result = new List<OpenOrderDto>();
        foreach (var o in orders)
        {
            var lineIds = o.Lines.Where(l => !l.IsDeleted).Select(l => l.Id).ToList();
            var deliveredByLine = await _lineRepository.Query()
                .Where(dl => !dl.IsDeleted && lineIds.Contains(dl.OrderLineId)
                             && dl.Delivery.Status == DeliveryStatus.Onaylandi
                             && !dl.Delivery.IsDeleted)
                .GroupBy(dl => dl.OrderLineId)
                .Select(g => new { OrderLineId = g.Key, Total = g.Sum(x => x.Quantity) })
                .ToDictionaryAsync(x => x.OrderLineId, x => x.Total);

            int totalRemaining = 0;
            foreach (var line in o.Lines.Where(l => !l.IsDeleted))
            {
                var delivered = deliveredByLine.TryGetValue(line.Id, out var d) ? d : 0;
                totalRemaining += Math.Max(0, line.Quantity - delivered);
            }

            if (totalRemaining > 0)
            {
                result.Add(new OpenOrderDto
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerId = o.CustomerId,
                    CustomerTitle = o.Customer.Title,
                    OrderDate = o.OrderDate,
                    DeliveryDate = o.DeliveryDate,
                    Status = o.Status,
                    TotalRemainingQuantity = totalRemaining
                });
            }
        }

        return result.OrderByDescending(x => x.OrderDate).ToList();
    }

    public async Task<IEnumerable<OpenOrderLineDto>> GetOpenLinesForOrderAsync(int orderId, int? excludeDeliveryId = null)
    {
        var order = await _orderRepository.Query()
            .Include(o => o.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines.Where(l => !l.IsDeleted)).ThenInclude(l => l.Recipe)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return Enumerable.Empty<OpenOrderLineDto>();

        var lineIds = order.Lines.Where(l => !l.IsDeleted).Select(l => l.Id).ToList();

        // Onaylanmış teslimatlardaki teslim edilmiş miktarlar (excludeDelivery hariç)
        var deliveredQuery = _lineRepository.Query()
            .Where(dl => !dl.IsDeleted && lineIds.Contains(dl.OrderLineId)
                         && dl.Delivery.Status == DeliveryStatus.Onaylandi
                         && !dl.Delivery.IsDeleted);

        if (excludeDeliveryId.HasValue)
            deliveredQuery = deliveredQuery.Where(dl => dl.DeliveryId != excludeDeliveryId.Value);

        var deliveredByLine = await deliveredQuery
            .GroupBy(dl => dl.OrderLineId)
            .Select(g => new { OrderLineId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.OrderLineId, x => x.Total);

        var result = new List<OpenOrderLineDto>();
        foreach (var line in order.Lines.Where(l => !l.IsDeleted))
        {
            var delivered = deliveredByLine.TryGetValue(line.Id, out var d) ? d : 0;
            var remaining = line.Quantity - delivered;
            if (remaining <= 0) continue;

            result.Add(new OpenOrderLineDto
            {
                OrderLineId = line.Id,
                ProductName = line.ProductItem != null ? line.ProductItem.Name :
                              (line.Recipe != null ? line.Recipe.Code : "-"),
                RecipeCode = line.Recipe?.Code,
                WidthMm = line.WidthMm,
                HeightMm = line.HeightMm,
                OrderQuantity = line.Quantity,
                DeliveredQuantity = delivered,
                RemainingQuantity = remaining,
                PozNo = line.PozNo
            });
        }

        return result;
    }

    public async Task<DeliveryDto> CreateAsync(DeliveryCreateDto dto)
    {
        if (dto.Lines == null || !dto.Lines.Any() || !dto.Lines.Any(l => l.Quantity > 0))
            throw new InvalidOperationException("En az bir kalem ve miktar girilmelidir.");

        // Açık miktar kontrolü
        await ValidateLineQuantitiesAsync(dto.OrderId, dto.Lines, excludeDeliveryId: null);

        var deliveryNumber = await GenerateDeliveryNumberAsync();
        var entity = new Delivery
        {
            DeliveryNumber = deliveryNumber,
            OrderId = dto.OrderId,
            DeliveryDate = dto.DeliveryDate,
            Status = DeliveryStatus.Taslak,
            Notes = dto.Notes
        };

        foreach (var lineDto in dto.Lines.Where(l => l.Quantity > 0))
        {
            entity.Lines.Add(new DeliveryLine
            {
                OrderLineId = lineDto.OrderLineId,
                Quantity = lineDto.Quantity,
                Notes = lineDto.Notes
            });
        }

        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(DeliveryUpdateDto dto)
    {
        var entity = await _repository.Query()
            .Include(d => d.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == dto.Id)
            ?? throw new KeyNotFoundException($"Teslimat bulunamadı: {dto.Id}");

        // Açık miktar kontrolü (kendi onaylı satırlarını hariç tut)
        await ValidateLineQuantitiesAsync(entity.OrderId, dto.Lines,
            excludeDeliveryId: entity.Status == DeliveryStatus.Onaylandi ? entity.Id : (int?)null);

        entity.DeliveryDate = dto.DeliveryDate;
        entity.Notes = dto.Notes;

        // Mevcut satırları soft-delete
        var now = TurkeyTime.Now;
        foreach (var line in entity.Lines.Where(l => !l.IsDeleted).ToList())
        {
            line.IsDeleted = true;
            line.DeletedAt = now;
        }

        // Yeni satırları ekle
        foreach (var lineDto in dto.Lines.Where(l => l.Quantity > 0))
        {
            entity.Lines.Add(new DeliveryLine
            {
                OrderLineId = lineDto.OrderLineId,
                Quantity = lineDto.Quantity,
                Notes = lineDto.Notes
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Teslimat bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task ApproveAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(d => d.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Teslimat bulunamadı: {id}");

        if (entity.Status == DeliveryStatus.Onaylandi)
            throw new InvalidOperationException("Teslimat zaten onaylanmış.");

        // Onaylama anında miktar kontrolü
        var lineDtos = entity.Lines.Where(l => !l.IsDeleted)
            .Select(l => new DeliveryLineCreateDto { OrderLineId = l.OrderLineId, Quantity = l.Quantity, Notes = l.Notes })
            .ToList();
        await ValidateLineQuantitiesAsync(entity.OrderId, lineDtos, excludeDeliveryId: null);

        entity.Status = DeliveryStatus.Onaylandi;
        entity.ApprovedAt = TurkeyTime.Now;
        await _repository.UpdateAsync(entity);
    }

    public async Task UnapproveAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Teslimat bulunamadı: {id}");

        if (entity.Status != DeliveryStatus.Onaylandi)
            throw new InvalidOperationException("Sadece onaylı teslimatlar geri alınabilir.");

        entity.Status = DeliveryStatus.Taslak;
        entity.ApprovedAt = null;
        await _repository.UpdateAsync(entity);
    }

    public async Task<DeliveryDto?> CreateFullDeliveryForOrderAsync(int orderId)
    {
        var openLines = (await GetOpenLinesForOrderAsync(orderId)).ToList();
        if (!openLines.Any()) return null;

        var deliveryNumber = await GenerateDeliveryNumberAsync();
        var entity = new Delivery
        {
            DeliveryNumber = deliveryNumber,
            OrderId = orderId,
            DeliveryDate = TurkeyTime.Now,
            Status = DeliveryStatus.Onaylandi,
            ApprovedAt = TurkeyTime.Now,
            Notes = "Otomatik oluşturulan tam teslimat"
        };

        foreach (var line in openLines)
        {
            entity.Lines.Add(new DeliveryLine
            {
                OrderLineId = line.OrderLineId,
                Quantity = line.RemainingQuantity
            });
        }

        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task<int> GetDeliveredQuantityAsync(int orderLineId)
    {
        return await _lineRepository.Query()
            .Where(dl => !dl.IsDeleted && dl.OrderLineId == orderLineId
                         && dl.Delivery.Status == DeliveryStatus.Onaylandi
                         && !dl.Delivery.IsDeleted)
            .SumAsync(dl => (int?)dl.Quantity) ?? 0;
    }

    public async Task<bool> IsOrderLineUsedInApprovedDeliveryAsync(int orderLineId)
    {
        return await _lineRepository.Query()
            .AnyAsync(dl => !dl.IsDeleted && dl.OrderLineId == orderLineId
                            && dl.Delivery.Status == DeliveryStatus.Onaylandi
                            && !dl.Delivery.IsDeleted);
    }

    public async Task<bool> HasApprovedDeliveryAsync(int orderId)
    {
        return await _repository.Query()
            .AnyAsync(d => !d.IsDeleted && d.OrderId == orderId
                           && d.Status == DeliveryStatus.Onaylandi);
    }

    public async Task<int> ReconcileWithOrderAsync(int orderId)
    {
        // Siparişin güncel kalemlerini al (id -> quantity)
        var order = await _orderRepository.Query()
            .Include(o => o.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return 0;

        var orderLineQty = order.Lines
            .Where(l => !l.IsDeleted)
            .ToDictionary(l => l.Id, l => l.Quantity);

        // Bu sipariş için tüm aktif teslimatları çek
        var deliveries = await _repository.Query()
            .Include(d => d.Lines.Where(l => !l.IsDeleted))
            .Where(d => d.OrderId == orderId && !d.IsDeleted)
            .ToListAsync();

        int changes = 0;
        foreach (var d in deliveries)
        {
            foreach (var dl in d.Lines.Where(l => !l.IsDeleted).ToList())
            {
                if (!orderLineQty.TryGetValue(dl.OrderLineId, out var newOrderQty))
                {
                    // Sipariş kalemi silinmiş — teslimat satırını da sil
                    await _lineRepository.DeleteAsync(dl);
                    changes++;
                }
                else if (dl.Quantity > newOrderQty)
                {
                    // Teslimat miktarı yeni sipariş miktarını aşıyor — düşür
                    dl.Quantity = newOrderQty;
                    changes++;
                }
            }
        }

        if (changes > 0)
            await _unitOfWork.SaveChangesAsync();

        return changes;
    }

    private async Task ValidateLineQuantitiesAsync(int orderId, List<DeliveryLineCreateDto> lines, int? excludeDeliveryId)
    {
        var openLines = (await GetOpenLinesForOrderAsync(orderId, excludeDeliveryId)).ToDictionary(x => x.OrderLineId);

        foreach (var line in lines.Where(l => l.Quantity > 0))
        {
            if (!openLines.TryGetValue(line.OrderLineId, out var open))
                throw new InvalidOperationException($"OrderLine {line.OrderLineId} için teslim edilebilecek açık miktar yok.");

            if (line.Quantity > open.RemainingQuantity)
                throw new InvalidOperationException(
                    $"'{open.ProductName}' kalemi için max {open.RemainingQuantity} adet teslim edilebilir, {line.Quantity} girildi.");
        }
    }

    private async Task<string> GenerateDeliveryNumberAsync()
    {
        // TES-yyyy-XXX (yıl bazlı, ay sıfırlamasız)
        var year = TurkeyTime.Now.Year;
        var prefix = $"TES-{year}-";

        var thisYear = await _repository.Query()
            .Where(d => d.DeliveryNumber.StartsWith(prefix))
            .Select(d => d.DeliveryNumber)
            .ToListAsync();

        int maxSeq = 0;
        foreach (var num in thisYear)
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
