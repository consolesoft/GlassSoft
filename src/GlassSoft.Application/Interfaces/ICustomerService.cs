using GlassSoft.Application.DTOs.Sales;

namespace GlassSoft.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CustomerCreateDto dto);
    Task UpdateAsync(CustomerUpdateDto dto);
    Task DeleteAsync(int id);
    Task<CustomerReferencesDto> GetReferencesAsync(int id);
    Task ForceDeleteAsync(int id);
    Task<string> GenerateCodeAsync(string prefix);
}

public class CustomerReferencesDto
{
    public int OrderCount { get; set; }
    public int PurchaseOrderCount { get; set; }
    public int AccountTransactionCount { get; set; }
    public int ChequeNoteCount { get; set; }
    public int CashTransactionCount { get; set; }
    public bool HasAnyReference =>
        OrderCount > 0 || PurchaseOrderCount > 0 || AccountTransactionCount > 0 || ChequeNoteCount > 0 || CashTransactionCount > 0;
}
