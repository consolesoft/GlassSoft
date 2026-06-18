using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Sales;

public class CustomerDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string CustomerTypeDisplay => CustomerType switch
    {
        CustomerType.Musteri => "Müşteri",
        CustomerType.Tedarikci => "Tedarikçi",
        CustomerType.HerIkisi => "Müşteri + Tedarikçi",
        _ => CustomerType.ToString()
    };
    public string CustomerTypeBadgeClass => CustomerType switch
    {
        CustomerType.Musteri => "badge-primary",
        CustomerType.Tedarikci => "badge-warning",
        CustomerType.HerIkisi => "badge-info",
        _ => "badge-secondary"
    };
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; }
    public int OrderCount { get; set; }
}

public class CustomerCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Musteri;
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
}

public class CustomerUpdateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
    public string? TaxNumber { get; set; }
    public string? TaxOffice { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; }
}
