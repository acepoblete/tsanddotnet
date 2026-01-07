using FunctionExecutor.Models;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Repositories;

namespace FunctionExecutor.Services;

public class FunctionWrapperService : IFunctionWrapperService
{
    private readonly IFunctionWrapperRepository _wrapperRepo;
    private readonly ICostCodeRepository _costCodeRepo;
    private readonly IScriptExecutor _scriptExecutor;
    private readonly ILogger<FunctionWrapperService> _logger;
    
    public FunctionWrapperService(
        IFunctionWrapperRepository wrapperRepo,
        ICostCodeRepository costCodeRepo,
        IScriptExecutor scriptExecutor,
        ILogger<FunctionWrapperService> logger)
    {
        _wrapperRepo = wrapperRepo;
        _costCodeRepo = costCodeRepo;
        _scriptExecutor = scriptExecutor;
        _logger = logger;
    }
    
    public async Task<FunctionWrapperDto?> GetByIdAsync(int id)
    {
        var wrapper = await _wrapperRepo.GetByIdWithCostCodesAsync(id);
        return wrapper == null ? null : MapToDto(wrapper);
    }
    
    public async Task<List<FunctionWrapperDto>> GetAllAsync()
    {
        var wrappers = await _wrapperRepo.GetAllAsync();
        return wrappers.Select(MapToDto).ToList();
    }
    
    public async Task<FunctionWrapperDto> CreateAsync(CreateFunctionWrapperRequest request)
    {
        // Validate the function syntax first
        var (isValid, error) = _scriptExecutor.Validate(request.TheFunction);
        if (!isValid)
        {
            throw new ArgumentException($"Invalid function: {error}");
        }
        
        // Load selected cost codes with all their descendants
        var costCodes = await _costCodeRepo.GetManyWithDescendantsAsync(request.SelectedCostCodeIds);
        
        if (costCodes.Count == 0)
        {
            throw new ArgumentException("At least one valid cost code must be selected");
        }
        
        var wrapper = new FunctionWrapper
        {
            Name = request.Name,
            Description = request.Description,
            TheFunction = request.TheFunction,
            CostCodes = costCodes,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var created = await _wrapperRepo.CreateAsync(wrapper);
        
        _logger.LogInformation(
            "Created FunctionWrapper {Id} with {CostCodeCount} cost codes",
            created.Id, costCodes.Count);
        
        return MapToDto(created);
    }
    
    public async Task<FunctionWrapperDto?> UpdateAsync(int id, UpdateFunctionWrapperRequest request)
    {
        var wrapper = await _wrapperRepo.GetByIdWithCostCodesAsync(id);
        if (wrapper == null) return null;
        
        // Validate the function syntax
        var (isValid, error) = _scriptExecutor.Validate(request.TheFunction);
        if (!isValid)
        {
            throw new ArgumentException($"Invalid function: {error}");
        }
        
        // Load new cost codes with descendants
        var costCodes = await _costCodeRepo.GetManyWithDescendantsAsync(request.SelectedCostCodeIds);
        
        if (costCodes.Count == 0)
        {
            throw new ArgumentException("At least one valid cost code must be selected");
        }
        
        // Update wrapper
        wrapper.Name = request.Name;
        wrapper.Description = request.Description;
        wrapper.TheFunction = request.TheFunction;
        wrapper.CostCodes.Clear();
        foreach (var code in costCodes)
        {
            wrapper.CostCodes.Add(code);
        }
        
        var updated = await _wrapperRepo.UpdateAsync(wrapper);
        
        _logger.LogInformation(
            "Updated FunctionWrapper {Id} to version {Version}",
            updated.Id, updated.Version);
        
        return MapToDto(updated);
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var wrapper = await _wrapperRepo.GetByIdAsync(id);
        if (wrapper == null) return false;
        
        await _wrapperRepo.DeleteAsync(id);
        
        _logger.LogInformation("Deleted FunctionWrapper {Id}", id);
        
        return true;
    }
    
    public async Task<ExecutionResultDto> ExecuteAsync(int functionWrapperId)
    {
        var wrapper = await _wrapperRepo.GetByIdWithCostCodesAsync(functionWrapperId);
        
        if (wrapper == null)
        {
            return new ExecutionResultDto(
                Success: false,
                Result: null,
                Error: $"FunctionWrapper with ID {functionWrapperId} not found",
                ExecutionTimeMs: 0
            );
        }
        
        _logger.LogDebug(
            "Executing FunctionWrapper {Id} v{Version} with {CostCodeCount} cost codes",
            wrapper.Id, wrapper.Version, wrapper.CostCodes.Count);
        
        var result = _scriptExecutor.Execute(wrapper);
        
        return new ExecutionResultDto(
            Success: result.Success,
            Result: result.Result,
            Error: result.Error,
            ExecutionTimeMs: result.ExecutionTimeMs
        );
    }
    
    private static FunctionWrapperDto MapToDto(FunctionWrapper wrapper)
    {
        return new FunctionWrapperDto(
            Id: wrapper.Id,
            Version: wrapper.Version,
            Name: wrapper.Name,
            Description: wrapper.Description,
            TheFunction: wrapper.TheFunction,
            CostCodes: wrapper.CostCodes.Select(c => new CostCodeDto(
                Id: c.Id,
                ParentId: c.ParentId,
                Name: c.Name,
                Value: c.Value
            )).ToList(),
            CreatedAt: wrapper.CreatedAt,
            UpdatedAt: wrapper.UpdatedAt
        );
    }
}
