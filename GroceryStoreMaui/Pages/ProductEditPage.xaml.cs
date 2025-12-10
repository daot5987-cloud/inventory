using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

[QueryProperty(nameof(Product), "Product")]
public partial class ProductEditPage : ContentPage
{
    private readonly ProductService _productService;

    public Product? Product { get; set; }

    public ProductEditPage(ProductService productService)
    {
        InitializeComponent();
        _productService = productService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Product != null)
        {
            CodeEntry.Text = Product.Code;
            NameEntry.Text = Product.Name;
            CategoryEntry.Text = Product.Category;
            UnitEntry.Text = Product.Unit;
            ImportPriceEntry.Text = Product.ImportPrice.ToString();
            SalePriceEntry.Text = Product.SalePrice.ToString();
            QuantityEntry.Text = Product.Quantity.ToString();
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (Product == null)
            Product = new Product();

        Product.Code = CodeEntry.Text ?? "";
        Product.Name = NameEntry.Text ?? "";
        Product.Category = CategoryEntry.Text ?? "";
        Product.Unit = UnitEntry.Text ?? "Cái";

        decimal.TryParse(ImportPriceEntry.Text, out var importPrice);
        decimal.TryParse(SalePriceEntry.Text, out var salePrice);
        int.TryParse(QuantityEntry.Text, out var qty);

        Product.ImportPrice = importPrice;
        Product.SalePrice = salePrice;
        Product.Quantity = qty;

        await _productService.SaveProductAsync(Product);
        await DisplayAlert("OK", "Đã lưu sản phẩm", "Đóng");
        await Shell.Current.GoToAsync("..");
    }
}
