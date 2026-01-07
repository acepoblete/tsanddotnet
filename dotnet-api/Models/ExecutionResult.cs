namespace FunctionExecutor.Models;

public class ExecutionResult
{
    public bool Success { get; set; }
    public decimal? Result { get; set; }
    public string? Error { get; set; }
    public long ExecutionTimeMs { get; set; }
    
    public static ExecutionResult Ok(decimal result, long executionTimeMs) => new()
    {
        Success = true,
        Result = result,
        ExecutionTimeMs = executionTimeMs
    };
    
    public static ExecutionResult Fail(string error, long executionTimeMs = 0) => new()
    {
        Success = false,
        Error = error,
        ExecutionTimeMs = executionTimeMs
    };
}
