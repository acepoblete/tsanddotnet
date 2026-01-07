using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Data;
using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public class CostCodeRepository : ICostCodeRepository
{
    private readonly AppDbContext _context;
    
    public CostCodeRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<CostCode?> GetByIdAsync(int id)
    {
        return await _context.CostCodes.FindAsync(id);
    }
    
    public async Task<List<CostCode>> GetAllAsync()
    {
        return await _context.CostCodes.ToListAsync();
    }
    
    public async Task<List<CostCode>> GetChildrenAsync(int parentId)
    {
        return await _context.CostCodes
            .Where(c => c.ParentId == parentId)
            .ToListAsync();
    }
    
    public async Task<List<CostCode>> GetWithDescendantsAsync(int id)
    {
        var result = new List<CostCode>();
        
        // Get the root node
        var root = await _context.CostCodes.FindAsync(id);
        if (root == null) return result;
        
        result.Add(root);
        
        // Recursively get all descendants
        await LoadDescendantsAsync(id, result);
        
        return result;
    }
    
    public async Task<List<CostCode>> GetManyWithDescendantsAsync(IEnumerable<int> ids)
    {
        var result = new List<CostCode>();
        var addedIds = new HashSet<int>();
        
        foreach (var id in ids)
        {
            var withDescendants = await GetWithDescendantsAsync(id);
            foreach (var code in withDescendants)
            {
                // Avoid duplicates if hierarchies overlap
                if (addedIds.Add(code.Id))
                {
                    result.Add(code);
                }
            }
        }
        
        return result;
    }
    
    private async Task LoadDescendantsAsync(int parentId, List<CostCode> result)
    {
        var children = await _context.CostCodes
            .Where(c => c.ParentId == parentId)
            .ToListAsync();
        
        foreach (var child in children)
        {
            result.Add(child);
            await LoadDescendantsAsync(child.Id, result);
        }
    }
}
