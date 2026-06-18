using GlassSoft.Application.DTOs.Recipe;

namespace GlassSoft.Application.Interfaces;

public interface IRecipeService
{
    Task<IEnumerable<RecipeDto>> GetAllAsync();
    Task<RecipeDto?> GetByIdAsync(int id);
    Task<RecipeDto> CreateAsync(RecipeCreateDto dto);
    Task UpdateAsync(RecipeUpdateDto dto);
    Task DeleteAsync(int id);
}
