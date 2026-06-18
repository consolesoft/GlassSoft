using GlassSoft.Application.DTOs.Accounting;
using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.Interfaces;

public interface IChequeNoteService
{
    Task<IEnumerable<ChequeNoteDto>> GetAllAsync();
    Task<ChequeNoteDto?> GetByIdAsync(int id);
    Task<ChequeNoteDto> CreateAsync(ChequeNoteCreateDto dto);
    Task UpdateAsync(ChequeNoteUpdateDto dto);
    Task UpdateStatusAsync(int id, ChequeNoteStatus status);
    Task DeleteAsync(int id);
}
