namespace GlassSoft.Application.Interfaces;

public interface IExcelImportService
{
    byte[] GenerateOrderLineTemplate();
    Task<List<DTOs.Sales.ExcelOrderLineImportDto>> ParseOrderLinesFromExcelAsync(Stream fileStream);
}
