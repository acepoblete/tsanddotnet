using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FunctionExecutor.Migrations
{
    /// <inheritdoc />
    public partial class SeedWorkbookData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CmicCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Labor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Qty = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Materials = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Other = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostCodes_CostCodes_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CostCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workbooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    TemplateFilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workbooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkbookCostCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkbookId = table.Column<int>(type: "INTEGER", nullable: false),
                    CostCodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    CmicCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Labor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Qty = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Materials = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Other = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    TotalCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkbookCostCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkbookCostCodes_CostCodes_CostCodeId",
                        column: x => x.CostCodeId,
                        principalTable: "CostCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkbookCostCodes_Workbooks_WorkbookId",
                        column: x => x.WorkbookId,
                        principalTable: "Workbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CostCodes",
                columns: new[] { "Id", "CmicCode", "Labor", "Materials", "Name", "Other", "ParentId", "Qty", "TotalCost" },
                values: new object[,]
                {
                    { 1, "LAB-000", 0m, 0m, "Labor", 0m, null, 0m, 0m },
                    { 2, "MAT-000", 0m, 0m, "Materials", 0m, null, 0m, 0m }
                });

            migrationBuilder.InsertData(
                table: "Workbooks",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "TemplateFilePath", "UpdatedAt", "Version" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Test workbook with sample cost codes for Excel calculation", "Sample Construction Project", "sample-calculation.xlsx", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1 });

            migrationBuilder.InsertData(
                table: "CostCodes",
                columns: new[] { "Id", "CmicCode", "Labor", "Materials", "Name", "Other", "ParentId", "Qty", "TotalCost" },
                values: new object[,]
                {
                    { 3, "LAB-001", 5000m, 0m, "Direct Labor", 0m, 1, 1m, 5000m },
                    { 4, "LAB-002", 2000m, 0m, "Indirect Labor", 0m, 1, 1m, 2000m },
                    { 7, "MAT-001", 0m, 3000m, "Lumber", 0m, 2, 100m, 3000m },
                    { 8, "MAT-002", 0m, 1500m, "Electrical", 0m, 2, 50m, 1500m },
                    { 9, "MAT-003", 0m, 500m, "Hardware", 0m, 2, 200m, 500m }
                });

            migrationBuilder.InsertData(
                table: "WorkbookCostCodes",
                columns: new[] { "Id", "CmicCode", "CostCodeId", "Labor", "Materials", "Name", "Other", "Qty", "TotalCost", "WorkbookId" },
                values: new object[,]
                {
                    { 1, "LAB-000", 1, 0m, 0m, "Labor", 0m, 0m, 0m, 1 },
                    { 2, "MAT-000", 2, 0m, 0m, "Materials", 0m, 0m, 0m, 1 }
                });

            migrationBuilder.InsertData(
                table: "CostCodes",
                columns: new[] { "Id", "CmicCode", "Labor", "Materials", "Name", "Other", "ParentId", "Qty", "TotalCost" },
                values: new object[,]
                {
                    { 5, "LAB-001-01", 2500m, 0m, "Carpenter", 0m, 3, 1m, 2500m },
                    { 6, "LAB-001-02", 2500m, 0m, "Electrician", 0m, 3, 1m, 2500m }
                });

            migrationBuilder.InsertData(
                table: "WorkbookCostCodes",
                columns: new[] { "Id", "CmicCode", "CostCodeId", "Labor", "Materials", "Name", "Other", "Qty", "TotalCost", "WorkbookId" },
                values: new object[,]
                {
                    { 3, "LAB-001", 3, 5000m, 0m, "Direct Labor", 0m, 1m, 5000m, 1 },
                    { 4, "LAB-002", 4, 2000m, 0m, "Indirect Labor", 0m, 1m, 2000m, 1 },
                    { 7, "MAT-001", 7, 0m, 3000m, "Lumber", 0m, 100m, 3000m, 1 },
                    { 8, "MAT-002", 8, 0m, 1500m, "Electrical", 0m, 50m, 1500m, 1 },
                    { 9, "MAT-003", 9, 0m, 500m, "Hardware", 0m, 200m, 500m, 1 },
                    { 5, "LAB-001-01", 5, 2500m, 0m, "Carpenter", 0m, 1m, 2500m, 1 },
                    { 6, "LAB-001-02", 6, 2500m, 0m, "Electrician", 0m, 1m, 2500m, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostCodes_ParentId",
                table: "CostCodes",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkbookCostCodes_CostCodeId",
                table: "WorkbookCostCodes",
                column: "CostCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkbookCostCodes_WorkbookId_CmicCode",
                table: "WorkbookCostCodes",
                columns: new[] { "WorkbookId", "CmicCode" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkbookCostCodes");

            migrationBuilder.DropTable(
                name: "CostCodes");

            migrationBuilder.DropTable(
                name: "Workbooks");
        }
    }
}
