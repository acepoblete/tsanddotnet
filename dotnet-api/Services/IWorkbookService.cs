using FunctionExecutor.Models.Dto;

namespace FunctionExecutor.Services;

public interface IWorkbookService
{
    Task<WorkbookDto?> GetByIdAsync(int id);
    Task<IEnumerable<WorkbookSummaryDto>> GetAllAsync();
    Task<WorkbookDto> CreateAsync(CreateWorkbookRequest request);
    Task<WorkbookDto?> UpdateAsync(int id, UpdateWorkbookRequest request);
    Task<bool> DeleteAsync(int id);
    Task<WorkbookDto?> UpdateCostCodesAsync(int workbookId, IEnumerable<CostCodeUpdateInput> updates);
    Task<CalculationResultDto> ExecuteCalculationAsync(int workbookId, CalculationRequest? request = null);
}
