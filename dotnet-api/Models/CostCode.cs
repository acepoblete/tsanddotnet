namespace FunctionExecutor.Models;

public class CostCode
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    
    // Navigation properties
    public CostCode? Parent { get; set; }
    public ICollection<CostCode> Children { get; set; } = new List<CostCode>();
    public ICollection<FunctionWrapper> FunctionWrappers { get; set; } = new List<FunctionWrapper>();
}
