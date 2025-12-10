using System;
using System.Collections.ObjectModel;
using System.Linq;
using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class SalesPage : ContentPage
{
    private readonly ProductService _productService;
    private readonly SalesService _salesService;
    private readonly AuthService _authService;

    // Class đại diện cho 1 dòng trong giỏ hàng
    public class CartItem
    {
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal LineTotal => Product.SalePrice * Quantity;
    }

    private readonly ObservableCollection<CartItem> _cart = new();

    public SalesPage(ProductService productService, SalesService salesService, AuthService authService)
    {
        InitializeComponent();

        _productService = productService;
        _salesService = salesService;
        _authService = authService;

        CartCollection.ItemsSource = _cart;
    }

    // 🔎 Tìm sản phẩm khi bấm nút search trên bàn phím
    private async void OnSearchProduct(object sender, EventArgs e)
    {
        var keyword = ProductSearchBar.Text;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultCollection.ItemsSource = null;
            return;
        }

        var results = await _productService.GetProductsAsync(keyword);
        SearchResultCollection.ItemsSource = results;
    }

    // 🔎 Tìm sản phẩm khi gõ chữ (TextChanged trong XAML)
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue;

        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultCollection.ItemsSource = null;
            return;
        }

        var results = await _productService.GetProductsAsync(keyword);
        SearchResultCollection.ItemsSource = results;
    }

    // Khi chọn 1 sản phẩm từ danh sách kết quả
    private void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Product p)
        {
            var exist = _cart.FirstOrDefault(x => x.Product.Id == p.Id);
            if (exist != null)
            {
                exist.Quantity += 1;

                // cập nhật lại item để UI refresh
                var idx = _cart.IndexOf(exist);
                _cart.RemoveAt(idx);
                _cart.Insert(idx, exist);
            }
            else
            {
                _cart.Add(new CartItem
                {
                    Product = p,
                    Quantity = 1
                });
            }

            UpdateTotal();
            SearchResultCollection.SelectedItem = null;
        }
    }

    // Tăng số lượng
    private void OnIncreaseQuantityClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is CartItem item)
        {
            item.Quantity += 1;
            RefreshCartItem(item);
            UpdateTotal();
        }
    }

    // Giảm số lượng
    private void OnDecreaseQuantityClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is CartItem item)
        {
            item.Quantity -= 1;
            if (item.Quantity <= 0)
            {
                _cart.Remove(item);
            }
            else
            {
                RefreshCartItem(item);
            }
            UpdateTotal();
        }
    }

    // Xóa 1 dòng khỏi giỏ
    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is CartItem item)
        {
            _cart.Remove(item);
            UpdateTotal();
        }
    }

    // Xóa cả giỏ
    private async void OnClearCartClicked(object sender, EventArgs e)
    {
        if (_cart.Count == 0)
            return;

        bool confirm = await DisplayAlert(
            "Xóa giỏ hàng",
            "Bạn có chắc muốn xóa toàn bộ giỏ hàng?",
            "Xóa", "Hủy");

        if (confirm)
        {
            _cart.Clear();
            UpdateTotal();
        }
    }

    // Cập nhật tổng tiền
    private void UpdateTotal()
    {
        decimal total = _cart.Sum(c => c.LineTotal);
        decimal.TryParse(DiscountEntry.Text, out var discount);
        total -= discount;
        if (total < 0) total = 0;

        TotalLabel.Text = $"{total:N0} đ";
    }

    private void DiscountEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotal();
    }

    // Làm “động” lại 1 item trong ObservableCollection để UI nhận thay đổi
    private void RefreshCartItem(CartItem item)
    {
        var idx = _cart.IndexOf(item);
        if (idx >= 0)
        {
            _cart.RemoveAt(idx);
            _cart.Insert(idx, item);
        }
    }

    // Thanh toán
    private async void OnCheckoutClicked(object sender, EventArgs e)
    {
        if (_cart.Count == 0)
        {
            await DisplayAlert("Thông báo", "Giỏ hàng đang trống", "OK");
            return;
        }

        decimal.TryParse(DiscountEntry.Text, out var discount);
        var list = _cart.Select(c => (c.Product, c.Quantity)).ToList();

        var seller = _authService.CurrentUser?.Username ?? "unknown";

        var sale = await _salesService.CreateSaleAsync(
            list,
            discount,
            PaymentMethod.Cash,   // mặc định tiền mặt
            seller);

        await DisplayAlert(
            "Thành công",
            $"Đã tạo hóa đơn #{sale.Id}\nTổng tiền: {sale.TotalAmount:N0} đ",
            "OK");

        _cart.Clear();
        DiscountEntry.Text = "0";
        UpdateTotal();
    }
}
