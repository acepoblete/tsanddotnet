using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public interface IFunctionWrapperRepository
{
    Task<FunctionWrapper?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets FunctionWrapper with all associated CostCodes loaded
    /// This is the primary method for execution
    /// </summary>
    Task<FunctionWrapper?> GetByIdWithCostCodesAsync(int id);
    
    Task<List<FunctionWrapper>> GetAllAsync();
    Task<FunctionWrapper> CreateAsync(FunctionWrapper wrapper);
    Task<FunctionWrapper> UpdateAsync(FunctionWrapper wrapper);
    Task DeleteAsync(int id);
}
