using ClosedXML.Excel;
using GlassSoft.Application.DTOs.Sales;
using GlassSoft.Application.Interfaces;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Application.Services.Sales;

public class ExcelImportService : IExcelImportService
{
    private readonly IRepository<ProductItem> _productItemRepository;
    private readonly IRepository<Domain.Entities.Recipe.Recipe> _recipeRepository;

    public ExcelImportService(
        IRepository<ProductItem> productItemRepository,
        IRepository<Domain.Entities.Recipe.Recipe> recipeRepository)
    {
        _productItemRepository = productItemRepository;
        _recipeRepository = recipeRepository;
    }

    public byte[] GenerateOrderLineTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sipariş Kalemleri");

        // Header row
        var headers = new[] { "Ürün/Reçete Kodu", "En (mm)", "Boy (mm)", "Adet", "Birim Fiyat (m²)", "Notlar" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Example row
        worksheet.Cell(2, 1).Value = "4+12+4";
        worksheet.Cell(2, 2).Value = 1000;
        worksheet.Cell(2, 3).Value = 1500;
        worksheet.Cell(2, 4).Value = 5;
        worksheet.Cell(2, 5).Value = 150.00;
        worksheet.Cell(2, 6).Value = "Örnek kalem";

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<List<ExcelOrderLineImportDto>> ParseOrderLinesFromExcelAsync(Stream fileStream)
    {
        var result = new List<ExcelOrderLineImportDto>();

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheets.First();

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        // Load products and recipes for matching
        var products = await _productItemRepository.Query()
            .Where(p => !p.IsDeleted)
            .ToListAsync();

        var recipes = await _recipeRepository.Query()
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        for (int row = 2; row <= lastRow; row++)
        {
            var codeCell = worksheet.Cell(row, 1).GetText().Trim();
            if (string.IsNullOrEmpty(codeCell)) continue;

            var dto = new ExcelOrderLineImportDto
            {
                RowNumber = row,
                ItemCode = codeCell
            };

            // Parse numeric values
            if (worksheet.Cell(row, 2).TryGetValue<decimal>(out var width))
                dto.WidthMm = width;

            if (worksheet.Cell(row, 3).TryGetValue<decimal>(out var height))
                dto.HeightMm = height;

            if (worksheet.Cell(row, 4).TryGetValue<int>(out var qty))
                dto.Quantity = qty;

            if (worksheet.Cell(row, 5).TryGetValue<decimal>(out var price))
                dto.UnitPrice = price;

            dto.Notes = worksheet.Cell(row, 6).GetText().Trim();
            if (string.IsNullOrEmpty(dto.Notes)) dto.Notes = null;

            // Match code: first try ProductItem.Code, then Recipe.Code (case-insensitive)
            var matchedProduct = products.FirstOrDefault(p =>
                string.Equals(p.Code, codeCell, StringComparison.OrdinalIgnoreCase));

            if (matchedProduct != null)
            {
                dto.MatchedProductItemId = matchedProduct.Id;
                dto.MatchedItemName = matchedProduct.Name;
                dto.IsMatched = true;
            }
            else
            {
                var matchedRecipe = recipes.FirstOrDefault(r =>
                    string.Equals(r.Code, codeCell, StringComparison.OrdinalIgnoreCase));

                if (matchedRecipe != null)
                {
                    dto.MatchedRecipeId = matchedRecipe.Id;
                    dto.MatchedItemName = matchedRecipe.Name;
                    dto.IsMatched = true;
                }
                else
                {
                    dto.IsMatched = false;
                    dto.MatchError = $"'{codeCell}' kodu ile eşleşen ürün veya reçete bulunamadı.";
                }
            }

            result.Add(dto);
        }

        return result;
    }
}
