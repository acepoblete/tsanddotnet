using Microsoft.AspNetCore.Mvc;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Services;

namespace FunctionExecutor.Controllers;

[ApiController]
[Route("api/workbooks")]
public class WorkbookController : ControllerBase
{
    private readonly IWorkbookService _workbookService;
    private readonly ILogger<WorkbookController> _logger;

    public WorkbookController(IWorkbookService workbookService, ILogger<WorkbookController> logger)
    {
        _workbookService = workbookService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkbookSummaryDto>>> GetAll()
    {
        var workbooks = await _workbookService.GetAllAsync();
        return Ok(workbooks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkbookDto>> GetById(int id)
    {
        var workbook = await _workbookService.GetByIdAsync(id);
        if (workbook == null)
            return NotFound();

        return Ok(workbook);
    }

    [HttpPost]
    public async Task<ActionResult<WorkbookDto>> Create([FromBody] CreateWorkbookRequest request)
    {
        var workbook = await _workbookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = workbook.Id }, workbook);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkbookDto>> Update(int id, [FromBody] UpdateWorkbookRequest request)
    {
        var workbook = await _workbookService.UpdateAsync(id, request);
        if (workbook == null)
            return NotFound();

        return Ok(workbook);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _workbookService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/cost-codes")]
    public async Task<ActionResult<WorkbookDto>> UpdateCostCodes(
        int id,
        [FromBody] IEnumerable<CostCodeUpdateInput> updates)
    {
        var workbook = await _workbookService.UpdateCostCodesAsync(id, updates);
        if (workbook == null)
            return NotFound();

        return Ok(workbook);
    }

    [HttpPost("{id}/calculate")]
    public async Task<ActionResult<CalculationResultDto>> ExecuteCalculation(
        int id,
        [FromBody] CalculationRequest? request = null)
    {
        var result = await _workbookService.ExecuteCalculationAsync(id, request);

        if (!result.Success)
        {
            _logger.LogWarning("Calculation failed for workbook {WorkbookId}: {Error}", id, result.Error);
        }

        return Ok(result);
    }
}
