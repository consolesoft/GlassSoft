using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Product;

public class ProductGroupService : IProductGroupService
{
    private readonly IRepository<ProductGroup> _repository;

    public ProductGroupService(IRepository<ProductGroup> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ProductGroupDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Select(g => new ProductGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IsActive = g.IsActive,
                ProductCount = g.Products.Count(p => !p.IsDeleted)
            })
            .OrderBy(g => g.Name)
            .ToListAsync();
    }

    public async Task<ProductGroupDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(g => g.Id == id)
            .Select(g => new ProductGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                IsActive = g.IsActive,
                ProductCount = g.Products.Count(p => !p.IsDeleted)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductGroupDto> CreateAsync(ProductGroupCreateDto dto)
    {
        var entity = new ProductGroup
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
        await _repository.AddAsync(entity);
        return new ProductGroupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }

    public async Task UpdateAsync(ProductGroupUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Ürün grubu bulunamadı: {dto.Id}");
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Ürün grubu bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }
}
