using GroceryStoreMaui.Services;

namespace GroceryStoreMaui;

public partial class App : Application
{
    public App(DatabaseService db)
    {
        InitializeComponent();

        // Khởi tạo DB
        Task.Run(async () => await db.InitAsync());

        MainPage = new AppShell();
    }
}
