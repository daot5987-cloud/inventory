using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class SalesPage : ContentPage
{
    private readonly ProductService _productService;
    private readonly SalesService _salesService;
    private readonly AuthService _authService;

    // Class đại diện 1 dòng trong giỏ hàng – có INotifyPropertyChanged
    public class CartItem : INotifyPropertyChanged
    {
        private Product _product = null!;
        private int _quantity;

        public Product Product
        {
            get => _product;
            set
            {
                if (_product != value)
                {
                    _product = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public decimal LineTotal => Product.SalePrice * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

    // Tìm sản phẩm (khi bấm search)
    private async void OnSearchProduct(object sender, EventArgs e)
    {
        var keyword = ProductSearchBar.Text;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultCollection.ItemsSource = null;
            return;
        }

        SearchResultCollection.ItemsSource = await _productService.GetProductsAsync(keyword);
    }

    // Tìm khi gõ
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var keyword = e.NewTextValue;
        if (string.IsNullOrWhiteSpace(keyword))
        {
            SearchResultCollection.ItemsSource = null;
            return;
        }

        SearchResultCollection.ItemsSource = await _productService.GetProductsAsync(keyword);
    }

    // Chọn SP từ list kết quả -> thêm vào giỏ
    private void OnProductSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Product p)
        {
            var exist = _cart.FirstOrDefault(x => x.Product.Id == p.Id);
            if (exist != null)
            {
                exist.Quantity += 1;
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
        if (sender is Button btn && btn.BindingContext is CartItem item)
        {
            item.Quantity += 1;
            UpdateTotal();
        }
    }

    // Giảm số lượng
    private void OnDecreaseQuantityClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is CartItem item)
        {
            item.Quantity -= 1;
            if (item.Quantity <= 0)
            {
                _cart.Remove(item);
            }

            UpdateTotal();
        }
    }

    // Xóa 1 dòng khỏi giỏ
    private void OnRemoveItemClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is CartItem item)
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

        bool confirm = await DisplayAlert("Xóa giỏ hàng",
            "Bạn có chắc muốn xóa toàn bộ giỏ hàng?", "Xóa", "Hủy");

        if (confirm)
        {
            _cart.Clear();
            DiscountEntry.Text = "0";
            UpdateTotal();
        }
    }

    // Khi sửa ô giảm giá
    private void DiscountEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTotal();
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
            PaymentMethod.Cash,
            seller);

        await DisplayAlert("Thành công",
            $"Đã tạo hóa đơn #{sale.Id}\nTổng tiền: {sale.TotalAmount:N0} đ",
            "OK");

        _cart.Clear();
        DiscountEntry.Text = "0";
        UpdateTotal();
    }
}
