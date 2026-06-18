using GlassSoft.Application.DTOs.Product;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Product;

public class GlassPlateDefinitionService : IGlassPlateDefinitionService
{
    private readonly IRepository<GlassPlateDefinition> _repository;

    public GlassPlateDefinitionService(IRepository<GlassPlateDefinition> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GlassPlateDefinitionDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Select(g => new GlassPlateDefinitionDto
            {
                Id = g.Id,
                Name = g.Name,
                WidthMm = g.WidthMm,
                HeightMm = g.HeightMm,
                IsDefault = g.IsDefault
            })
            .OrderByDescending(g => g.WidthMm)
            .ToListAsync();
    }

    public async Task<GlassPlateDefinitionDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(g => g.Id == id)
            .Select(g => new GlassPlateDefinitionDto
            {
                Id = g.Id,
                Name = g.Name,
                WidthMm = g.WidthMm,
                HeightMm = g.HeightMm,
                IsDefault = g.IsDefault
            })
            .FirstOrDefaultAsync();
    }

    public async Task<GlassPlateDefinitionDto> CreateAsync(GlassPlateDefinitionCreateDto dto)
    {
        var entity = new GlassPlateDefinition
        {
            Name = dto.Name,
            WidthMm = dto.WidthMm,
            HeightMm = dto.HeightMm,
            IsDefault = dto.IsDefault
        };
        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(GlassPlateDefinitionUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Plaka tanımı bulunamadı: {dto.Id}");
        entity.Name = dto.Name;
        entity.WidthMm = dto.WidthMm;
        entity.HeightMm = dto.HeightMm;
        entity.IsDefault = dto.IsDefault;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Plaka tanımı bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }
}
