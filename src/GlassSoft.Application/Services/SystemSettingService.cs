using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly DbContext _context;

    public SystemSettingService(DbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetValueAsync(string key)
    {
        var setting = await _context.Set<SystemSetting>().FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0)
    {
        var val = await GetValueAsync(key);
        return val != null && decimal.TryParse(val, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public async Task SetValueAsync(string key, string value, string? description = null)
    {
        var setting = await _context.Set<SystemSetting>().FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null)
        {
            setting = new SystemSetting { Key = key, Value = value, Description = description };
            _context.Set<SystemSetting>().Add(setting);
        }
        else
        {
            setting.Value = value;
            if (description != null)
                setting.Description = description;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        return await _context.Set<SystemSetting>().ToDictionaryAsync(s => s.Key, s => s.Value);
    }
}
