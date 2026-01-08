namespace FunctionExecutor.Models;

public class Workbook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TemplateFilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    // Navigation properties
    public ICollection<WorkbookCostCode> WorkbookCostCodes { get; set; } = new List<WorkbookCostCode>();
}
