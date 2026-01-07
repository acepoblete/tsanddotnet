using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Data;
using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public class FunctionWrapperRepository : IFunctionWrapperRepository
{
    private readonly AppDbContext _context;
    
    public FunctionWrapperRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<FunctionWrapper?> GetByIdAsync(int id)
    {
        return await _context.FunctionWrappers.FindAsync(id);
    }
    
    public async Task<FunctionWrapper?> GetByIdWithCostCodesAsync(int id)
    {
        return await _context.FunctionWrappers
            .Include(f => f.CostCodes)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    
    public async Task<List<FunctionWrapper>> GetAllAsync()
    {
        return await _context.FunctionWrappers
            .Include(f => f.CostCodes)
            .ToListAsync();
    }
    
    public async Task<FunctionWrapper> CreateAsync(FunctionWrapper wrapper)
    {
        _context.FunctionWrappers.Add(wrapper);
        await _context.SaveChangesAsync();
        return wrapper;
    }
    
    public async Task<FunctionWrapper> UpdateAsync(FunctionWrapper wrapper)
    {
        wrapper.UpdatedAt = DateTime.UtcNow;
        wrapper.Version++;
        
        _context.FunctionWrappers.Update(wrapper);
        await _context.SaveChangesAsync();
        return wrapper;
    }
    
    public async Task DeleteAsync(int id)
    {
        var wrapper = await _context.FunctionWrappers.FindAsync(id);
        if (wrapper != null)
        {
            _context.FunctionWrappers.Remove(wrapper);
            await _context.SaveChangesAsync();
        }
    }
}
