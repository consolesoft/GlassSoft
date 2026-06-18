using GlassSoft.Application.DTOs.Output;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Enums;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Output;

public class PrintTemplateService : IPrintTemplateService
{
    private readonly IRepository<PrintTemplate> _repository;

    public PrintTemplateService(IRepository<PrintTemplate> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PrintTemplateDto>> GetAllAsync()
    {
        return await _repository.Query()
            .OrderBy(t => t.OutputType)
            .ThenBy(t => t.Name)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<IEnumerable<PrintTemplateDto>> GetByOutputTypeAsync(OutputType type)
    {
        return await _repository.Query()
            .Where(t => t.OutputType == type)
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<PrintTemplateDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PrintTemplateDto?> GetDefaultAsync(OutputType type)
    {
        var entity = await _repository.Query()
            .Where(t => t.OutputType == type && t.IsDefault)
            .FirstOrDefaultAsync();
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<PrintTemplateDto> CreateAsync(PrintTemplateCreateDto dto)
    {
        if (dto.IsDefault)
            await ClearDefaultAsync(dto.OutputType);

        var entity = new PrintTemplate
        {
            Name = dto.Name,
            OutputType = dto.OutputType,
            HtmlContent = dto.HtmlContent,
            IsDefault = dto.IsDefault,
            Description = dto.Description
        };

        await _repository.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task UpdateAsync(PrintTemplateUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Şablon bulunamadı: {dto.Id}");

        if (dto.IsDefault && !entity.IsDefault)
            await ClearDefaultAsync(dto.OutputType);

        entity.Name = dto.Name;
        entity.OutputType = dto.OutputType;
        entity.HtmlContent = dto.HtmlContent;
        entity.IsDefault = dto.IsDefault;
        entity.Description = dto.Description;

        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Şablon bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }

    public async Task<PrintTemplateDto> CopyAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Şablon bulunamadı: {id}");

        var copy = new PrintTemplate
        {
            Name = entity.Name + " (Kopya)",
            OutputType = entity.OutputType,
            HtmlContent = entity.HtmlContent,
            IsDefault = false,
            Description = entity.Description
        };

        await _repository.AddAsync(copy);
        return MapToDto(copy);
    }

    private async Task ClearDefaultAsync(OutputType type)
    {
        var defaults = await _repository.Query()
            .Where(t => t.OutputType == type && t.IsDefault)
            .ToListAsync();

        foreach (var t in defaults)
        {
            t.IsDefault = false;
            await _repository.UpdateAsync(t);
        }
    }

    private static PrintTemplateDto MapToDto(PrintTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        OutputType = t.OutputType,
        HtmlContent = t.HtmlContent,
        IsDefault = t.IsDefault,
        Description = t.Description,
        CreatedAt = t.CreatedAt
    };
}
