using GlassSoft.Application.DTOs.Output;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Interfaces;

public interface IPrintTemplateService
{
    Task<IEnumerable<PrintTemplateDto>> GetAllAsync();
    Task<IEnumerable<PrintTemplateDto>> GetByOutputTypeAsync(OutputType type);
    Task<PrintTemplateDto?> GetByIdAsync(int id);
    Task<PrintTemplateDto?> GetDefaultAsync(OutputType type);
    Task<PrintTemplateDto> CreateAsync(PrintTemplateCreateDto dto);
    Task UpdateAsync(PrintTemplateUpdateDto dto);
    Task DeleteAsync(int id);
    Task<PrintTemplateDto> CopyAsync(int id);
}
