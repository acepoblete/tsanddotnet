using Microsoft.EntityFrameworkCore;
using FunctionExecutor.Models;

namespace FunctionExecutor.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<CostCode> CostCodes => Set<CostCode>();
    public DbSet<FunctionWrapper> FunctionWrappers => Set<FunctionWrapper>();
    
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
            
            entity.Property(e => e.Value)
                .HasPrecision(18, 4);
            
            // Self-referencing relationship
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // FunctionWrapper configuration
        modelBuilder.Entity<FunctionWrapper>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.TheFunction)
                .IsRequired();
            
            // Many-to-many with CostCode (EF Core 5+ implicit join table)
            entity.HasMany(e => e.CostCodes)
                .WithMany(e => e.FunctionWrappers)
                .UsingEntity<Dictionary<string, object>>(
                    "FunctionWrapperCostCode",
                    j => j.HasOne<CostCode>()
                        .WithMany()
                        .HasForeignKey("CostCodeId")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j.HasOne<FunctionWrapper>()
                        .WithMany()
                        .HasForeignKey("FunctionWrapperId")
                        .OnDelete(DeleteBehavior.Cascade)
                );
        });
        
        // Seed some test data
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed CostCodes with a hierarchy
        modelBuilder.Entity<CostCode>().HasData(
            // Root level
            new CostCode { Id = 1, ParentId = null, Name = "Labor", Value = 0 },
            new CostCode { Id = 2, ParentId = null, Name = "Materials", Value = 0 },
            
            // Labor children
            new CostCode { Id = 3, ParentId = 1, Name = "Direct Labor", Value = 5000 },
            new CostCode { Id = 4, ParentId = 1, Name = "Indirect Labor", Value = 2000 },
            
            // Direct Labor children
            new CostCode { Id = 5, ParentId = 3, Name = "Carpenter", Value = 2500 },
            new CostCode { Id = 6, ParentId = 3, Name = "Electrician", Value = 2500 },
            
            // Materials children
            new CostCode { Id = 7, ParentId = 2, Name = "Lumber", Value = 3000 },
            new CostCode { Id = 8, ParentId = 2, Name = "Electrical", Value = 1500 },
            new CostCode { Id = 9, ParentId = 2, Name = "Hardware", Value = 500 }
        );
        
        // Seed sample FunctionWrappers
        modelBuilder.Entity<FunctionWrapper>().HasData(
            new FunctionWrapper
            {
                Id = 1,
                Version = 1,
                Name = "Sum All Labor Costs",
                Description = "Sums all labor-related cost codes using getDescendants",
                TheFunction = @"
// Get the Labor root node and all its descendants
const labor = getCostCode(1);
const allLaborCodes = getDescendants(1);

// Sum the values of all descendants
let total = labor.value;
for (const code of allLaborCodes) {
    total += code.value;
}
return total;
",
                CreatedAt = DateTime.Parse("2025-01-01T00:00:00Z"),
                UpdatedAt = DateTime.Parse("2025-01-01T00:00:00Z")
            },
            new FunctionWrapper
            {
                Id = 2,
                Version = 1,
                Name = "Total Project Cost",
                Description = "Sums all cost codes in the entire hierarchy",
                TheFunction = @"
// Get all cost codes and sum their values
const allCodes = getCostCodes();
let total = 0;
for (const code of allCodes) {
    total += code.value;
}
return total;
",
                CreatedAt = DateTime.Parse("2025-01-01T00:00:00Z"),
                UpdatedAt = DateTime.Parse("2025-01-01T00:00:00Z")
            }
        );

        // Seed the many-to-many join table for FunctionWrapper <-> CostCode
        modelBuilder.Entity("FunctionWrapperCostCode").HasData(
            // Function 1 (Sum All Labor Costs) - Labor hierarchy (IDs 1, 3, 4, 5, 6)
            new { FunctionWrapperId = 1, CostCodeId = 1 },
            new { FunctionWrapperId = 1, CostCodeId = 3 },
            new { FunctionWrapperId = 1, CostCodeId = 4 },
            new { FunctionWrapperId = 1, CostCodeId = 5 },
            new { FunctionWrapperId = 1, CostCodeId = 6 },
            // Function 2 (Total Project Cost) - All cost codes
            new { FunctionWrapperId = 2, CostCodeId = 1 },
            new { FunctionWrapperId = 2, CostCodeId = 2 },
            new { FunctionWrapperId = 2, CostCodeId = 3 },
            new { FunctionWrapperId = 2, CostCodeId = 4 },
            new { FunctionWrapperId = 2, CostCodeId = 5 },
            new { FunctionWrapperId = 2, CostCodeId = 6 },
            new { FunctionWrapperId = 2, CostCodeId = 7 },
            new { FunctionWrapperId = 2, CostCodeId = 8 },
            new { FunctionWrapperId = 2, CostCodeId = 9 }
        );
    }
}
