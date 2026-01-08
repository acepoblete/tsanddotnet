using FunctionExecutor.Models;
using FunctionExecutor.Models.Dto;

namespace FunctionExecutor.Services;

public interface IExcelCalculationService
{
    Task<CalculationResultDto> ExecuteAsync(
        string templateFilePath,
        IEnumerable<WorkbookCostCode> costCodes,
        IEnumerable<CostCodeUpdateInput>? inputOverrides = null);
}
