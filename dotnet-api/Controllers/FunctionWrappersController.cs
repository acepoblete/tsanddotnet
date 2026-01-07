using Microsoft.AspNetCore.Mvc;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Services;

namespace FunctionExecutor.Controllers;

[ApiController]
[Route("api/function-wrappers")]
public class FunctionWrappersController : ControllerBase
{
    private readonly IFunctionWrapperService _service;
    private readonly ILogger<FunctionWrappersController> _logger;
    
    public FunctionWrappersController(
        IFunctionWrapperService service,
        ILogger<FunctionWrappersController> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    /// <summary>
    /// Get all function wrappers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FunctionWrapperDto>>> GetAll()
    {
        var wrappers = await _service.GetAllAsync();
        return Ok(wrappers);
    }
    
    /// <summary>
    /// Get a function wrapper by ID (includes all cost codes)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FunctionWrapperDto>> GetById(int id)
    {
        var wrapper = await _service.GetByIdAsync(id);
        if (wrapper == null)
        {
            return NotFound(new { error = $"FunctionWrapper with ID {id} not found" });
        }
        return Ok(wrapper);
    }
    
    /// <summary>
    /// Create a new function wrapper
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FunctionWrapperDto>> Create(
        [FromBody] CreateFunctionWrapperRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Update an existing function wrapper
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FunctionWrapperDto>> Update(
        int id, 
        [FromBody] UpdateFunctionWrapperRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request);
            if (updated == null)
            {
                return NotFound(new { error = $"FunctionWrapper with ID {id} not found" });
            }
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Delete a function wrapper
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { error = $"FunctionWrapper with ID {id} not found" });
        }
        return NoContent();
    }
}
