using GlassSoft.Domain.Enums;

namespace GlassSoft.Application.DTOs.Product;

public class ProductItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductGroupId { get; set; }
    public string ProductGroupName { get; set; } = string.Empty;
    public UnitType Unit { get; set; }
    public string UnitDisplay => Unit switch
    {
        UnitType.Adet => "Adet",
        UnitType.MetreKare => "m²",
        UnitType.Metre => "m",
        UnitType.Kilogram => "kg",
        UnitType.Litre => "lt",
        _ => Unit.ToString()
    };
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsPlate { get; set; }
    public decimal? ThicknessMm { get; set; }
    public decimal CurrentStockQty { get; set; }
    public int CurrentStockPlateCount { get; set; }
}

public class ProductItemCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductGroupId { get; set; }
    public UnitType Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPlate { get; set; }
    public decimal? ThicknessMm { get; set; }
}

public class ProductItemUpdateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductGroupId { get; set; }
    public UnitType Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsPlate { get; set; }
    public decimal? ThicknessMm { get; set; }
}
