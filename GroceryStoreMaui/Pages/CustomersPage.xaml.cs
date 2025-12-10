using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class CustomersPage : ContentPage
{
    private readonly DatabaseService _database;
    private Customer? _currentCustomer;

    public CustomersPage(DatabaseService database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync(string? keyword = null)
    {
        var conn = _database.Connection;
        var query = conn.Table<Customer>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(keyword) ||
                c.Phone.ToLower().Contains(keyword));
        }

        var list = await query.OrderBy(c => c.Name).ToListAsync();
        CustomerCollection.ItemsSource = list;
    }

    private async void OnSearch(object sender, EventArgs e)
    {
        await LoadDataAsync(SearchBar.Text);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchBar.Text = "";
        _currentCustomer = null;
        ClearForm();
        await LoadDataAsync();
    }

    private void OnNewClicked(object sender, EventArgs e)
    {
        _currentCustomer = null;
        ClearForm();
    }

    private void ClearForm()
    {
        NameEntry.Text = "";
        PhoneEntry.Text = "";
        AddressEntry.Text = "";
        PointsEntry.Text = "";
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Customer c)
        {
            _currentCustomer = c;
            NameEntry.Text = c.Name;
            PhoneEntry.Text = c.Phone;
            AddressEntry.Text = c.Address;
            PointsEntry.Text = c.LoyaltyPoints.ToString();
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var conn = _database.Connection;

        if (_currentCustomer == null)
            _currentCustomer = new Customer();

        _currentCustomer.Name = NameEntry.Text ?? "";
        _currentCustomer.Phone = PhoneEntry.Text ?? "";
        _currentCustomer.Address = AddressEntry.Text ?? "";

        int.TryParse(PointsEntry.Text, out var points);
        _currentCustomer.LoyaltyPoints = points;

        if (_currentCustomer.Id == 0)
            await conn.InsertAsync(_currentCustomer);
        else
            await conn.UpdateAsync(_currentCustomer);

        await DisplayAlert("OK", "Đã lưu khách hàng", "Đóng");
        await LoadDataAsync(SearchBar.Text);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentCustomer == null || _currentCustomer.Id == 0)
        {
            await DisplayAlert("Thông báo", "Chưa chọn khách hàng để xóa", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa khách hàng này?", "Xóa", "Hủy");
        if (!confirm) return;

        var conn = _database.Connection;
        await conn.DeleteAsync(_currentCustomer);
        _currentCustomer = null;
        ClearForm();
        await LoadDataAsync(SearchBar.Text);
    }
}
