using GlassSoft.Domain.Common;
using GlassSoft.Domain.Entities;
using GlassSoft.Domain.Entities.Accounting;
using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Domain.Entities.Output;
using GlassSoft.Domain.Entities.Product;
using GlassSoft.Domain.Entities.Production;
using GlassSoft.Domain.Entities.Purchasing;
using GlassSoft.Domain.Entities.Sales;
using GlassSoft.Domain.Entities.Recipe;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlassSoft.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Product & Stock
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<ProductItem> ProductItems => Set<ProductItem>();
    public DbSet<ProductFeatureDefinition> ProductFeatureDefinitions => Set<ProductFeatureDefinition>();
    public DbSet<ProductFeatureOption> ProductFeatureOptions => Set<ProductFeatureOption>();
    public DbSet<GlassPlateDefinition> GlassPlateDefinitions => Set<GlassPlateDefinition>();
    public DbSet<StockEntry> StockEntries => Set<StockEntry>();

    // Recipe
    public DbSet<Domain.Entities.Recipe.Recipe> Recipes => Set<Domain.Entities.Recipe.Recipe>();
    public DbSet<RecipeLayer> RecipeLayers => Set<RecipeLayer>();
    public DbSet<RecipeConsumable> RecipeConsumables => Set<RecipeConsumable>();

    // Sales
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderLineFeature> OrderLineFeatures => Set<OrderLineFeature>();
    public DbSet<OrderFeatureDefinition> OrderFeatureDefinitions => Set<OrderFeatureDefinition>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryLine> DeliveryLines => Set<DeliveryLine>();

    // Production
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderLine> WorkOrderLines => Set<WorkOrderLine>();
    public DbSet<CuttingPlan> CuttingPlans => Set<CuttingPlan>();
    public DbSet<CuttingPlanItem> CuttingPlanItems => Set<CuttingPlanItem>();
    public DbSet<WorkOrderOrder> WorkOrderOrders => Set<WorkOrderOrder>();

    // Purchasing
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    // Accounting
    public DbSet<AccountTransaction> AccountTransactions => Set<AccountTransaction>();
    public DbSet<ChequeNote> ChequeNotes => Set<ChequeNote>();
    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashTransaction> CashTransactions => Set<CashTransaction>();

    // Output
    public DbSet<PrintTemplate> PrintTemplates => Set<PrintTemplate>();

    // System
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // Identity extensions
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Identity table names
        builder.Entity<ApplicationUser>(b => b.ToTable("Users"));
        builder.Entity<ApplicationRole>(b => b.ToTable("Roles"));

        // RolePermission composite key
        builder.Entity<RolePermission>(b =>
        {
            b.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            b.HasOne(rp => rp.Role).WithMany().HasForeignKey(rp => rp.RoleId);
            b.HasOne(rp => rp.Permission).WithMany(p => p.RolePermissions).HasForeignKey(rp => rp.PermissionId);
        });

        // FK cascade döngü problemlerini önlemek için Restrict kuralları
        // ProductItem çok referans ediliyor - cascade yerine restrict kullanılıyor
        builder.Entity<CuttingPlan>(b =>
        {
            b.HasOne(cp => cp.ProductItem).WithMany().HasForeignKey(cp => cp.ProductItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(cp => cp.GlassPlateDefinition).WithMany().HasForeignKey(cp => cp.GlassPlateDefinitionId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorkOrderLine>(b =>
        {
            b.HasOne(wl => wl.ProductItem).WithMany().HasForeignKey(wl => wl.ProductItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(wl => wl.OrderLine).WithMany().HasForeignKey(wl => wl.OrderLineId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorkOrderOrder>(b =>
        {
            b.HasOne(wo => wo.Order).WithMany().HasForeignKey(wo => wo.OrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(wo => wo.WorkOrder).WithMany(w => w.Orders).HasForeignKey(wo => wo.WorkOrderId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<WorkOrder>(b =>
        {
            b.HasOne(w => w.Order).WithMany().HasForeignKey(w => w.OrderId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CuttingPlanItem>(b =>
        {
            b.HasOne(ci => ci.CuttingPlan).WithMany(cp => cp.Items).HasForeignKey(ci => ci.CuttingPlanId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(ci => ci.OrderLine).WithMany().HasForeignKey(ci => ci.OrderLineId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderLine>(b =>
        {
            b.HasOne(ol => ol.ProductItem).WithMany().HasForeignKey(ol => ol.ProductItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(ol => ol.Recipe).WithMany().HasForeignKey(ol => ol.RecipeId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Delivery>(b =>
        {
            b.HasOne(d => d.Order).WithMany().HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<DeliveryLine>(b =>
        {
            b.HasOne(dl => dl.Delivery).WithMany(d => d.Lines).HasForeignKey(dl => dl.DeliveryId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(dl => dl.OrderLine).WithMany().HasForeignKey(dl => dl.OrderLineId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderLineFeature>(b =>
        {
            b.HasOne(f => f.FeatureDefinition).WithMany().HasForeignKey(f => f.FeatureDefinitionId).OnDelete(DeleteBehavior.Restrict);
            b.Property(f => f.UnitPrice).HasPrecision(18, 4);
        });

        builder.Entity<OrderFeatureDefinition>(b =>
        {
            b.Property(f => f.UnitPrice).HasPrecision(18, 4);
        });

        builder.Entity<RecipeLayer>(b =>
        {
            b.HasOne(rl => rl.ProductItem).WithMany().HasForeignKey(rl => rl.ProductItemId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RecipeConsumable>(b =>
        {
            b.HasOne(rc => rc.ProductItem).WithMany().HasForeignKey(rc => rc.ProductItemId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseOrderLine>(b =>
        {
            b.HasOne(pl => pl.ProductItem).WithMany().HasForeignKey(pl => pl.ProductItemId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StockEntry>(b =>
        {
            b.HasOne(se => se.GlassPlateDefinition).WithMany().HasForeignKey(se => se.GlassPlateDefinitionId).OnDelete(DeleteBehavior.Restrict);
        });

        // Decimal precisions
        builder.Entity<ProductItem>(b =>
        {
            b.Property(p => p.UnitPrice).HasPrecision(18, 4);
            b.Property(p => p.ThicknessMm).HasPrecision(10, 2);
        });

        builder.Entity<ProductFeatureOption>(b =>
        {
            b.Property(p => p.AdditionalPrice).HasPrecision(18, 4);
        });

        builder.Entity<GlassPlateDefinition>(b =>
        {
            b.Property(p => p.WidthMm).HasPrecision(10, 2);
            b.Property(p => p.HeightMm).HasPrecision(10, 2);
        });

        builder.Entity<StockEntry>(b =>
        {
            b.Property(p => p.Quantity).HasPrecision(18, 4);
        });

        builder.Entity<Domain.Entities.Recipe.Recipe>(b =>
        {
            b.Property(p => p.BaseUnitPrice).HasPrecision(18, 4);
        });

        builder.Entity<RecipeLayer>(b =>
        {
            b.Property(p => p.ThicknessMm).HasPrecision(10, 2);
        });

        builder.Entity<OrderLine>(b =>
        {
            b.Property(p => p.WidthMm).HasPrecision(10, 2);
            b.Property(p => p.HeightMm).HasPrecision(10, 2);
            b.Property(p => p.UnitPrice).HasPrecision(18, 4);
            b.Property(p => p.TotalPrice).HasPrecision(18, 4);
        });

        builder.Entity<Order>(b =>
        {
            b.Property(p => p.TotalAmount).HasPrecision(18, 4);
        });

        builder.Entity<WorkOrderLine>(b =>
        {
            b.Property(p => p.WidthMm).HasPrecision(10, 2);
            b.Property(p => p.HeightMm).HasPrecision(10, 2);
        });

        builder.Entity<CuttingPlan>(b =>
        {
            b.Property(p => p.WastePercentage).HasPrecision(5, 2);
            b.Property(p => p.UsedAreaM2).HasPrecision(18, 4);
            b.Property(p => p.WasteAreaM2).HasPrecision(18, 4);
        });

        builder.Entity<CuttingPlanItem>(b =>
        {
            b.Property(p => p.X).HasPrecision(10, 2);
            b.Property(p => p.Y).HasPrecision(10, 2);
            b.Property(p => p.WidthMm).HasPrecision(10, 2);
            b.Property(p => p.HeightMm).HasPrecision(10, 2);
        });

        builder.Entity<PurchaseOrder>(b =>
        {
            b.Property(p => p.TotalAmount).HasPrecision(18, 4);
            b.HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(p => p.Supplier).WithMany(s => s.PurchaseOrders).HasForeignKey(p => p.SupplierId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PurchaseOrderLine>(b =>
        {
            b.Property(p => p.Quantity).HasPrecision(18, 4);
            b.Property(p => p.UnitPrice).HasPrecision(18, 4);
            b.Property(p => p.TotalPrice).HasPrecision(18, 4);
            b.Property(p => p.ReceivedQuantity).HasPrecision(18, 4);
        });

        builder.Entity<GoodsReceiptLine>(b =>
        {
            b.Property(p => p.ReceivedQuantity).HasPrecision(18, 4);
        });

        builder.Entity<AccountTransaction>(b =>
        {
            b.Property(p => p.Amount).HasPrecision(18, 4);
            b.Property(p => p.ExchangeRate).HasPrecision(18, 6);
            b.Property(p => p.AmountTRY).HasPrecision(18, 4);
        });

        builder.Entity<ChequeNote>(b =>
        {
            b.Property(p => p.Amount).HasPrecision(18, 4);
        });

        builder.Entity<CurrencyRate>(b =>
        {
            b.Property(p => p.BuyRate).HasPrecision(18, 6);
            b.Property(p => p.SellRate).HasPrecision(18, 6);
        });

        builder.Entity<PrintTemplate>(b =>
        {
            b.Property(p => p.HtmlContent).HasColumnType("nvarchar(max)");
        });

        builder.Entity<SystemSetting>(b =>
        {
            b.HasIndex(s => s.Key).IsUnique();
            b.Property(s => s.Key).HasMaxLength(100);
            b.Property(s => s.Value).HasMaxLength(500);
        });

        builder.Entity<Expense>(b =>
        {
            b.Property(p => p.Amount).HasPrecision(18, 4);
            b.Property(p => p.ExchangeRate).HasPrecision(18, 6);
            b.Property(p => p.AmountTRY).HasPrecision(18, 4);
        });

        builder.Entity<CashRegister>(b =>
        {
            b.Property(p => p.Balance).HasPrecision(18, 4);
        });

        builder.Entity<CashTransaction>(b =>
        {
            b.Property(p => p.Amount).HasPrecision(18, 4);
        });

        // Global: Tüm FK'larda cascade delete yerine restrict kullan
        // Soft delete kullandığımız için cascade'e gerek yok, SQL Server döngü hatalarını da önler
        foreach (var relationship in builder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Global query filter for soft delete
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [builder]);
            }
        }
    }

    private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : BaseEntity
    {
        builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = TurkeyTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = TurkeyTime.Now;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = TurkeyTime.Now;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
