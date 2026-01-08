using ClosedXML.Excel;

// Get the Templates folder path (one directory up from Tools)
var toolsDir = AppContext.BaseDirectory;
var projectRoot = Path.GetFullPath(Path.Combine(toolsDir, "..", "..", "..", ".."));
var templatesPath = Path.Combine(projectRoot, "Templates");

Console.WriteLine($"Creating template in: {templatesPath}");
Directory.CreateDirectory(templatesPath);

var filePath = Path.Combine(templatesPath, "sample-calculation.xlsx");

using var workbook = new XLWorkbook();
var worksheet = workbook.Worksheets.Add("CostCodes");

// Headers (Row 1)
worksheet.Cell(1, 1).Value = "CMIC Code";
worksheet.Cell(1, 2).Value = "Name";
worksheet.Cell(1, 3).Value = "Labor";
worksheet.Cell(1, 4).Value = "Qty";
worksheet.Cell(1, 5).Value = "Materials";
worksheet.Cell(1, 6).Value = "Other";
worksheet.Cell(1, 7).Value = "TotalCost";

// Style headers
var headerRange = worksheet.Range(1, 1, 1, 7);
headerRange.Style.Font.Bold = true;
headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

// Sample data matching the seeded cost codes
var costCodes = new (string CmicCode, string Name, decimal Labor, decimal Qty, decimal Materials, decimal Other)[]
{
    ("LAB-000", "Labor", 0, 0, 0, 0),
    ("MAT-000", "Materials", 0, 0, 0, 0),
    ("LAB-001", "Direct Labor", 5000, 1, 0, 0),
    ("LAB-002", "Indirect Labor", 2000, 1, 0, 0),
    ("LAB-001-01", "Carpenter", 2500, 1, 0, 0),
    ("LAB-001-02", "Electrician", 2500, 1, 0, 0),
    ("MAT-001", "Lumber", 0, 100, 3000, 0),
    ("MAT-002", "Electrical", 0, 50, 1500, 0),
    ("MAT-003", "Hardware", 0, 200, 500, 0),
};

for (int i = 0; i < costCodes.Length; i++)
{
    var row = i + 2; // Start from row 2
    var cc = costCodes[i];

    worksheet.Cell(row, 1).Value = cc.CmicCode;
    worksheet.Cell(row, 2).Value = cc.Name;
    worksheet.Cell(row, 3).Value = (double)cc.Labor;
    worksheet.Cell(row, 4).Value = (double)cc.Qty;
    worksheet.Cell(row, 5).Value = (double)cc.Materials;
    worksheet.Cell(row, 6).Value = (double)cc.Other;

    // TotalCost formula: Labor + (Qty * Materials) + Other
    worksheet.Cell(row, 7).FormulaA1 = $"C{row}+(D{row}*E{row})+F{row}";
}

// Add a summary row
var summaryRow = costCodes.Length + 3;
worksheet.Cell(summaryRow, 1).Value = "TOTAL";
worksheet.Cell(summaryRow, 2).Value = "Grand Total";
worksheet.Cell(summaryRow, 7).FormulaA1 = $"SUM(G2:G{costCodes.Length + 1})";
worksheet.Cell(summaryRow, 7).Style.Font.Bold = true;

// Auto-fit columns
worksheet.Columns().AdjustToContents();

workbook.SaveAs(filePath);

Console.WriteLine($"Created sample template: {filePath}");
