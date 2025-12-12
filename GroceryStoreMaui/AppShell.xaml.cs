using GroceryStoreMaui.Pages;

namespace GroceryStoreMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ProductEditPage), typeof(ProductEditPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Đăng xuất",
            "Bạn có chắc muốn đăng xuất?",
            "Đăng xuất",
            "Hủy");

        if (!confirm)
            return;

        // Xóa thông tin đăng nhập nếu có AuthService
        //(App.Current as App)?.AuthService.Logout();

        // Điều hướng về màn hình đăng nhập
        await Shell.Current.GoToAsync("//LoginPage");

        // Đóng menu flyout
        Shell.Current.FlyoutIsPresented = false;
    }
}
