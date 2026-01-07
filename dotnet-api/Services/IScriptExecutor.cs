using FunctionExecutor.Models;

namespace FunctionExecutor.Services;

public interface IScriptExecutor
{
    /// <summary>
    /// Executes the function defined in the FunctionWrapper
    /// Uses the pre-loaded CostCodes from the wrapper
    /// </summary>
    /// <param name="wrapper">FunctionWrapper with CostCodes loaded</param>
    /// <returns>ExecutionResult with numeric result or error</returns>
    ExecutionResult Execute(FunctionWrapper wrapper);
    
    /// <summary>
    /// Validates that a function string is syntactically correct
    /// Does not execute the function
    /// </summary>
    (bool IsValid, string? Error) Validate(string functionString);
}
