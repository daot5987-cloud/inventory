using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class CashFlowPage : ContentPage
{
    private readonly DatabaseService _db;

    public CashFlowPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;

        // Mặc định xem 7 ngày gần đây
        FromDatePicker.Date = DateTime.Today.AddDays(-7);
        ToDatePicker.Date = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SafeLoadAsync();
    }

    // Bọc LoadDataAsync trong try/catch để không crash app
    private async Task SafeLoadAsync()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi",
                "Không tải được dữ liệu thu chi.\n" + ex.Message,
                "OK");
        }
    }

    private async Task LoadDataAsync()
    {
        var from = FromDatePicker.Date.Date;
        var to = ToDatePicker.Date.Date;

        if (to < from)
            to = from;

        // Tính ngày kết thúc +1 ở ngoài, KHÔNG dùng trong biểu thức LINQ gửi xuống SQLite
        var endDate = to.AddDays(1);

        var conn = _db.Connection;
        if (conn == null)
        {
            throw new InvalidOperationException(
                "Kết nối CSDL chưa sẵn sàng (_db.Connection = null).");
        }

        // 1) Lấy tất cả CashTransaction từ DB
        var all = await conn.Table<CashTransaction>().ToListAsync();

        // 2) Lọc trong bộ nhớ => không còn lỗi "no such function: adddays"
        var list = all
            .Where(c => c.Date >= from && c.Date < endDate)
            .OrderByDescending(c => c.Date)
            .ToList();

        // Gán vào CollectionView
        CashCollection.ItemsSource = list;

        // Tính tổng thu / chi / chênh lệch
        decimal totalIn = list.Where(c => c.Type == CashType.In).Sum(c => c.Amount);
        decimal totalOut = list.Where(c => c.Type == CashType.Out).Sum(c => c.Amount);
        decimal balance = totalIn - totalOut;

        TotalInLabel.Text = $"{totalIn:N0} đ";
        TotalOutLabel.Text = $"{totalOut:N0} đ";
        BalanceLabel.Text = $"{balance:N0} đ";
    }

    // Nút "Lọc" trên Toolbar
    private async void OnFilterClicked(object sender, EventArgs e)
    {
        await SafeLoadAsync();
    }

    // Khi chạm vào 1 dòng thu/chi -> xem chi tiết
    private async void OnCashItemTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // Frame là container của từng item
            if (sender is not Frame frame)
                return;

            if (frame.BindingContext is not CashTransaction tx)
                return;

            string typeText = tx.Type == CashType.In
                ? "Thu (tiền vào)"
                : tx.Type == CashType.Out
                    ? "Chi (tiền ra)"
                    : tx.Type.ToString();

            string desc = string.IsNullOrWhiteSpace(tx.Description)
                ? "(không có nội dung)"
                : tx.Description;

            string message =
                $"Loại: {typeText}\n" +
                $"Số tiền: {tx.Amount:N0} đ\n" +
                $"Thời gian: {tx.Date:dd/MM/yyyy HH:mm}\n\n" +
                $"Nội dung:\n{desc}";

            await DisplayAlert("Chi tiết thu - chi", message, "Đóng");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Lỗi",
                "Không xem được chi tiết.\n" + ex.Message,
                "OK");
        }
    }
}
