using FunctionExecutor.Models;
using FunctionExecutor.Models.Dto;
using FunctionExecutor.Repositories;
using Microsoft.Extensions.Logging;

namespace FunctionExecutor.Services;

public class WorkbookService : IWorkbookService
{
    private readonly IWorkbookRepository _workbookRepository;
    private readonly ICostCodeRepository _costCodeRepository;
    private readonly IExcelCalculationService _excelCalculationService;
    private readonly ILogger<WorkbookService> _logger;

    public WorkbookService(
        IWorkbookRepository workbookRepository,
        ICostCodeRepository costCodeRepository,
        IExcelCalculationService excelCalculationService,
        ILogger<WorkbookService> logger)
    {
        _workbookRepository = workbookRepository;
        _costCodeRepository = costCodeRepository;
        _excelCalculationService = excelCalculationService;
        _logger = logger;
    }

    public async Task<WorkbookDto?> GetByIdAsync(int id)
    {
        var workbook = await _workbookRepository.GetByIdWithCostCodesAsync(id);
        return workbook == null ? null : MapToDto(workbook);
    }

    public async Task<IEnumerable<WorkbookSummaryDto>> GetAllAsync()
    {
        var workbooks = await _workbookRepository.GetAllAsync();
        return workbooks.Select(w => new WorkbookSummaryDto(
            Id: w.Id,
            Name: w.Name,
            Description: w.Description,
            TemplateFilePath: w.TemplateFilePath,
            Version: w.Version,
            CreatedAt: w.CreatedAt,
            UpdatedAt: w.UpdatedAt,
            CostCodeCount: w.WorkbookCostCodes.Count
        ));
    }

    public async Task<WorkbookDto> CreateAsync(CreateWorkbookRequest request)
    {
        // Create the workbook
        var workbook = new Workbook
        {
            Name = request.Name,
            Description = request.Description,
            TemplateFilePath = request.TemplateFilePath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Version = 1
        };

        await _workbookRepository.CreateAsync(workbook);

        // "Prime the pump" - copy all active cost codes to the workbook
        var allCostCodes = await _costCodeRepository.GetAllAsync();
        var workbookCostCodes = allCostCodes.Select(cc => new WorkbookCostCode
        {
            WorkbookId = workbook.Id,
            CostCodeId = cc.Id,
            CmicCode = cc.CmicCode,
            Name = cc.Name,
            Labor = cc.Labor,
            Qty = cc.Qty,
            Materials = cc.Materials,
            Other = cc.Other,
            TotalCost = cc.TotalCost
        }).ToList();

        await _workbookRepository.AddCostCodesAsync(workbookCostCodes);

        // Reload to get the cost codes
        var createdWorkbook = await _workbookRepository.GetByIdWithCostCodesAsync(workbook.Id);
        return MapToDto(createdWorkbook!);
    }

    public async Task<WorkbookDto?> UpdateAsync(int id, UpdateWorkbookRequest request)
    {
        var workbook = await _workbookRepository.GetByIdAsync(id);
        if (workbook == null)
            return null;

        workbook.Name = request.Name;
        workbook.Description = request.Description;
        workbook.TemplateFilePath = request.TemplateFilePath;
        workbook.UpdatedAt = DateTime.UtcNow;
        workbook.Version++;

        await _workbookRepository.UpdateAsync(workbook);

        var updatedWorkbook = await _workbookRepository.GetByIdWithCostCodesAsync(id);
        return MapToDto(updatedWorkbook!);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var workbook = await _workbookRepository.GetByIdAsync(id);
        if (workbook == null)
            return false;

        await _workbookRepository.DeleteAsync(id);
        return true;
    }

    public async Task<WorkbookDto?> UpdateCostCodesAsync(int workbookId, IEnumerable<CostCodeUpdateInput> updates)
    {
        var costCodes = (await _workbookRepository.GetCostCodesByWorkbookIdAsync(workbookId)).ToList();
        if (!costCodes.Any())
            return null;

        var updateLookup = updates.ToDictionary(u => u.CmicCode, u => u);
        var modified = new List<WorkbookCostCode>();

        foreach (var costCode in costCodes)
        {
            if (updateLookup.TryGetValue(costCode.CmicCode, out var update))
            {
                if (update.Labor.HasValue) costCode.Labor = update.Labor.Value;
                if (update.Qty.HasValue) costCode.Qty = update.Qty.Value;
                if (update.Materials.HasValue) costCode.Materials = update.Materials.Value;
                if (update.Other.HasValue) costCode.Other = update.Other.Value;
                modified.Add(costCode);
            }
        }

        if (modified.Any())
        {
            await _workbookRepository.UpdateCostCodesAsync(modified);
        }

        // Update workbook timestamp
        var workbook = await _workbookRepository.GetByIdAsync(workbookId);
        if (workbook != null)
        {
            workbook.UpdatedAt = DateTime.UtcNow;
            await _workbookRepository.UpdateAsync(workbook);
        }

        return await GetByIdAsync(workbookId);
    }

    public async Task<CalculationResultDto> ExecuteCalculationAsync(int workbookId, CalculationRequest? request = null)
    {
        var workbook = await _workbookRepository.GetByIdWithCostCodesAsync(workbookId);
        if (workbook == null)
        {
            return new CalculationResultDto(
                Success: false,
                Error: $"Workbook with ID {workbookId} not found",
                ExecutionTimeMs: 0,
                UpdatedCostCodes: new List<WorkbookCostCodeDto>()
            );
        }

        // Execute Excel calculation
        var result = await _excelCalculationService.ExecuteAsync(
            workbook.TemplateFilePath,
            workbook.WorkbookCostCodes,
            request?.CostCodeInputs
        );

        if (result.Success)
        {
            // Update database with calculated values
            var costCodeLookup = workbook.WorkbookCostCodes.ToDictionary(cc => cc.CmicCode, cc => cc);
            var modified = new List<WorkbookCostCode>();

            foreach (var updatedCostCode in result.UpdatedCostCodes)
            {
                if (costCodeLookup.TryGetValue(updatedCostCode.CmicCode, out var costCode))
                {
                    costCode.Labor = updatedCostCode.Labor;
                    costCode.Qty = updatedCostCode.Qty;
                    costCode.Materials = updatedCostCode.Materials;
                    costCode.Other = updatedCostCode.Other;
                    costCode.TotalCost = updatedCostCode.TotalCost;
                    modified.Add(costCode);
                }
            }

            if (modified.Any())
            {
                await _workbookRepository.UpdateCostCodesAsync(modified);
            }

            // Update workbook timestamp
            workbook.UpdatedAt = DateTime.UtcNow;
            await _workbookRepository.UpdateAsync(workbook);

            _logger.LogInformation(
                "Calculation executed for workbook {WorkbookId}, updated {Count} cost codes",
                workbookId, modified.Count);
        }

        return result;
    }

    private static WorkbookDto MapToDto(Workbook workbook)
    {
        return new WorkbookDto(
            Id: workbook.Id,
            Name: workbook.Name,
            Description: workbook.Description,
            TemplateFilePath: workbook.TemplateFilePath,
            Version: workbook.Version,
            CreatedAt: workbook.CreatedAt,
            UpdatedAt: workbook.UpdatedAt,
            CostCodes: workbook.WorkbookCostCodes.Select(cc => new WorkbookCostCodeDto(
                Id: cc.Id,
                WorkbookId: cc.WorkbookId,
                CostCodeId: cc.CostCodeId,
                CmicCode: cc.CmicCode,
                Name: cc.Name,
                Labor: cc.Labor,
                Qty: cc.Qty,
                Materials: cc.Materials,
                Other: cc.Other,
                TotalCost: cc.TotalCost
            )).ToList()
        );
    }
}
