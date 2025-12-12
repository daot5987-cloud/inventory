using System.Linq;                     // 👈 nhớ import
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

        try
        {
            var conn = _db.Connection;

            // Lấy danh sách
            var products = await _productService.GetProductsAsync();
            var customers = await conn.Table<Customer>().ToListAsync();

            // Hàng sắp hết / hết
            var lowStock = products
                .Where(p => p.Status != ProductStatus.InStock)
                .ToList();

            // 👇 ĐÚNG THEO TÊN TRONG XAML
            ProductCountLabel.Text = products.Count.ToString();
            CustomerCountLabel.Text = customers.Count.ToString();

            LowStockCollection.ItemsSource = lowStock;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi", "Không tải được dữ liệu tổng quan.\n" + ex.Message, "OK");
        }
    }
}
