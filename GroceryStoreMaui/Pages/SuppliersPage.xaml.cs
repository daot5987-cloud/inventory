using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class SuppliersPage : ContentPage
{
    private readonly DatabaseService _database;
    private Supplier? _currentSupplier;

    public SuppliersPage(DatabaseService database)
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
        var query = conn.Table<Supplier>();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(keyword) ||
                s.Phone.ToLower().Contains(keyword));
        }

        var list = await query.OrderBy(s => s.Name).ToListAsync();
        SupplierCollection.ItemsSource = list;
    }

    private async void OnSearch(object sender, EventArgs e)
    {
        await LoadDataAsync(SearchBar.Text);
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        SearchBar.Text = "";
        _currentSupplier = null;
        ClearForm();
        await LoadDataAsync();
    }

    private void OnNewClicked(object sender, EventArgs e)
    {
        _currentSupplier = null;
        ClearForm();
    }

    private void ClearForm()
    {
        NameEntry.Text = "";
        PhoneEntry.Text = "";
        AddressEntry.Text = "";
        EmailEntry.Text = "";
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Supplier s)
        {
            _currentSupplier = s;
            NameEntry.Text = s.Name;
            PhoneEntry.Text = s.Phone;
            AddressEntry.Text = s.Address;
            EmailEntry.Text = s.Email;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var conn = _database.Connection;

        if (_currentSupplier == null)
            _currentSupplier = new Supplier();

        _currentSupplier.Name = NameEntry.Text ?? "";
        _currentSupplier.Phone = PhoneEntry.Text ?? "";
        _currentSupplier.Address = AddressEntry.Text ?? "";
        _currentSupplier.Email = EmailEntry.Text ?? "";

        if (_currentSupplier.Id == 0)
            await conn.InsertAsync(_currentSupplier);
        else
            await conn.UpdateAsync(_currentSupplier);

        await DisplayAlert("OK", "Đã lưu nhà cung cấp", "Đóng");
        await LoadDataAsync(SearchBar.Text);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentSupplier == null || _currentSupplier.Id == 0)
        {
            await DisplayAlert("Thông báo", "Chưa chọn nhà cung cấp để xóa", "OK");
            return;
        }

        bool confirm = await DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa nhà cung cấp này?", "Xóa", "Hủy");
        if (!confirm) return;

        var conn = _database.Connection;
        await conn.DeleteAsync(_currentSupplier);
        _currentSupplier = null;
        ClearForm();
        await LoadDataAsync(SearchBar.Text);
    }
}
