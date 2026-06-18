using GlassSoft.Application.DTOs.Purchasing;

namespace GlassSoft.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllAsync();
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierDto> CreateAsync(SupplierCreateDto dto);
    Task UpdateAsync(SupplierUpdateDto dto);
    Task DeleteAsync(int id);
}
