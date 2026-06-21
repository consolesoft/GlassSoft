using GlassSoft.Application.Interfaces;
using GlassSoft.Application.Services;
using GlassSoft.Application.Services.Accounting;
using GlassSoft.Application.Services.Product;
using GlassSoft.Application.Services.Purchasing;
using GlassSoft.Application.Services.Recipe;
using GlassSoft.Application.Services.Output;
using GlassSoft.Application.Services.Production;
using GlassSoft.Application.Services.Sales;
using GlassSoft.Domain.Entities.Identity;
using GlassSoft.Domain.Interfaces;
using GlassSoft.Infrastructure.Data;
using GlassSoft.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GlassSoft.Application.Licensing;
using GlassSoft.Infrastructure.Licensing;

namespace GlassSoft.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<LicenseOptions>(configuration.GetSection(LicenseOptions.SectionName));
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(12);
        });

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Application Services
        services.AddScoped<IProductGroupService, ProductGroupService>();
        services.AddScoped<IProductItemService, ProductItemService>();
        services.AddScoped<IGlassPlateDefinitionService, GlassPlateDefinitionService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDeliveryService, DeliveryService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IAccountTransactionService, AccountTransactionService>();
        services.AddScoped<IChequeNoteService, ChequeNoteService>();
        services.AddScoped<ICurrencyRateService, CurrencyRateService>();
        services.AddScoped<IPrintTemplateService, PrintTemplateService>();
        services.AddScoped<IOutputRenderService, OutputRenderService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<ICashTransactionService, CashTransactionService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ILicenseService, FileLicenseService>();

        return services;
    }
}
