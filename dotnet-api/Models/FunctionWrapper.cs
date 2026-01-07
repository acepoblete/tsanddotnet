namespace FunctionExecutor.Models;

public class FunctionWrapper
{
    public int Id { get; set; }
    public int Version { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TheFunction { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property - many-to-many with CostCode
    public ICollection<CostCode> CostCodes { get; set; } = new List<CostCode>();
}
