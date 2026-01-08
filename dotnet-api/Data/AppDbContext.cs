using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Models;

namespace FunctionExecutor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CostCode> CostCodes => Set<CostCode>();
    public DbSet<Workbook> Workbooks => Set<Workbook>();
    public DbSet<WorkbookCostCode> WorkbookCostCodes => Set<WorkbookCostCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CostCode configuration
        modelBuilder.Entity<CostCode>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CmicCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Labor)
                .HasPrecision(18, 4);

            entity.Property(e => e.Qty)
                .HasPrecision(18, 4);

            entity.Property(e => e.Materials)
                .HasPrecision(18, 4);

            entity.Property(e => e.Other)
                .HasPrecision(18, 4);

            entity.Property(e => e.TotalCost)
                .HasPrecision(18, 4);

            // Self-referencing relationship
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Workbook configuration
        modelBuilder.Entity<Workbook>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.TemplateFilePath)
                .IsRequired()
                .HasMaxLength(500);
        });

        // WorkbookCostCode configuration
        modelBuilder.Entity<WorkbookCostCode>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CmicCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Labor)
                .HasPrecision(18, 4);

            entity.Property(e => e.Qty)
                .HasPrecision(18, 4);

            entity.Property(e => e.Materials)
                .HasPrecision(18, 4);

            entity.Property(e => e.Other)
                .HasPrecision(18, 4);

            entity.Property(e => e.TotalCost)
                .HasPrecision(18, 4);

            // Relationships
            entity.HasOne(e => e.Workbook)
                .WithMany(e => e.WorkbookCostCodes)
                .HasForeignKey(e => e.WorkbookId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CostCode)
                .WithMany(e => e.WorkbookCostCodes)
                .HasForeignKey(e => e.CostCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for faster lookups
            entity.HasIndex(e => new { e.WorkbookId, e.CmicCode });
        });

        // Seed some test data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed CostCodes with a hierarchy and CMIC codes
        modelBuilder.Entity<CostCode>().HasData(
            // Root level
            new CostCode { Id = 1, ParentId = null, Name = "Labor", CmicCode = "LAB-000", Labor = 0, Qty = 0, Materials = 0, Other = 0, TotalCost = 0 },
            new CostCode { Id = 2, ParentId = null, Name = "Materials", CmicCode = "MAT-000", Labor = 0, Qty = 0, Materials = 0, Other = 0, TotalCost = 0 },

            // Labor children
            new CostCode { Id = 3, ParentId = 1, Name = "Direct Labor", CmicCode = "LAB-001", Labor = 5000, Qty = 1, Materials = 0, Other = 0, TotalCost = 5000 },
            new CostCode { Id = 4, ParentId = 1, Name = "Indirect Labor", CmicCode = "LAB-002", Labor = 2000, Qty = 1, Materials = 0, Other = 0, TotalCost = 2000 },

            // Direct Labor children
            new CostCode { Id = 5, ParentId = 3, Name = "Carpenter", CmicCode = "LAB-001-01", Labor = 2500, Qty = 1, Materials = 0, Other = 0, TotalCost = 2500 },
            new CostCode { Id = 6, ParentId = 3, Name = "Electrician", CmicCode = "LAB-001-02", Labor = 2500, Qty = 1, Materials = 0, Other = 0, TotalCost = 2500 },

            // Materials children
            new CostCode { Id = 7, ParentId = 2, Name = "Lumber", CmicCode = "MAT-001", Labor = 0, Qty = 100, Materials = 3000, Other = 0, TotalCost = 3000 },
            new CostCode { Id = 8, ParentId = 2, Name = "Electrical", CmicCode = "MAT-002", Labor = 0, Qty = 50, Materials = 1500, Other = 0, TotalCost = 1500 },
            new CostCode { Id = 9, ParentId = 2, Name = "Hardware", CmicCode = "MAT-003", Labor = 0, Qty = 200, Materials = 500, Other = 0, TotalCost = 500 }
        );
    }
}
