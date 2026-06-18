using GlassSoft.Application.DTOs.Purchasing;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Purchasing;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Purchasing;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;

    public SupplierService(IRepository<Supplier> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllAsync()
    {
        return await _repository.Query()
            .Include(s => s.PurchaseOrders)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Title = s.Title,
                TaxNumber = s.TaxNumber,
                TaxOffice = s.TaxOffice,
                Address = s.Address,
                City = s.City,
                Phone = s.Phone,
                Email = s.Email,
                IsActive = s.IsActive,
                OrderCount = s.PurchaseOrders.Count
            })
            .OrderBy(s => s.Title)
            .ToListAsync();
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        return await _repository.Query()
            .Where(s => s.Id == id)
            .Include(s => s.PurchaseOrders)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Code = s.Code,
                Title = s.Title,
                TaxNumber = s.TaxNumber,
                TaxOffice = s.TaxOffice,
                Address = s.Address,
                City = s.City,
                Phone = s.Phone,
                Email = s.Email,
                IsActive = s.IsActive,
                OrderCount = s.PurchaseOrders.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SupplierDto> CreateAsync(SupplierCreateDto dto)
    {
        var entity = new Supplier
        {
            Code = dto.Code,
            Title = dto.Title,
            TaxNumber = dto.TaxNumber,
            TaxOffice = dto.TaxOffice,
            Address = dto.Address,
            City = dto.City,
            Phone = dto.Phone,
            Email = dto.Email,
            IsActive = dto.IsActive
        };
        await _repository.AddAsync(entity);
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task UpdateAsync(SupplierUpdateDto dto)
    {
        var entity = await _repository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Tedarikçi bulunamadı: {dto.Id}");
        entity.Code = dto.Code;
        entity.Title = dto.Title;
        entity.TaxNumber = dto.TaxNumber;
        entity.TaxOffice = dto.TaxOffice;
        entity.Address = dto.Address;
        entity.City = dto.City;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.IsActive = dto.IsActive;
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Tedarikçi bulunamadı: {id}");
        await _repository.DeleteAsync(entity);
    }
}
