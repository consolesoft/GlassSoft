using GlassSoft.Application.DTOs.Recipe;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Recipe;

using GlassSoft.Domain.Entities.Recipe;

public class RecipeService : IRecipeService
{
    private readonly IRepository<Recipe> _repository;
    private readonly IRepository<RecipeLayer> _layerRepository;
    private readonly IRepository<RecipeConsumable> _consumableRepository;

    public RecipeService(
        IRepository<Recipe> repository,
        IRepository<RecipeLayer> layerRepository,
        IRepository<RecipeConsumable> consumableRepository)
    {
        _repository = repository;
        _layerRepository = layerRepository;
        _consumableRepository = consumableRepository;
    }

    public async Task<IEnumerable<RecipeDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(r => r.ProductItem)
            .Include(r => r.Layers)
            .Include(r => r.Consumables)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                BaseUnitPrice = r.BaseUnitPrice,
                IsActive = r.IsActive,
                ProductItemId = r.ProductItemId,
                ProductItemName = r.ProductItem != null ? r.ProductItem.Name : null,
                LayerCount = r.Layers.Count,
                ConsumableCount = r.Consumables.Count
            })
            .OrderBy(r => r.Code)
            .ToListAsync();
    }

    public async Task<RecipeDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(r => r.Id == id)
            .Include(r => r.ProductItem)
            .Include(r => r.Layers).ThenInclude(l => l.ProductItem)
            .Include(r => r.Consumables).ThenInclude(c => c.ProductItem)
            .Select(r => new RecipeDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                BaseUnitPrice = r.BaseUnitPrice,
                IsActive = r.IsActive,
                ProductItemId = r.ProductItemId,
                ProductItemName = r.ProductItem != null ? r.ProductItem.Name : null,
                LayerCount = r.Layers.Count,
                ConsumableCount = r.Consumables.Count,
                Layers = r.Layers.OrderBy(l => l.SortOrder).Select(l => new RecipeLayerDto
                {
                    Id = l.Id,
                    SortOrder = l.SortOrder,
                    LayerType = l.LayerType,
                    ProductItemId = l.ProductItemId,
                    ProductItemName = l.ProductItem.Name,
                    ThicknessMm = l.ThicknessMm,
                    QuantityPerUnit = l.QuantityPerUnit
                }).ToList(),
                Consumables = r.Consumables.Select(c => new RecipeConsumableDto
                {
                    Id = c.Id,
                    ProductItemId = c.ProductItemId,
                    ProductItemName = c.ProductItem.Name,
                    ConsumptionFormula = c.ConsumptionFormula,
                    Description = c.Description
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RecipeDto> CreateAsync(RecipeCreateDto dto)
    {
        var entity = new Recipe
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            BaseUnitPrice = dto.BaseUnitPrice,
            IsActive = dto.IsActive,
            ProductItemId = dto.ProductItemId
        };
        await _repository.AddAsync(entity);

        foreach (var layerDto in dto.Layers)
        {
            var layer = new RecipeLayer
            {
                RecipeId = entity.Id,
                SortOrder = layerDto.SortOrder,
                LayerType = layerDto.LayerType,
                ProductItemId = layerDto.ProductItemId,
                ThicknessMm = layerDto.ThicknessMm,
                QuantityPerUnit = layerDto.QuantityPerUnit
            };
            await _layerRepository.AddAsync(layer);
        }

        foreach (var consDto in dto.Consumables)
        {
            var consumable = new RecipeConsumable
            {
                RecipeId = entity.Id,
                ProductItemId = consDto.ProductItemId,
                ConsumptionFormula = consDto.ConsumptionFormula,
                Description = consDto.Description
            };
            await _consumableRepository.AddAsync(consumable);
        }

        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(RecipeUpdateDto dto)
    {
        var entity = await _repository.Query()
            .Include(r => r.Layers)
            .Include(r => r.Consumables)
            .FirstOrDefaultAsync(r => r.Id == dto.Id)
            ?? throw new KeyNotFoundException($"Reçete bulunamadı: {dto.Id}");

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.BaseUnitPrice = dto.BaseUnitPrice;
        entity.IsActive = dto.IsActive;
        entity.ProductItemId = dto.ProductItemId;
        await _repository.UpdateAsync(entity);

        // Mevcut katmanları sil, yenilerini ekle
        foreach (var layer in entity.Layers.ToList())
            await _layerRepository.DeleteAsync(layer);

        foreach (var layerDto in dto.Layers)
        {
            var layer = new RecipeLayer
            {
                RecipeId = entity.Id,
                SortOrder = layerDto.SortOrder,
                LayerType = layerDto.LayerType,
                ProductItemId = layerDto.ProductItemId,
                ThicknessMm = layerDto.ThicknessMm,
                QuantityPerUnit = layerDto.QuantityPerUnit
            };
            await _layerRepository.AddAsync(layer);
        }

        // Mevcut sarf malzemeleri sil, yenilerini ekle
        foreach (var cons in entity.Consumables.ToList())
            await _consumableRepository.DeleteAsync(cons);

        foreach (var consDto in dto.Consumables)
        {
            var consumable = new RecipeConsumable
            {
                RecipeId = entity.Id,
                ProductItemId = consDto.ProductItemId,
                ConsumptionFormula = consDto.ConsumptionFormula,
                Description = consDto.Description
            };
            await _consumableRepository.AddAsync(consumable);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.Query()
            .Include(r => r.Layers)
            .Include(r => r.Consumables)
            .FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new KeyNotFoundException($"Reçete bulunamadı: {id}");

        foreach (var layer in entity.Layers.ToList())
            await _layerRepository.DeleteAsync(layer);
        foreach (var cons in entity.Consumables.ToList())
            await _consumableRepository.DeleteAsync(cons);

        await _repository.DeleteAsync(entity);
    }
}
