using System.Diagnostics;
using System.Globalization;
using ClosedXML.Excel;
using FunctionExecutor.Models;
using FunctionExecutor.Models.Dto;
using Microsoft.Extensions.Logging;

namespace FunctionExecutor.Services;

public class ExcelCalculationService : IExcelCalculationService
{
    private readonly ILogger<ExcelCalculationService> _logger;
    private readonly string _templateBasePath;

    // Expected column positions (1-based for ClosedXML)
    private const int CmicCodeColumn = 1;      // Column A
    private const int NameColumn = 2;          // Column B
    private const int LaborColumn = 3;         // Column C
    private const int QtyColumn = 4;           // Column D
    private const int MaterialsColumn = 5;     // Column E
    private const int OtherColumn = 6;         // Column F
    private const int TotalCostColumn = 7;     // Column G

    public ExcelCalculationService(ILogger<ExcelCalculationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _templateBasePath = configuration.GetValue<string>("ExcelTemplates:BasePath") ?? "Templates";
    }

    public async Task<CalculationResultDto> ExecuteAsync(
        string templateFilePath,
        IEnumerable<WorkbookCostCode> costCodes,
        IEnumerable<CostCodeUpdateInput>? inputOverrides = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var fullPath = Path.Combine(_templateBasePath, templateFilePath);

            if (!File.Exists(fullPath))
            {
                return new CalculationResultDto(
                    Success: false,
                    Error: $"Template file not found: {templateFilePath}",
                    ExecutionTimeMs: stopwatch.ElapsedMilliseconds,
                    UpdatedCostCodes: new List<WorkbookCostCodeDto>()
                );
            }

            // Create a working copy to avoid modifying the template
            using var stream = new MemoryStream();
            await using (var fileStream = File.OpenRead(fullPath))
            {
                await fileStream.CopyToAsync(stream);
            }
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            // Build lookup from CMIC code to cost code
            var costCodeLookup = costCodes.ToDictionary(cc => cc.CmicCode, cc => cc);

            // Build override lookup if provided
            var overrideLookup = inputOverrides?.ToDictionary(o => o.CmicCode, o => o)
                ?? new Dictionary<string, CostCodeUpdateInput>();

            // Find all rows with CMIC codes and populate data
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            var rowMappings = new Dictionary<string, int>(); // CMIC code -> row number

            for (int row = 2; row <= lastRow; row++) // Start from row 2, assuming row 1 is header
            {
                var cmicCode = worksheet.Cell(row, CmicCodeColumn).GetString()?.Trim();
                if (string.IsNullOrEmpty(cmicCode))
                    continue;

                rowMappings[cmicCode] = row;

                // Check for override first, then use stored values
                if (overrideLookup.TryGetValue(cmicCode, out var overrideInput))
                {
                    if (costCodeLookup.TryGetValue(cmicCode, out var costCode))
                    {
                        // Use override values if provided, otherwise use existing values
                        worksheet.Cell(row, LaborColumn).Value = (double)(overrideInput.Labor ?? costCode.Labor);
                        worksheet.Cell(row, QtyColumn).Value = (double)(overrideInput.Qty ?? costCode.Qty);
                        worksheet.Cell(row, MaterialsColumn).Value = (double)(overrideInput.Materials ?? costCode.Materials);
                        worksheet.Cell(row, OtherColumn).Value = (double)(overrideInput.Other ?? costCode.Other);
                    }
                }
                else if (costCodeLookup.TryGetValue(cmicCode, out var costCode))
                {
                    // Use stored values
                    worksheet.Cell(row, LaborColumn).Value = (double)costCode.Labor;
                    worksheet.Cell(row, QtyColumn).Value = (double)costCode.Qty;
                    worksheet.Cell(row, MaterialsColumn).Value = (double)costCode.Materials;
                    worksheet.Cell(row, OtherColumn).Value = (double)costCode.Other;
                }
            }

            // Force recalculation of all formulas
            workbook.RecalculateAllFormulas();

            // Extract results
            var updatedCostCodes = new List<WorkbookCostCodeDto>();

            foreach (var costCode in costCodes)
            {
                if (rowMappings.TryGetValue(costCode.CmicCode, out var row))
                {
                    // After RecalculateAllFormulas(), use Value property which returns calculated values
                    var totalCost = GetDecimalValue(worksheet.Cell(row, TotalCostColumn));
                    var labor = GetDecimalValue(worksheet.Cell(row, LaborColumn));
                    var qty = GetDecimalValue(worksheet.Cell(row, QtyColumn));
                    var materials = GetDecimalValue(worksheet.Cell(row, MaterialsColumn));
                    var other = GetDecimalValue(worksheet.Cell(row, OtherColumn));

                    updatedCostCodes.Add(new WorkbookCostCodeDto(
                        Id: costCode.Id,
                        WorkbookId: costCode.WorkbookId,
                        CostCodeId: costCode.CostCodeId,
                        CmicCode: costCode.CmicCode,
                        Name: costCode.Name,
                        Labor: labor,
                        Qty: qty,
                        Materials: materials,
                        Other: other,
                        TotalCost: totalCost
                    ));
                }
                else
                {
                    // Cost code not found in Excel, keep existing values
                    updatedCostCodes.Add(new WorkbookCostCodeDto(
                        Id: costCode.Id,
                        WorkbookId: costCode.WorkbookId,
                        CostCodeId: costCode.CostCodeId,
                        CmicCode: costCode.CmicCode,
                        Name: costCode.Name,
                        Labor: costCode.Labor,
                        Qty: costCode.Qty,
                        Materials: costCode.Materials,
                        Other: costCode.Other,
                        TotalCost: costCode.TotalCost
                    ));
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Excel calculation completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return new CalculationResultDto(
                Success: true,
                Error: null,
                ExecutionTimeMs: stopwatch.ElapsedMilliseconds,
                UpdatedCostCodes: updatedCostCodes
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Excel calculation failed");

            return new CalculationResultDto(
                Success: false,
                Error: ex.Message,
                ExecutionTimeMs: stopwatch.ElapsedMilliseconds,
                UpdatedCostCodes: new List<WorkbookCostCodeDto>()
            );
        }
    }

    private static decimal GetDecimalValue(IXLCell cell)
    {
        // Use CachedValue after recalculation and try to convert to double
        var cellValue = cell.CachedValue;
        if (cellValue.TryConvert(out double value, CultureInfo.InvariantCulture))
        {
            return (decimal)value;
        }

        return 0;
    }
}
