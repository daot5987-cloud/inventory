using GroceryStoreMaui.Services;
using GroceryStoreMaui.Models;

namespace GroceryStoreMaui.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly ProductService _productService;
    private readonly DatabaseService _db;

    public DashboardPage(ProductService productService, DatabaseService db)
    {
        InitializeComponent();
        _productService = productService;
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var conn = _db.Connection;

        var products = await _productService.GetProductsAsync();
        var customers = await conn.Table<Customer>().ToListAsync();
        var lowStock = products.Where(p => p.Status != ProductStatus.InStock).ToList();

        TotalProductLabel.Text = products.Count.ToString();
        TotalCustomerLabel.Text = customers.Count.ToString();
        LowStockCollection.ItemsSource = lowStock;
    }
}
