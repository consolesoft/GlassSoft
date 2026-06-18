namespace GlassSoft.Application.Interfaces;

public interface ISystemSettingService
{
    Task<string?> GetValueAsync(string key);
    Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0);
    Task SetValueAsync(string key, string value, string? description = null);
    Task<Dictionary<string, string>> GetAllAsync();
}
