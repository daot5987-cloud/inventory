using SQLite;
using GroceryStoreMaui.Models;

namespace GroceryStoreMaui.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _db;

    public DatabaseService()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "grocery_store.db3");
        _db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitAsync()
    {
        await _db.CreateTableAsync<User>();
        await _db.CreateTableAsync<LoginHistory>();
        await _db.CreateTableAsync<Product>();
        await _db.CreateTableAsync<Supplier>();
        await _db.CreateTableAsync<Customer>();
        await _db.CreateTableAsync<Purchase>();
        await _db.CreateTableAsync<PurchaseItem>();
        await _db.CreateTableAsync<Sale>();
        await _db.CreateTableAsync<SaleItem>();
        await _db.CreateTableAsync<CashTransaction>();

        // Tạo user admin mặc định
        var admin = await _db.Table<User>().Where(u => u.Username == "admin").FirstOrDefaultAsync();
        if (admin == null)
        {
            await _db.InsertAsync(new User
            {
                Username = "admin",
                Password = "123456",
                FullName = "Quản trị viên",
                Role = UserRole.Admin
            });
        }
    }

    public SQLiteAsyncConnection Connection => _db;
}
