using Microsoft.AspNetCore.Mvc;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Repositories;

namespace FunctionExecutor.Controllers;

[ApiController]
[Route("api/cost-codes")]
public class CostCodesController : ControllerBase
{
    private readonly ICostCodeRepository _repository;
    
    public CostCodesController(ICostCodeRepository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// Get all cost codes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CostCodeDto>>> GetAll()
    {
        var codes = await _repository.GetAllAsync();
        var dtos = codes.Select(c => new CostCodeDto(c.Id, c.ParentId, c.Name, c.Value)).ToList();
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get a cost code by ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CostCodeDto>> GetById(int id)
    {
        var code = await _repository.GetByIdAsync(id);
        if (code == null)
        {
            return NotFound(new { error = $"CostCode with ID {id} not found" });
        }
        return Ok(new CostCodeDto(code.Id, code.ParentId, code.Name, code.Value));
    }
    
    /// <summary>
    /// Get a cost code with all its descendants
    /// Useful for previewing what will be included in a FunctionWrapper
    /// </summary>
    [HttpGet("{id:int}/with-descendants")]
    public async Task<ActionResult<List<CostCodeDto>>> GetWithDescendants(int id)
    {
        var codes = await _repository.GetWithDescendantsAsync(id);
        if (codes.Count == 0)
        {
            return NotFound(new { error = $"CostCode with ID {id} not found" });
        }
        var dtos = codes.Select(c => new CostCodeDto(c.Id, c.ParentId, c.Name, c.Value)).ToList();
        return Ok(dtos);
    }
    
    /// <summary>
    /// Get children of a cost code
    /// </summary>
    [HttpGet("{id:int}/children")]
    public async Task<ActionResult<List<CostCodeDto>>> GetChildren(int id)
    {
        var codes = await _repository.GetChildrenAsync(id);
        var dtos = codes.Select(c => new CostCodeDto(c.Id, c.ParentId, c.Name, c.Value)).ToList();
        return Ok(dtos);
    }
}
