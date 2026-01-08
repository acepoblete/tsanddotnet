namespace FunctionExecutor.Models;

public class WorkbookCostCode
{
    public int Id { get; set; }
    public int WorkbookId { get; set; }
    public int CostCodeId { get; set; }
    public string CmicCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Labor { get; set; }
    public decimal Qty { get; set; }
    public decimal Materials { get; set; }
    public decimal Other { get; set; }
    public decimal TotalCost { get; set; }

    // Navigation properties
    public Workbook Workbook { get; set; } = null!;
    public CostCode CostCode { get; set; } = null!;
}
