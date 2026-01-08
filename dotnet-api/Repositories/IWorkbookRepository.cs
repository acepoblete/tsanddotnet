using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public interface IWorkbookRepository
{
    Task<Workbook?> GetByIdAsync(int id);
    Task<Workbook?> GetByIdWithCostCodesAsync(int id);
    Task<IEnumerable<Workbook>> GetAllAsync();
    Task<Workbook> CreateAsync(Workbook workbook);
    Task UpdateAsync(Workbook workbook);
    Task DeleteAsync(int id);

    // WorkbookCostCode operations
    Task<WorkbookCostCode?> GetCostCodeByIdAsync(int id);
    Task<WorkbookCostCode?> GetCostCodeByCmicAsync(int workbookId, string cmicCode);
    Task<IEnumerable<WorkbookCostCode>> GetCostCodesByWorkbookIdAsync(int workbookId);
    Task AddCostCodesAsync(IEnumerable<WorkbookCostCode> costCodes);
    Task UpdateCostCodesAsync(IEnumerable<WorkbookCostCode> costCodes);
}
