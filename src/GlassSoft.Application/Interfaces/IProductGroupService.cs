using GlassSoft.Application.DTOs.Product;

namespace GlassSoft.Application.Interfaces;

public interface IProductGroupService
{
    Task<IEnumerable<ProductGroupDto>> GetAllAsync();
    Task<ProductGroupDto?> GetByIdAsync(int id);
    Task<ProductGroupDto> CreateAsync(ProductGroupCreateDto dto);
    Task UpdateAsync(ProductGroupUpdateDto dto);
    Task DeleteAsync(int id);
}
