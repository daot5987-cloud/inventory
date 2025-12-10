using GroceryStoreMaui.Services;

namespace GroceryStoreMaui.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập đủ tài khoản và mật khẩu", "OK");
            return;
        }

        bool success = await _authService.LoginAsync(username, password);
        if (!success)
        {
            await DisplayAlert("Lỗi", "Tài khoản hoặc mật khẩu không đúng", "OK");
            return;
        }

        // Chuyển sang Dashboard, reset stack
        await Shell.Current.GoToAsync("//Dashboard");
    }
}
