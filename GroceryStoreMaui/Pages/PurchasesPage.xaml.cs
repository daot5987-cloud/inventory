using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages
{
    public partial class PurchasesPage : ContentPage
    {
        private readonly ProductService _productService;
        private readonly PurchaseService _purchaseService;
        private readonly DatabaseService _db;
        private readonly AuthService _authService;

        // 1 dòng trong giỏ nhập – có INotifyPropertyChanged để UI tự cập nhật
        public class PurchaseCartItem : INotifyPropertyChanged
        {
            private Product _product = null!;
            private int _quantity;
            private decimal _unitPrice;

            public Product Product
            {
                get => _product;
                set
                {
                    if (_product != value)
                    {
                        _product = value;
                        OnPropertyChanged();
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

            public decimal UnitPrice
            {
                get => _unitPrice;
                set
                {
                    if (_unitPrice != value)
                    {
                        _unitPrice = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(LineTotal));
                    }
                }
            }

            public decimal LineTotal => UnitPrice * Quantity;

            public event PropertyChangedEventHandler? PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly ObservableCollection<PurchaseCartItem> _cart = new();

        public PurchasesPage(
            ProductService productService,
            PurchaseService purchaseService,
            DatabaseService db,
            AuthService authService)
        {
            InitializeComponent();

            _productService = productService;
            _purchaseService = purchaseService;
            _db = db;
            _authService = authService;

            CartCollection.ItemsSource = _cart;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var conn = _db.Connection;

                // load nhà cung cấp
                var suppliers = await conn.Table<Supplier>().ToListAsync();
                SupplierPicker.ItemsSource = suppliers;
                SupplierPicker.ItemDisplayBinding = new Binding("Name");

                // load phiếu nhập gần đây
                var purchases = await conn.Table<Purchase>()
                                          .OrderByDescending(p => p.Date)
                                          .Take(10)
                                          .ToListAsync();
                RecentPurchasesCollection.ItemsSource = purchases;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không tải được dữ liệu nhập hàng.\n" + ex.Message, "OK");
            }
        }

        // Tìm sản phẩm khi bấm nút search
        private async void OnSearchProduct(object sender, EventArgs e)
        {
            await SearchProductsAsync(ProductSearchBar.Text);
        }

        // Tìm khi gõ
        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            await SearchProductsAsync(e.NewTextValue);
        }

        private async Task SearchProductsAsync(string? keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    SearchResultCollection.ItemsSource = null;
                    return;
                }

                SearchResultCollection.ItemsSource = await _productService.GetProductsAsync(keyword);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không tải được danh sách sản phẩm.\n" + ex.Message, "OK");
            }
        }

        // Chọn sản phẩm từ list để thêm vào giỏ nhập
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

        // Tăng số lượng
        private void OnIncreaseQuantityClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PurchaseCartItem item)
            {
                item.Quantity += 1;
                UpdateTotals();
            }
        }

        // Giảm số lượng
        private void OnDecreaseQuantityClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PurchaseCartItem item)
            {
                item.Quantity -= 1;
                if (item.Quantity <= 0)
                {
                    _cart.Remove(item);
                }

                UpdateTotals();
            }
        }

        // Xóa 1 dòng khỏi giỏ
        private void OnRemoveItemClicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is PurchaseCartItem item)
            {
                _cart.Remove(item);
                UpdateTotals();
            }
        }

        // Khi sửa số tiền đã trả
        private void PaidEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTotals();
        }

        // Cập nhật tổng tiền & công nợ
        private void UpdateTotals()
        {
            decimal total = _cart.Sum(c => c.LineTotal);
            decimal.TryParse(PaidEntry.Text, out var paid);
            if (paid < 0) paid = 0;

            var debt = total - paid;
            if (debt < 0) debt = 0;

            TotalAmountLabel.Text = $"{total:N0} đ";
            PaidAmountLabel.Text = $"{paid:N0} đ";
            DebtAmountLabel.Text = $"{debt:N0} đ";
        }

        // Lưu phiếu nhập
        private async void OnSavePurchaseClicked(object sender, EventArgs e)
        {
            if (SupplierPicker.SelectedItem is not Supplier supplier)
            {
                await DisplayAlert("Lỗi", "Chọn nhà cung cấp!", "OK");
                return;
            }

            decimal.TryParse(PaidEntry.Text, out var paid);

            var items = _cart.Select(c =>
                (c.Product, c.Quantity, c.UnitPrice)
            ).ToList();

            if (items.Count == 0)
            {
                await DisplayAlert("Lỗi", "Chưa có mặt hàng nào!", "OK");
                return;
            }

            try
            {
                var createdBy = _authService.CurrentUser?.Username ?? "system";

                var purchase = await _purchaseService.CreatePurchaseAsync(
                    supplier.Id,
                    items,
                    paid,
                    createdBy
                );

                await DisplayAlert(
                    "Thành công",
                    $"Đã tạo phiếu nhập #{purchase.Id}",
                    "OK"
                );

                _cart.Clear();
                PaidEntry.Text = "0";
                UpdateTotals();

                OnAppearing();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không lưu được phiếu nhập.\n" + ex.Message, "OK");
            }
        }

        // Tải lại dữ liệu
        private void OnRefreshClicked(object sender, EventArgs e)
        {
            _cart.Clear();
            PaidEntry.Text = "0";
            UpdateTotals();
            OnAppearing();
        }

        // Khi chọn 1 phiếu nhập gần đây -> xem chi tiết
        private async void OnRecentPurchaseSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is not Purchase purchase)
                return;

            try
            {
                var conn = _db.Connection;

                // Lấy các dòng hàng của phiếu này
                var items = await conn.Table<PurchaseItem>()
                                      .Where(pi => pi.PurchaseId == purchase.Id)
                                      .ToListAsync();

                if (items.Count == 0)
                {
                    await DisplayAlert("Chi tiết phiếu nhập",
                        "Phiếu này không có dòng hàng nào.", "OK");
                    RecentPurchasesCollection.SelectedItem = null;
                    return;
                }

                // Lấy toàn bộ sản phẩm rồi lọc trong bộ nhớ
                var allProducts = await conn.Table<Product>().ToListAsync();
                var productDict = allProducts.ToDictionary(p => p.Id, p => p);

                var lines = new List<string>();

                foreach (var it in items)
                {
                    productDict.TryGetValue(it.ProductId, out var prod);
                    var name = prod?.Name ?? $"SP#{it.ProductId}";
                    var code = prod?.Code ?? "";

                    lines.Add($"{name} ({code})  -  SL: {it.Quantity}");
                }

                string message =
                    $"Mã phiếu: PN#{purchase.Id}\n" +
                    $"Ngày: {purchase.Date:dd/MM/yyyy HH:mm}\n" +
                    $"Tổng tiền: {purchase.TotalAmount:N0} đ\n\n" +
                    string.Join("\n", lines);

                await DisplayAlert("Chi tiết phiếu nhập", message, "Đóng");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", "Không xem được chi tiết phiếu.\n" + ex.Message, "OK");
            }
            finally
            {
                // bỏ chọn để lần sau bấm lại còn trigger
                RecentPurchasesCollection.SelectedItem = null;
            }
        }
    }
}
