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

    [HttpGet]
    public async Task<ActionResult<List<CostCodeDto>>> GetAll()
    {
        var codes = await _repository.GetAllAsync();
        var dtos = codes.Select(c => new CostCodeDto(
            c.Id, c.ParentId, c.Name, c.CmicCode,
            c.Labor, c.Qty, c.Materials, c.Other, c.TotalCost
        )).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CostCodeDto>> GetById(int id)
    {
        var code = await _repository.GetByIdAsync(id);
        if (code == null)
        {
            return NotFound(new { error = $"CostCode with ID {id} not found" });
        }
        return Ok(new CostCodeDto(
            code.Id, code.ParentId, code.Name, code.CmicCode,
            code.Labor, code.Qty, code.Materials, code.Other, code.TotalCost
        ));
    }

    [HttpGet("{id:int}/with-descendants")]
    public async Task<ActionResult<List<CostCodeDto>>> GetWithDescendants(int id)
    {
        var codes = await _repository.GetWithDescendantsAsync(id);
        if (codes.Count == 0)
        {
            return NotFound(new { error = $"CostCode with ID {id} not found" });
        }
        var dtos = codes.Select(c => new CostCodeDto(
            c.Id, c.ParentId, c.Name, c.CmicCode,
            c.Labor, c.Qty, c.Materials, c.Other, c.TotalCost
        )).ToList();
        return Ok(dtos);
    }

    [HttpGet("{id:int}/children")]
    public async Task<ActionResult<List<CostCodeDto>>> GetChildren(int id)
    {
        var codes = await _repository.GetChildrenAsync(id);
        var dtos = codes.Select(c => new CostCodeDto(
            c.Id, c.ParentId, c.Name, c.CmicCode,
            c.Labor, c.Qty, c.Materials, c.Other, c.TotalCost
        )).ToList();
        return Ok(dtos);
    }
}
