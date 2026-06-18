using GlassSoft.Application.DTOs.Product;

namespace GlassSoft.Application.Interfaces;

public interface IProductItemService
{
    Task<IEnumerable<ProductItemDto>> GetAllAsync();
    Task<IEnumerable<ProductItemDto>> GetByGroupAsync(int groupId);
    Task<ProductItemDto?> GetByIdAsync(int id);
    Task<ProductItemDto> CreateAsync(ProductItemCreateDto dto);
    Task UpdateAsync(ProductItemUpdateDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<ProductItemDto>> GetPlateProductsAsync();
    Task<string> GenerateCodeAsync(string prefix);
}
