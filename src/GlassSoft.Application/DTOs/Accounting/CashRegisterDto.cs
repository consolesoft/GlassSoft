namespace GlassSoft.Application.DTOs.Accounting;

public class CashRegisterDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "TRY";
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int TransactionCount { get; set; }
}

public class CashRegisterCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "TRY";
    public decimal OpeningBalance { get; set; }
    public string? Description { get; set; }
}

public class CashRegisterUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "TRY";
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
