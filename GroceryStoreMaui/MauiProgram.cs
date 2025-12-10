using GroceryStoreMaui.Services;
using GroceryStoreMaui.Pages;

namespace GroceryStoreMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Services
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ProductService>();
        builder.Services.AddSingleton<SalesService>();
        builder.Services.AddSingleton<PurchaseService>();
        builder.Services.AddSingleton<FinanceService>();

        // Pages
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<ProductsPage>();
        builder.Services.AddTransient<ProductEditPage>();
        builder.Services.AddSingleton<CustomersPage>();
        builder.Services.AddSingleton<SuppliersPage>();
        builder.Services.AddSingleton<PurchasesPage>();
        builder.Services.AddSingleton<SalesPage>();
        builder.Services.AddSingleton<CashFlowPage>();

        return builder.Build();
    }
}
