namespace FunctionExecutor.Models.Dto;

// Request DTOs
public record CreateWorkbookRequest(
    string Name,
    string? Description,
    string TemplateFilePath
);

public record UpdateWorkbookRequest(
    string Name,
    string? Description,
    string TemplateFilePath
);

public record CostCodeUpdateInput(
    string CmicCode,
    decimal? Labor,
    decimal? Qty,
    decimal? Materials,
    decimal? Other
);

public record CalculationRequest(
    List<CostCodeUpdateInput> CostCodeInputs
);

// Response DTOs
public record CostCodeDto(
    int Id,
    int? ParentId,
    string Name,
    string CmicCode,
    decimal Labor,
    decimal Qty,
    decimal Materials,
    decimal Other,
    decimal TotalCost
);

public record WorkbookCostCodeDto(
    int Id,
    int WorkbookId,
    int CostCodeId,
    string CmicCode,
    string Name,
    decimal Labor,
    decimal Qty,
    decimal Materials,
    decimal Other,
    decimal TotalCost
);

public record WorkbookDto(
    int Id,
    string Name,
    string? Description,
    string TemplateFilePath,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<WorkbookCostCodeDto> CostCodes
);

public record WorkbookSummaryDto(
    int Id,
    string Name,
    string? Description,
    string TemplateFilePath,
    int Version,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CostCodeCount
);

public record CalculationResultDto(
    bool Success,
    string? Error,
    long ExecutionTimeMs,
    List<WorkbookCostCodeDto> UpdatedCostCodes
);
