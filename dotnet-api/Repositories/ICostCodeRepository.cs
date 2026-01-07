using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public interface ICostCodeRepository
{
    Task<CostCode?> GetByIdAsync(int id);
    Task<List<CostCode>> GetAllAsync();
    Task<List<CostCode>> GetChildrenAsync(int parentId);
    
    /// <summary>
    /// Gets a cost code and ALL its descendants (children, grandchildren, etc.)
    /// This is used when adding a cost code to a FunctionWrapper
    /// </summary>
    Task<List<CostCode>> GetWithDescendantsAsync(int id);
    
    /// <summary>
    /// Gets multiple cost codes with all their descendants
    /// Used when user selects multiple root codes for a FunctionWrapper
    /// </summary>
    Task<List<CostCode>> GetManyWithDescendantsAsync(IEnumerable<int> ids);
}
