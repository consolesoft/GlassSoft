using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Enums;
using RecipeEntity = GlassSoft.Domain.Entities.Recipe.Recipe;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Product;

public class StockService : IStockService
{
    private readonly IRepository<StockEntry> _repository;
    private readonly IRepository<ProductItem> _productRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<RecipeEntity> _recipeRepository;

    public StockService(
        IRepository<StockEntry> repository,
        IRepository<ProductItem> productRepository,
        IRepository<Order> orderRepository,
        IRepository<RecipeEntity> recipeRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _recipeRepository = recipeRepository;
    }

    public async Task<IEnumerable<StockEntryDto>> GetEntriesByProductAsync(int productItemId)
    {
        return await _repository.Query()
            .Where(s => s.ProductItemId == productItemId)
            .Include(s => s.ProductItem)
            .Include(s => s.GlassPlateDefinition)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StockEntryDto
            {
                Id = s.Id,
                ProductItemId = s.ProductItemId,
                ProductItemName = s.ProductItem.Name,
                MovementType = s.MovementType,
                Quantity = s.Quantity,
                PlateCount = s.PlateCount,
                GlassPlateDefinitionId = s.GlassPlateDefinitionId,
                PlateDefinitionDisplay = s.GlassPlateDefinition != null
                    ? s.GlassPlateDefinition.WidthMm + "x" + s.GlassPlateDefinition.HeightMm + " mm"
                    : null,
                ReferenceType = s.ReferenceType,
                ReferenceId = s.ReferenceId,
                Description = s.Description,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<StockEntryDto>> GetRecentEntriesAsync(int count = 50)
    {
        return await _repository.Query()
            .Include(s => s.ProductItem)
            .Include(s => s.GlassPlateDefinition)
            .OrderByDescending(s => s.CreatedAt)
            .Take(count)
            .Select(s => new StockEntryDto
            {
                Id = s.Id,
                ProductItemId = s.ProductItemId,
                ProductItemName = s.ProductItem.Name,
                MovementType = s.MovementType,
                Quantity = s.Quantity,
                PlateCount = s.PlateCount,
                GlassPlateDefinitionId = s.GlassPlateDefinitionId,
                PlateDefinitionDisplay = s.GlassPlateDefinition != null
                    ? s.GlassPlateDefinition.WidthMm + "x" + s.GlassPlateDefinition.HeightMm + " mm"
                    : null,
                Description = s.Description,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<StockEntryDto> CreateEntryAsync(StockEntryCreateDto dto)
    {
        var entity = new StockEntry
        {
            ProductItemId = dto.ProductItemId,
            MovementType = dto.MovementType,
            Quantity = dto.Quantity,
            PlateCount = dto.PlateCount,
            GlassPlateDefinitionId = dto.GlassPlateDefinitionId,
            Description = dto.Description,
            ReferenceType = "Manual"
        };
        await _repository.AddAsync(entity);
        return new StockEntryDto
        {
            Id = entity.Id,
            ProductItemId = entity.ProductItemId,
            MovementType = entity.MovementType,
            Quantity = entity.Quantity,
            PlateCount = entity.PlateCount,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<StockEntryDto?> GetEntryByIdAsync(int id)
    {
        var s = await _repository.Query()
            .Include(e => e.ProductItem)
            .Include(e => e.GlassPlateDefinition)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (s == null) return null;
        return new StockEntryDto
        {
            Id = s.Id,
            ProductItemId = s.ProductItemId,
            ProductItemName = s.ProductItem.Name,
            MovementType = s.MovementType,
            Quantity = s.Quantity,
            PlateCount = s.PlateCount,
            GlassPlateDefinitionId = s.GlassPlateDefinitionId,
            PlateDefinitionDisplay = s.GlassPlateDefinition != null
                ? s.GlassPlateDefinition.WidthMm + "x" + s.GlassPlateDefinition.HeightMm + " mm"
                : null,
            ReferenceType = s.ReferenceType,
            ReferenceId = s.ReferenceId,
            Description = s.Description,
            CreatedAt = s.CreatedAt
        };
    }

    public async Task UpdateEntryAsync(int id, StockEntryCreateDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Stok hareketi bulunamadı: {id}");
        entity.ProductItemId = dto.ProductItemId;
        entity.MovementType = dto.MovementType;
        entity.Quantity = dto.Quantity;
        entity.PlateCount = dto.PlateCount;
        entity.GlassPlateDefinitionId = dto.GlassPlateDefinitionId;
        entity.Description = dto.Description;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteEntryAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Stok hareketi bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync()
    {
        var products = await _productRepository.Query()
            .Include(p => p.ProductGroup)
            .Include(p => p.StockEntries.Where(s => !s.IsDeleted))
            .Where(p => p.IsActive)
            .ToListAsync();

        return products.Select(p => new StockSummaryDto
        {
            ProductItemId = p.Id,
            ProductCode = p.Code,
            ProductName = p.Name,
            ProductGroupName = p.ProductGroup.Name,
            UnitDisplay = p.Unit switch
            {
                UnitType.MetreKare => "m²",
                UnitType.Metre => "m",
                UnitType.Kilogram => "kg",
                UnitType.Litre => "lt",
                _ => "Adet"
            },
            IsPlate = p.IsPlate,
            TotalQuantity = p.StockEntries
                .Sum(s => s.MovementType == StockMovementType.Giris ? s.Quantity : -s.Quantity),
            TotalPlateCount = p.StockEntries
                .Sum(s => s.MovementType == StockMovementType.Giris ? s.PlateCount : -s.PlateCount)
        })
        .OrderBy(s => s.ProductGroupName).ThenBy(s => s.ProductName)
        .ToList();
    }

    public async Task<StockSummaryDto?> GetProductStockAsync(int productItemId)
    {
        var summaries = await GetStockSummaryAsync();
        return summaries.FirstOrDefault(s => s.ProductItemId == productItemId);
    }

    public async Task DeductStockForOrderAsync(int orderId)
    {
        var order = await _orderRepository.Query()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {orderId}");

        foreach (var line in order.Lines.Where(l => !l.IsDeleted))
        {
            if (line.RecipeId.HasValue)
            {
                // Reçeteli ürün → reçetedeki tüm katmanların stokları düşer
                var recipe = await _recipeRepository.Query()
                    .Include(r => r.Layers).ThenInclude(l => l.ProductItem)
                    .FirstOrDefaultAsync(r => r.Id == line.RecipeId.Value);

                if (recipe != null)
                {
                    foreach (var layer in recipe.Layers.Where(l => !l.IsDeleted))
                    {
                        // Her katman için: parça m² × adet × katman adedi
                        var areaM2 = line.TotalAreaM2 * layer.QuantityPerUnit;
                        await CreateStockExitAsync(
                            layer.ProductItemId,
                            areaM2,
                            order.OrderNumber,
                            orderId,
                            $"{order.OrderNumber} siparişine istinaden çıkış ({layer.ProductItem.Name}, {line.WidthMm:N0}x{line.HeightMm:N0}mm, {line.Quantity} adet)");
                    }
                }
            }
            else if (line.ProductItemId.HasValue)
            {
                // Tekli ürün → toplam m² düşer
                await CreateStockExitAsync(
                    line.ProductItemId.Value,
                    line.TotalAreaM2,
                    order.OrderNumber,
                    orderId,
                    $"{order.OrderNumber} siparişine istinaden çıkış ({line.WidthMm:N0}x{line.HeightMm:N0}mm, {line.Quantity} adet)");
            }
        }
    }

    public async Task<int> ReverseStockForOrderAsync(int orderId)
    {
        var entries = await _repository.Query()
            .Where(e => e.ReferenceType == "Order"
                        && e.ReferenceId == orderId
                        && e.MovementType == StockMovementType.Cikis)
            .ToListAsync();

        int count = 0;
        foreach (var entry in entries)
        {
            await _repository.DeleteAsync(entry);
            count++;
        }
        return count;
    }

    public async Task<int> ResetOrderEntriesAsync()
    {
        var orderEntries = await _repository.Query()
            .Where(e => e.ReferenceType == "Order")
            .ToListAsync();
        int count = 0;
        foreach (var entry in orderEntries)
        {
            await _repository.DeleteAsync(entry);
            count++;
        }
        return count;
    }

    private async Task CreateStockExitAsync(int productItemId, decimal quantity, string orderNumber, int orderId, string description)
    {
        // Ürün var mı kontrol et, yoksa devam et (stok kartı otomatik oluşmaz, sadece hareket yazılır)
        var productExists = await _productRepository.Query().AnyAsync(p => p.Id == productItemId);
        if (!productExists) return;

        var entry = new StockEntry
        {
            ProductItemId = productItemId,
            MovementType = StockMovementType.Cikis,
            Quantity = quantity,
            PlateCount = 0,
            ReferenceType = "Order",
            ReferenceId = orderId,
            Description = description
        };
        await _repository.AddAsync(entry);
    }
}
