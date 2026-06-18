namespace GlassSoft.Application.DTOs.Sales;

public class ExcelOrderLineImportDto
{
    public string ItemCode { get; set; } = string.Empty;
    public int? MatchedProductItemId { get; set; }
    public int? MatchedRecipeId { get; set; }
    public string? MatchedItemName { get; set; }
    public decimal WidthMm { get; set; }
    public decimal HeightMm { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public bool IsMatched { get; set; }
    public string? MatchError { get; set; }
    public int RowNumber { get; set; }
}
