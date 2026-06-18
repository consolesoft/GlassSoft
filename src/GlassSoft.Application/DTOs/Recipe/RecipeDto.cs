namespace GlassSoft.Application.DTOs.Recipe;

public class RecipeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseUnitPrice { get; set; }
    public bool IsActive { get; set; }
    public int? ProductItemId { get; set; }
    public string? ProductItemName { get; set; }
    public int LayerCount { get; set; }
    public int ConsumableCount { get; set; }
    public List<RecipeLayerDto> Layers { get; set; } = new();
    public List<RecipeConsumableDto> Consumables { get; set; } = new();
}

public class RecipeCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseUnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public int? ProductItemId { get; set; }
    public List<RecipeLayerCreateDto> Layers { get; set; } = new();
    public List<RecipeConsumableCreateDto> Consumables { get; set; } = new();
}

public class RecipeUpdateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseUnitPrice { get; set; }
    public bool IsActive { get; set; }
    public int? ProductItemId { get; set; }
    public List<RecipeLayerCreateDto> Layers { get; set; } = new();
    public List<RecipeConsumableCreateDto> Consumables { get; set; } = new();
}

public class RecipeLayerDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string LayerType { get; set; } = string.Empty;
    public string LayerTypeDisplay => LayerType == "Glass" ? "Cam" : "Ara Parça";
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public decimal ThicknessMm { get; set; }
    public int QuantityPerUnit { get; set; }
}

public class RecipeLayerCreateDto
{
    public int SortOrder { get; set; }
    public string LayerType { get; set; } = "Glass";
    public int ProductItemId { get; set; }
    public decimal ThicknessMm { get; set; }
    public int QuantityPerUnit { get; set; } = 1;
}

public class RecipeConsumableDto
{
    public int Id { get; set; }
    public int ProductItemId { get; set; }
    public string ProductItemName { get; set; } = string.Empty;
    public string ConsumptionFormula { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RecipeConsumableCreateDto
{
    public int ProductItemId { get; set; }
    public string ConsumptionFormula { get; set; } = string.Empty;
    public string? Description { get; set; }
}
