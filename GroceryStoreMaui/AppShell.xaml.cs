namespace GroceryStoreMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(Pages.ProductEditPage), typeof(Pages.ProductEditPage));
    }
}
