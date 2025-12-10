using GroceryStoreMaui.Services;
using GroceryStoreMaui.Models;

namespace GroceryStoreMaui.Pages;

public partial class ProductsPage : ContentPage
{
    private readonly ProductService _productService;

    public ProductsPage(ProductService productService)
    {
        InitializeComponent();
        _productService = productService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync(string? keyword = null)
    {
        ProductCollection.ItemsSource = await _productService.GetProductsAsync(keyword);
    }

    private async void OnSearch(object sender, EventArgs e)
    {
        await LoadDataAsync(SearchBar.Text);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchBar.Text = "";
        await LoadDataAsync();
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProductEditPage));
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Product product)
        {
            var navParam = new Dictionary<string, object>
            {
                { "Product", product }
            };
            await Shell.Current.GoToAsync(nameof(ProductEditPage), navParam);

            ProductCollection.SelectedItem = null;
        }
    }
}
