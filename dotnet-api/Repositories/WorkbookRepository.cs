using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Data;
using FunctionExecutor.Models;

namespace FunctionExecutor.Repositories;

public class WorkbookRepository : IWorkbookRepository
{
    private readonly AppDbContext _context;

    public WorkbookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Workbook?> GetByIdAsync(int id)
    {
        return await _context.Workbooks.FindAsync(id);
    }

    public async Task<Workbook?> GetByIdWithCostCodesAsync(int id)
    {
        return await _context.Workbooks
            .Include(w => w.WorkbookCostCodes)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Workbook>> GetAllAsync()
    {
        return await _context.Workbooks
            .Include(w => w.WorkbookCostCodes)
            .ToListAsync();
    }

    public async Task<Workbook> CreateAsync(Workbook workbook)
    {
        _context.Workbooks.Add(workbook);
        await _context.SaveChangesAsync();
        return workbook;
    }

    public async Task UpdateAsync(Workbook workbook)
    {
        _context.Workbooks.Update(workbook);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var workbook = await _context.Workbooks.FindAsync(id);
        if (workbook != null)
        {
            _context.Workbooks.Remove(workbook);
            await _context.SaveChangesAsync();
        }
    }

    // WorkbookCostCode operations
    public async Task<WorkbookCostCode?> GetCostCodeByIdAsync(int id)
    {
        return await _context.WorkbookCostCodes.FindAsync(id);
    }

    public async Task<WorkbookCostCode?> GetCostCodeByCmicAsync(int workbookId, string cmicCode)
    {
        return await _context.WorkbookCostCodes
            .FirstOrDefaultAsync(wcc => wcc.WorkbookId == workbookId && wcc.CmicCode == cmicCode);
    }

    public async Task<IEnumerable<WorkbookCostCode>> GetCostCodesByWorkbookIdAsync(int workbookId)
    {
        return await _context.WorkbookCostCodes
            .Where(wcc => wcc.WorkbookId == workbookId)
            .ToListAsync();
    }

    public async Task AddCostCodesAsync(IEnumerable<WorkbookCostCode> costCodes)
    {
        _context.WorkbookCostCodes.AddRange(costCodes);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCostCodesAsync(IEnumerable<WorkbookCostCode> costCodes)
    {
        _context.WorkbookCostCodes.UpdateRange(costCodes);
        await _context.SaveChangesAsync();
    }
}
