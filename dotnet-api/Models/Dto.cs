namespace FunctionExecutor.Models.Dto;

// Request DTOs
public record CreateFunctionWrapperRequest(
    string Name,
    string? Description,
    string TheFunction,
    List<int> SelectedCostCodeIds  // Root IDs - descendants auto-loaded
);

public record UpdateFunctionWrapperRequest(
    string Name,
    string? Description,
    string TheFunction,
    List<int> SelectedCostCodeIds
);

public record ExecuteRequest(
    int FunctionWrapperId
);

// Response DTOs
public record CostCodeDto(
    int Id,
    int? ParentId,
    string Name,
    decimal Value
);

public record FunctionWrapperDto(
    int Id,
    int Version,
    string Name,
    string? Description,
    string TheFunction,
    List<CostCodeDto> CostCodes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ExecutionResultDto(
    bool Success,
    decimal? Result,
    string? Error,
    long ExecutionTimeMs
);
