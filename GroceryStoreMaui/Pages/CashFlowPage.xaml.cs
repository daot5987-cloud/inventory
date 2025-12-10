using GroceryStoreMaui.Models;
using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class CashFlowPage : ContentPage
{
    private readonly FinanceService _financeService;

    public CashFlowPage(FinanceService financeService)
    {
        InitializeComponent();
        _financeService = financeService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Mặc định xem 7 ngày gần nhất
        var today = DateTime.Today;
        FromDatePicker.Date = today.AddDays(-7);
        ToDatePicker.Date = today;

        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var from = FromDatePicker.Date;
        var to = ToDatePicker.Date.AddDays(1).AddTicks(-1); // đến cuối ngày

        var list = await _financeService.GetCashTransactionsAsync(from, to);
        CashCollection.ItemsSource = list;

        decimal totalIn = list.Where(c => c.Type == CashType.In).Sum(c => c.Amount);
        decimal totalOut = list.Where(c => c.Type == CashType.Out).Sum(c => c.Amount);
        decimal balance = totalIn - totalOut;

        TotalInLabel.Text = $"{totalIn:N0} đ";
        TotalOutLabel.Text = $"{totalOut:N0} đ";
        BalanceLabel.Text = $"{balance:N0} đ";
    }

    private async void OnFilterClicked(object sender, EventArgs e)
    {
        await LoadDataAsync();
    }
}
