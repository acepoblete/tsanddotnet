using FunctionExecutor.Models;
using FunctionExecutor.Models.Dto;

namespace FunctionExecutor.Services;

public interface IFunctionWrapperService
{
    Task<FunctionWrapperDto?> GetByIdAsync(int id);
    Task<List<FunctionWrapperDto>> GetAllAsync();
    Task<FunctionWrapperDto> CreateAsync(CreateFunctionWrapperRequest request);
    Task<FunctionWrapperDto?> UpdateAsync(int id, UpdateFunctionWrapperRequest request);
    Task<bool> DeleteAsync(int id);
    Task<ExecutionResultDto> ExecuteAsync(int functionWrapperId);
}
