using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Product;

public class ProductItemService : IProductItemService
{
    private readonly IRepository<ProductItem> _repository;
    private readonly IRepository<StockEntry> _stockRepository;

    public ProductItemService(IRepository<ProductItem> repository, IRepository<StockEntry> stockRepository)
    {
        _repository = repository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<ProductItemDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(p => p.ProductGroup)
            .Select(p => new ProductItemDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                IsPlate = p.IsPlate,
                ThicknessMm = p.ThicknessMm
            })
            .OrderBy(p => p.ProductGroupName).ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductItemDto>> GetByGroupAsync(int groupId)
    {
        return await _repository.Query()
            .Where(p => p.ProductGroupId == groupId)
            .Include(p => p.ProductGroup)
            .Select(p => new ProductItemDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                IsPlate = p.IsPlate,
                ThicknessMm = p.ThicknessMm
            })
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<ProductItemDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(p => p.Id == id)
            .Include(p => p.ProductGroup)
            .Select(p => new ProductItemDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                IsPlate = p.IsPlate,
                ThicknessMm = p.ThicknessMm
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductItemDto> CreateAsync(ProductItemCreateDto dto)
    {
        var entity = new ProductItem
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            ProductGroupId = dto.ProductGroupId,
            Unit = dto.Unit,
            UnitPrice = dto.UnitPrice,
            IsActive = dto.IsActive,
            IsPlate = dto.IsPlate,
            ThicknessMm = dto.ThicknessMm
        };
        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(ProductItemUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Ürün bulunamadı: {dto.Id}");
        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.ProductGroupId = dto.ProductGroupId;
        entity.Unit = dto.Unit;
        entity.UnitPrice = dto.UnitPrice;
        entity.IsActive = dto.IsActive;
        entity.IsPlate = dto.IsPlate;
        entity.ThicknessMm = dto.ThicknessMm;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Ürün bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task<string> GenerateCodeAsync(string prefix)
    {
        var last = await _repository.Query()
            .Where(p => p.Code.StartsWith(prefix + "-"))
            .OrderByDescending(p => p.Code)
            .FirstOrDefaultAsync();

        if (last == null)
            return prefix + "-001";

        var numPart = last.Code[(prefix.Length + 1)..];
        if (int.TryParse(numPart, out var num))
            return prefix + "-" + (num + 1).ToString("D3");

        return prefix + "-001";
    }

    public async Task<IEnumerable<ProductItemDto>> GetPlateProductsAsync()
    {
        return await _repository.Query()
            .Where(p => p.IsPlate && p.IsActive)
            .Include(p => p.ProductGroup)
            .Select(p => new ProductItemDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                ProductGroupId = p.ProductGroupId,
                ProductGroupName = p.ProductGroup.Name,
                Unit = p.Unit,
                UnitPrice = p.UnitPrice,
                IsActive = p.IsActive,
                IsPlate = p.IsPlate,
                ThicknessMm = p.ThicknessMm
            })
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
