using GlassSoft.Domain.Common;
using GlassSoft.Application.DTOs.Production;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Production;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Production;

public class WorkOrderService : IWorkOrderService
{
    private readonly IRepository<WorkOrder> _repository;
    private readonly IRepository<WorkOrderLine> _lineRepository;
    private readonly IRepository<CuttingPlan> _planRepository;
    private readonly IRepository<CuttingPlanItem> _planItemRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderLine> _orderLineRepository;
    private readonly IRepository<WorkOrderOrder> _workOrderOrderRepository;
    private readonly IRepository<Domain.Entities.Recipe.Recipe> _recipeRepository;
    private readonly IRepository<Domain.Entities.Product.GlassPlateDefinition> _plateRepository;
    private readonly IRepository<Domain.Entities.Product.StockEntry> _stockRepository;

    public WorkOrderService(
        IRepository<WorkOrder> repository,
        IRepository<WorkOrderLine> lineRepository,
        IRepository<CuttingPlan> planRepository,
        IRepository<CuttingPlanItem> planItemRepository,
        IRepository<Order> orderRepository,
        IRepository<OrderLine> orderLineRepository,
        IRepository<WorkOrderOrder> workOrderOrderRepository,
        IRepository<Domain.Entities.Recipe.Recipe> recipeRepository,
        IRepository<Domain.Entities.Product.GlassPlateDefinition> plateRepository,
        IRepository<Domain.Entities.Product.StockEntry> stockRepository)
    {
        _repository = repository;
        _lineRepository = lineRepository;
        _planRepository = planRepository;
        _planItemRepository = planItemRepository;
        _orderRepository = orderRepository;
        _orderLineRepository = orderLineRepository;
        _workOrderOrderRepository = workOrderOrderRepository;
        _recipeRepository = recipeRepository;
        _plateRepository = plateRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<WorkOrderDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(w => w.Order!).ThenInclude(o => o.Customer)
            .Include(w => w.Orders).ThenInclude(wo => wo.Order).ThenInclude(o => o.Customer)
            .Include(w => w.Lines)
            .Include(w => w.CuttingPlans)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                WorkOrderNumber = w.WorkOrderNumber,
                OrderId = w.OrderId ?? (w.Orders.Any() ? w.Orders.First().OrderId : 0),
                OrderNumber = w.Order != null
                    ? w.Order.OrderNumber
                    : (w.Orders.Any() ? w.Orders.First().Order.OrderNumber : "-"),
                CustomerTitle = w.Order != null
                    ? w.Order.Customer.Title
                    : (w.Orders.Any() ? w.Orders.First().Order.Customer.Title : "-"),
                PlannedDate = w.PlannedDate,
                IsCompleted = w.IsCompleted,
                CompletedAt = w.CompletedAt,
                Notes = w.Notes,
                LineCount = w.Lines.Count,
                CuttingPlanCount = w.CuttingPlans.Count,
                RelatedOrders = w.Orders.Select(wo => new WorkOrderOrderDto
                {
                    OrderId = wo.OrderId,
                    OrderNumber = wo.Order.OrderNumber,
                    CustomerTitle = wo.Order.Customer.Title
                }).ToList()
            })
            .OrderByDescending(w => w.PlannedDate)
            .ToListAsync();
    }

    public async Task<WorkOrderDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(w => w.Id == id)
            .Include(w => w.Order!).ThenInclude(o => o.Customer)
            .Include(w => w.Orders).ThenInclude(wo => wo.Order).ThenInclude(o => o.Customer)
            .Include(w => w.Lines).ThenInclude(l => l.ProductItem)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.ProductItem)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.GlassPlateDefinition)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.Items)
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                WorkOrderNumber = w.WorkOrderNumber,
                OrderId = w.OrderId ?? (w.Orders.Any() ? w.Orders.First().OrderId : 0),
                OrderNumber = w.Order != null
                    ? w.Order.OrderNumber
                    : (w.Orders.Any() ? w.Orders.First().Order.OrderNumber : "-"),
                CustomerTitle = w.Order != null
                    ? w.Order.Customer.Title
                    : (w.Orders.Any() ? w.Orders.First().Order.Customer.Title : "-"),
                PlannedDate = w.PlannedDate,
                IsCompleted = w.IsCompleted,
                CompletedAt = w.CompletedAt,
                Notes = w.Notes,
                LineCount = w.Lines.Count,
                CuttingPlanCount = w.CuttingPlans.Count,
                RelatedOrders = w.Orders.Select(wo => new WorkOrderOrderDto
                {
                    OrderId = wo.OrderId,
                    OrderNumber = wo.Order.OrderNumber,
                    CustomerTitle = wo.Order.Customer.Title
                }).ToList(),
                Lines = w.Lines.Select(l => new WorkOrderLineDto
                {
                    Id = l.Id,
                    OrderLineId = l.OrderLineId,
                    ProductItemId = l.ProductItemId,
                    ProductItemName = l.ProductItem.Name,
                    WidthMm = l.WidthMm,
                    HeightMm = l.HeightMm,
                    Quantity = l.Quantity,
                    MaterialType = l.MaterialType,
                    IsOptimized = l.IsOptimized
                }).ToList(),
                CuttingPlans = w.CuttingPlans.Select(p => new CuttingPlanDto
                {
                    Id = p.Id,
                    WorkOrderId = p.WorkOrderId,
                    ProductItemId = p.ProductItemId,
                    ProductItemName = p.ProductItem.Name,
                    GlassPlateDefinitionId = p.GlassPlateDefinitionId,
                    PlateDisplayName = p.GlassPlateDefinition.WidthMm + "x" + p.GlassPlateDefinition.HeightMm,
                    PlateWidthMm = p.GlassPlateDefinition.WidthMm,
                    PlateHeightMm = p.GlassPlateDefinition.HeightMm,
                    PlateIndex = p.PlateIndex,
                    WastePercentage = p.WastePercentage,
                    UsedAreaM2 = p.UsedAreaM2,
                    WasteAreaM2 = p.WasteAreaM2,
                    CsvOutput = p.CsvOutput,
                    DxfOutput = p.DxfOutput,
                    IsApproved = p.IsApproved,
                    Items = p.Items.Select(i => new CuttingPlanItemDto
                    {
                        Id = i.Id,
                        OrderLineId = i.OrderLineId,
                        X = i.X,
                        Y = i.Y,
                        WidthMm = i.WidthMm,
                        HeightMm = i.HeightMm,
                        IsRotated = i.IsRotated
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Onaylı siparişten iş emri oluştur + reçete patlatma
    /// </summary>
    public async Task<WorkOrderDto> CreateFromOrderAsync(WorkOrderCreateDto dto)
    {
        var order = await _orderRepository.Query()
            .Include(o => o.Lines).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines).ThenInclude(l => l.Recipe).ThenInclude(r => r!.Layers).ThenInclude(l => l.ProductItem)
            .Include(o => o.Lines).ThenInclude(l => l.Recipe).ThenInclude(r => r!.Consumables).ThenInclude(c => c.ProductItem)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {dto.OrderId}");

        var workOrderNumber = await GenerateWorkOrderNumberAsync();
        var workOrder = new WorkOrder
        {
            WorkOrderNumber = workOrderNumber,
            OrderId = dto.OrderId,
            PlannedDate = TurkeyTime.WithCurrentTime(dto.PlannedDate),
            Notes = dto.Notes
        };
        await _repository.AddAsync(workOrder);

        // Siparişi üretime al
        order.Status = OrderStatus.Uretimde;
        await _orderRepository.UpdateAsync(order);

        // Reçete patlatma: sipariş kalemlerinden malzeme listesi oluştur
        await ExplodeRecipeForLines(workOrder.Id, order.Lines);

        return (await GetByIdAsync(workOrder.Id))!;
    }

    /// <summary>
    /// Birden fazla siparişten seçilen kalemlerle iş emri oluştur
    /// </summary>
    public async Task<WorkOrderDto> CreateFromLinesAsync(WorkOrderCreateFromLinesDto dto)
    {
        if (dto.SelectedOrderLineIds == null || dto.SelectedOrderLineIds.Count == 0)
            throw new ArgumentException("En az bir sipariş kalemi seçilmelidir.");

        // Seçilen sipariş kalemlerini yükle
        var selectedLines = await _orderLineRepository.Query()
            .Where(ol => dto.SelectedOrderLineIds.Contains(ol.Id))
            .Include(ol => ol.Order).ThenInclude(o => o.Customer)
            .Include(ol => ol.ProductItem)
            .Include(ol => ol.Recipe).ThenInclude(r => r!.Layers).ThenInclude(l => l.ProductItem)
            .Include(ol => ol.Recipe).ThenInclude(r => r!.Consumables).ThenInclude(c => c.ProductItem)
            .ToListAsync();

        if (selectedLines.Count == 0)
            throw new KeyNotFoundException("Seçilen sipariş kalemleri bulunamadı.");

        var workOrderNumber = await GenerateWorkOrderNumberAsync();
        var workOrder = new WorkOrder
        {
            WorkOrderNumber = workOrderNumber,
            OrderId = null, // Multi-order
            PlannedDate = TurkeyTime.WithCurrentTime(dto.PlannedDate),
            Notes = dto.Notes
        };
        await _repository.AddAsync(workOrder);

        // İlgili siparişler için WorkOrderOrder kayıtları oluştur
        var distinctOrderIds = selectedLines.Select(l => l.OrderId).Distinct().ToList();
        foreach (var orderId in distinctOrderIds)
        {
            var woOrder = new WorkOrderOrder
            {
                WorkOrderId = workOrder.Id,
                OrderId = orderId
            };
            await _workOrderOrderRepository.AddAsync(woOrder);
        }

        // Reçete patlatma: seçilen kalemlerden malzeme listesi oluştur
        await ExplodeRecipeForLines(workOrder.Id, selectedLines);

        return (await GetByIdAsync(workOrder.Id))!;
    }

    /// <summary>
    /// Henüz optimize edilmemiş (iş emrine eklenmemiş) sipariş kalemlerini getir
    /// </summary>
    public async Task<List<UnoptimizedOrderLineDto>> GetUnoptimizedOrderLinesAsync()
    {
        // Halihazırda bir WorkOrderLine'da referans edilen OrderLine Id'lerini bul
        var optimizedLineIds = await _lineRepository.Query()
            .Where(wol => wol.OrderLineId != null)
            .Select(wol => wol.OrderLineId!.Value)
            .Distinct()
            .ToListAsync();

        // Onaylı siparişlerdeki, henüz iş emrine eklenmemiş kalemleri getir
        return await _orderLineRepository.Query()
            .Where(ol => ol.Order.Status == OrderStatus.Onaylandi)
            .Where(ol => !optimizedLineIds.Contains(ol.Id))
            .Include(ol => ol.Order).ThenInclude(o => o.Customer)
            .Include(ol => ol.ProductItem)
            .Include(ol => ol.Recipe)
            .Select(ol => new UnoptimizedOrderLineDto
            {
                OrderLineId = ol.Id,
                OrderId = ol.OrderId,
                OrderNumber = ol.Order.OrderNumber,
                CustomerTitle = ol.Order.Customer.Title,
                ProductItemId = ol.ProductItemId,
                ProductItemName = ol.ProductItem != null ? ol.ProductItem.Name : null,
                RecipeId = ol.RecipeId,
                RecipeCode = ol.Recipe != null ? ol.Recipe.Code : null,
                WidthMm = ol.WidthMm ?? 0,
                HeightMm = ol.HeightMm ?? 0,
                Quantity = ol.Quantity
            })
            .OrderBy(ol => ol.OrderNumber)
            .ThenBy(ol => ol.OrderLineId)
            .ToListAsync();
    }

    /// <summary>
    /// İş emrindeki cam parçaları için kesim optimizasyonu çalıştır
    /// </summary>
    public async Task<List<CuttingPlanDto>> RunCuttingOptimizationAsync(int workOrderId)
    {
        var workOrder = await _repository.Query()
            .Include(w => w.Lines).ThenInclude(l => l.ProductItem)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(w => w.Id == workOrderId)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı: {workOrderId}");

        if (workOrder.IsCompleted)
            throw new InvalidOperationException("Tamamlanmış iş emrinde optimizasyon yeniden çalıştırılamaz.");

        // Eski planlar ancak yeni planların tamamı başarıyla hesaplandıktan sonra
        // silinecek. Böylece sığmayan parça gibi bir hata mevcut planı bozmaz.
        var oldPlans = workOrder.CuttingPlans.ToList();

        // Cam malzemeleri grupla (ürün bazında)
        var glassLines = workOrder.Lines
            .Where(l => l.MaterialType == "Glass" && l.WidthMm > 0 && l.HeightMm > 0 && l.Quantity > 0)
            .GroupBy(l => l.ProductItemId)
            .ToList();

        var allPlates = await _plateRepository.Query()
            .Where(p => p.WidthMm > 0 && p.HeightMm > 0)
            .OrderByDescending(p => p.IsDefault)
            .ThenBy(p => p.WidthMm * p.HeightMm)
            .ToListAsync();

        if (glassLines.Count > 0 && allPlates.Count == 0)
            throw new InvalidOperationException("Kesim optimizasyonu için en az bir geçerli plaka tanımı gereklidir.");

        // Tüm planları ve item'ları bellekte oluştur, sonra TEK SEFERDE kaydet
        var allNewPlans = new List<CuttingPlan>();

        foreach (var group in glassLines)
        {
            var productItemId = group.Key;

            // Sipariş kalemi kimliği kaybolmamalı: aynı ölçüdeki farklı siparişler
            // ayrı parça örnekleri olarak optimizasyona girer.
            var pieces = group
                .SelectMany(line => Enumerable.Range(0, line.Quantity).Select(_ => new CutPiece
                {
                    Width = (double)line.WidthMm,
                    Height = (double)line.HeightMm,
                    Quantity = 1,
                    CanRotate = true,
                    OrderLineId = line.OrderLineId
                })).ToList();

            int plateIndex = 1;
            var remaining = pieces;

            while (remaining.Count > 0)
            {
                // Her turda bütün plaka ölçülerini dener; en çok parçayı yerleştiren,
                // eşitlikte daha düşük fire üreten plaka seçilir.
                var candidates = allPlates.Select(plate =>
                {
                    var cutter = new GuillotineCutter((double)plate.WidthMm, (double)plate.HeightMm);
                    var notPlaced = cutter.Pack(remaining);
                    return new { Plate = plate, Cutter = cutter, Remaining = notPlaced };
                }).Where(x => x.Cutter.PlacedPieces.Count > 0).ToList();

                var best = candidates
                    .OrderByDescending(x => x.Cutter.PlacedPieces.Count)
                    .ThenBy(x => x.Cutter.GetWastePercentage())
                    .ThenBy(x => x.Cutter.GetPlateArea())
                    .FirstOrDefault();

                if (best == null)
                {
                    var sample = remaining[0];
                    throw new InvalidOperationException(
                        $"{sample.Width:0.##}x{sample.Height:0.##} mm parça tanımlı plakalardan hiçbirine sığmıyor.");
                }

                var plate = best.Plate;
                var cutter = best.Cutter;
                remaining = best.Remaining;

                var usedArea = (decimal)(cutter.GetUsedArea() / 1_000_000.0);
                var plateArea = (decimal)(cutter.GetPlateArea() / 1_000_000.0);

                var cuttingPlan = new CuttingPlan
                {
                    WorkOrderId = workOrderId,
                    ProductItemId = productItemId,
                    GlassPlateDefinitionId = plate.Id,
                    PlateIndex = plateIndex,
                    WastePercentage = (decimal)cutter.GetWastePercentage(),
                    UsedAreaM2 = usedArea,
                    WasteAreaM2 = plateArea - usedArea,
                    CsvOutput = cutter.GenerateCsv(),
                    DxfOutput = cutter.GenerateDxf(),
                    Items = cutter.PlacedPieces.Select(placed => new CuttingPlanItem
                    {
                        OrderLineId = placed.OrderLineId,
                        X = (decimal)placed.X,
                        Y = (decimal)placed.Y,
                        WidthMm = (decimal)placed.Width,
                        HeightMm = (decimal)placed.Height,
                        IsRotated = placed.IsRotated
                    }).ToList()
                };
                allNewPlans.Add(cuttingPlan);
                plateIndex++;
            }

            foreach (var line in group)
                line.IsOptimized = true;
        }

        var now = TurkeyTime.Now;
        foreach (var plan in oldPlans)
        {
            foreach (var item in plan.Items)
            {
                item.IsDeleted = true;
                item.DeletedAt = now;
            }
            plan.IsDeleted = true;
            plan.DeletedAt = now;
        }

        var orderLineIds = glassLines.SelectMany(g => g)
            .Where(l => l.OrderLineId.HasValue)
            .Select(l => l.OrderLineId!.Value)
            .Distinct()
            .ToList();
        var orderLines = await _orderLineRepository.Query()
            .Where(ol => orderLineIds.Contains(ol.Id))
            .ToListAsync();
        foreach (var orderLine in orderLines)
            orderLine.IsOptimized = true;

        // TEK SaveChanges: eski planların soft-delete'i, yeni planlar ve satır durumları atomik kaydedilir.
        await _planRepository.AddRangeAsync(allNewPlans);

        var refreshed = await GetByIdAsync(workOrderId);
        return refreshed?.CuttingPlans ?? new List<CuttingPlanDto>();
    }

    public async Task CompleteAsync(int id)
    {
        var workOrder = await _repository.Query()
            .Include(w => w.Lines)
            .Include(w => w.CuttingPlans)
            .Include(w => w.Order!)
            .Include(w => w.Orders).ThenInclude(wo => wo.Order)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı: {id}");

        if (workOrder.IsCompleted)
            throw new InvalidOperationException("İş emri zaten tamamlanmış.");
        if (workOrder.Lines.Any(l => l.MaterialType == "Glass" && l.Quantity > 0) && !workOrder.CuttingPlans.Any())
            throw new InvalidOperationException("Cam içeren iş emri, kesim optimizasyonu yapılmadan tamamlanamaz.");

        workOrder.IsCompleted = true;
        workOrder.CompletedAt = TurkeyTime.Now;

        // Siparişleri tamamla
        if (workOrder.Order != null)
            workOrder.Order.Status = OrderStatus.Tamamlandi;
        foreach (var woOrder in workOrder.Orders)
            woOrder.Order.Status = OrderStatus.Tamamlandi;

        // Stoktan düş: kullanılan plakalar
        var stockEntries = workOrder.CuttingPlans.Select(plan => new Domain.Entities.Product.StockEntry
        {
            ProductItemId = plan.ProductItemId,
            MovementType = StockMovementType.Cikis,
            Quantity = plan.UsedAreaM2 + plan.WasteAreaM2, // Toplam plaka alanı
            PlateCount = 1,
            GlassPlateDefinitionId = plan.GlassPlateDefinitionId,
            ReferenceType = "WorkOrder",
            ReferenceId = workOrder.Id,
            Description = $"İş Emri {workOrder.WorkOrderNumber} - Plaka #{plan.PlateIndex}"
        }).ToList();

        // Takip edilen iş emri/sipariş değişiklikleri ve stok hareketleri tek SaveChanges ile yazılır.
        await _stockRepository.AddRangeAsync(stockEntries);
    }

    public async Task DeleteAsync(int id)
    {
        var workOrder = await _repository.Query()
            .Include(w => w.Lines)
            .Include(w => w.Orders)
            .Include(w => w.CuttingPlans).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException($"İş emri bulunamadı: {id}");

        // Kesim planlarını sil
        foreach (var plan in workOrder.CuttingPlans.ToList())
        {
            foreach (var item in plan.Items.ToList())
                await _planItemRepository.DeleteAsync(item);
            await _planRepository.DeleteAsync(plan);
        }

        // WorkOrderOrder junction kayıtlarını sil
        foreach (var woOrder in workOrder.Orders.ToList())
            await _workOrderOrderRepository.DeleteAsync(woOrder);

        // İş emri satırlarının referans ettiği OrderLine'ların IsOptimized'ını sıfırla
        var orderLineIds = workOrder.Lines
            .Where(l => l.OrderLineId != null)
            .Select(l => l.OrderLineId!.Value)
            .Distinct()
            .ToList();

        if (orderLineIds.Count > 0)
        {
            var orderLines = await _orderLineRepository.Query()
                .Where(ol => orderLineIds.Contains(ol.Id))
                .ToListAsync();
            foreach (var ol in orderLines)
            {
                ol.IsOptimized = false;
                await _orderLineRepository.UpdateAsync(ol);
            }
        }

        // İş emri satırlarını sil
        foreach (var line in workOrder.Lines.ToList())
            await _lineRepository.DeleteAsync(line);

        await _repository.DeleteAsync(workOrder);
    }

    public async Task<string> GenerateWorkOrderNumberAsync()
    {
        var prefix = $"IE-{TurkeyTime.Now:yyyyMM}-";
        var last = await _repository.Query()
            .Where(w => w.WorkOrderNumber.StartsWith(prefix))
            .OrderByDescending(w => w.WorkOrderNumber)
            .FirstOrDefaultAsync();

        if (last == null) return prefix + "001";
        var lastNum = int.Parse(last.WorkOrderNumber[prefix.Length..]);
        return prefix + (lastNum + 1).ToString("D3");
    }

    /// <summary>
    /// Sipariş kalemlerinden reçete patlatma yaparak iş emri satırları oluştur
    /// </summary>
    private async Task ExplodeRecipeForLines(int workOrderId, IEnumerable<OrderLine> lines)
    {
        foreach (var line in lines)
        {
            if (line.Recipe != null)
            {
                // Reçeteli ürün: katmanları ayrı malzeme olarak ekle
                foreach (var layer in line.Recipe.Layers)
                {
                    var woLine = new WorkOrderLine
                    {
                        WorkOrderId = workOrderId,
                        OrderLineId = line.Id,
                        ProductItemId = layer.ProductItemId,
                        WidthMm = line.WidthMm ?? 0,
                        HeightMm = line.HeightMm ?? 0,
                        Quantity = line.Quantity * layer.QuantityPerUnit,
                        MaterialType = layer.LayerType
                    };
                    await _lineRepository.AddAsync(woLine);
                }

                // Sarf malzemeleri (kesim optimizasyonuna dahil edilmez, sadece listede gösterilir)
                foreach (var cons in line.Recipe.Consumables)
                {
                    var woLine = new WorkOrderLine
                    {
                        WorkOrderId = workOrderId,
                        OrderLineId = line.Id,
                        ProductItemId = cons.ProductItemId,
                        WidthMm = 0,
                        HeightMm = 0,
                        Quantity = line.Quantity,
                        MaterialType = "Consumable"
                    };
                    await _lineRepository.AddAsync(woLine);
                }
            }
            else if (line.ProductItem != null)
            {
                // Tekli ürün
                var materialType = line.ProductItem.IsPlate ? "Glass" : "Other";
                var woLine = new WorkOrderLine
                {
                    WorkOrderId = workOrderId,
                    OrderLineId = line.Id,
                    ProductItemId = line.ProductItemId!.Value,
                    WidthMm = line.WidthMm ?? 0,
                    HeightMm = line.HeightMm ?? 0,
                    Quantity = line.Quantity,
                    MaterialType = materialType
                };
                await _lineRepository.AddAsync(woLine);
            }
        }
    }
}
