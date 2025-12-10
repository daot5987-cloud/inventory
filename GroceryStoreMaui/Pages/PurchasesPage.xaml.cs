using System.Collections.ObjectModel;
using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class PurchasesPage : ContentPage
{
    private readonly DatabaseService _database;
    private readonly ProductService _productService;
    private readonly PurchaseService _purchaseService;
    private readonly AuthService _authService;

    private List<Supplier> _suppliers = new();
    private ObservableCollection<PurchaseCartItem> _cart =
        new ObservableCollection<PurchaseCartItem>();

    public PurchasesPage(
        DatabaseService database,
        ProductService productService,
        PurchaseService purchaseService,
        AuthService authService)
    {
        InitializeComponent();
        _database = database;
        _productService = productService;
        _purchaseService = purchaseService;
        _authService = authService;

        CartCollection.ItemsSource = _cart;
    }

    private class PurchaseCartItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSuppliersAsync();
        await LoadPurchasesAsync();
    }

    private async Task LoadSuppliersAsync()
    {
        var conn = _database.Connection;
        _suppliers = await conn.Table<Supplier>().OrderBy(s => s.Name).ToListAsync();
        SupplierPicker.ItemsSource = _suppliers;
    }

    private async Task LoadPurchasesAsync()
    {
        var list = await _purchaseService.GetPurchasesAsync();
        PurchaseCollection.ItemsSource = list;
    }

    private async void OnSearchProduct(object sender, EventArgs e)
    {
        var keyword = ProductSearchBar.Text;
        SearchResultCollection.ItemsSource = await _productService.GetProductsAsync(keyword);
    }

    private void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Product p)
        {
            // Thêm 1 sp với số lượng 1, giá nhập = ImportPrice hiện tại
            var exist = _cart.FirstOrDefault(x => x.Product.Id == p.Id);
            if (exist != null)
            {
                exist.Quantity += 1;
                // cập nhật để UI refresh
                var idx = _cart.IndexOf(exist);
                _cart.RemoveAt(idx);
                _cart.Insert(idx, exist);
            }
            else
            {
                _cart.Add(new PurchaseCartItem
                {
                    Product = p,
                    Quantity = 1,
                    UnitPrice = p.ImportPrice
                });
            }

            UpdateTotals();
            SearchResultCollection.SelectedItem = null;
        }
    }

    private void UpdateTotals()
    {
        decimal total = _cart.Sum(c => c.LineTotal);
        decimal.TryParse(PaidAmountEntry.Text, out var paid);
        decimal debt = total - paid;
        if (debt < 0) debt = 0;

        TotalLabel.Text = $"{total:N0} đ";
        DebtLabel.Text = $"{debt:N0} đ";
    }

    private void PaidAmountEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotals();
    }

    private async void OnSavePurchaseClicked(object sender, EventArgs e)
    {
        if (SupplierPicker.SelectedItem is not Supplier supplier)
        {
            await DisplayAlert("Lỗi", "Vui lòng chọn nhà cung cấp", "OK");
            return;
        }

        if (_cart.Count == 0)
        {
            await DisplayAlert("Lỗi", "Giỏ nhập hàng đang trống", "OK");
            return;
        }

        decimal.TryParse(PaidAmountEntry.Text, out var paid);

        var items = _cart.Select(c =>
            (c.Product, c.Quantity, c.UnitPrice)).ToList();

        var creator = _authService.CurrentUser?.Username ?? "unknown";

        var purchase = await _purchaseService.CreatePurchaseAsync(
            supplier.Id,
            items,
            paid,
            creator);

        await DisplayAlert("Thành công", $"Đã tạo phiếu nhập PN#{purchase.Id}", "OK");

        _cart.Clear();
        PaidAmountEntry.Text = "0";
        UpdateTotals();
        await LoadPurchasesAsync();
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        ProductSearchBar.Text = "";
        _cart.Clear();
        PaidAmountEntry.Text = "0";
        UpdateTotals();
        await LoadSuppliersAsync();
        await LoadPurchasesAsync();
    }
}
