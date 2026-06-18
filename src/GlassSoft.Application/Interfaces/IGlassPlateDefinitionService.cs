using GlassSoft.Application.DTOs.Product;

namespace GlassSoft.Application.Interfaces;

public interface IGlassPlateDefinitionService
{
    Task<IEnumerable<GlassPlateDefinitionDto>> GetAllAsync();
    Task<GlassPlateDefinitionDto?> GetByIdAsync(int id);
    Task<GlassPlateDefinitionDto> CreateAsync(GlassPlateDefinitionCreateDto dto);
    Task UpdateAsync(GlassPlateDefinitionUpdateDto dto);
    Task DeleteAsync(int id);
}
