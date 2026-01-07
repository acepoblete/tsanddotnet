using Microsoft.AspNetCore.Mvc;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Services;

namespace FunctionExecutor.Controllers;

[ApiController]
[Route("api/execute")]
public class ExecutionController : ControllerBase
{
    private readonly IFunctionWrapperService _service;
    private readonly IScriptExecutor _scriptExecutor;
    private readonly ILogger<ExecutionController> _logger;
    
    public ExecutionController(
        IFunctionWrapperService service,
        IScriptExecutor scriptExecutor,
        ILogger<ExecutionController> logger)
    {
        _service = service;
        _scriptExecutor = scriptExecutor;
        _logger = logger;
    }
    
    /// <summary>
    /// Execute a stored function wrapper by ID
    /// </summary>
    [HttpPost("{functionWrapperId:int}")]
    public async Task<ActionResult<ExecutionResultDto>> Execute(int functionWrapperId)
    {
        _logger.LogInformation("Executing FunctionWrapper {Id}", functionWrapperId);
        
        var result = await _service.ExecuteAsync(functionWrapperId);
        
        if (!result.Success && result.Error?.Contains("not found") == true)
        {
            return NotFound(result);
        }
        
        // Return 200 even for execution errors (they're expected behavior)
        // The client can check result.Success to know if it worked
        return Ok(result);
    }
    
    /// <summary>
    /// Validate a function string without executing it
    /// Useful for the admin UI to show syntax errors
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<ValidateResultDto> Validate([FromBody] ValidateRequest request)
    {
        var (isValid, error) = _scriptExecutor.Validate(request.TheFunction);
        
        return Ok(new ValidateResultDto(isValid, error));
    }
}

public record ValidateRequest(string TheFunction);
public record ValidateResultDto(bool IsValid, string? Error);
