using GroceryStoreMaui.Models;
using SQLite;

namespace GroceryStoreMaui.Services;

public class AuthService
{
    private readonly SQLiteAsyncConnection _db;

    public User? CurrentUser { get; private set; }

    public AuthService(DatabaseService database)
    {
        _db = database.Connection;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var user = await _db.Table<User>()
            .Where(u => u.Username == username && u.IsActive)
            .FirstOrDefaultAsync();

        bool success = user != null && user.Password == password;

        await _db.InsertAsync(new LoginHistory
        {
            Username = username,
            LoginTime = DateTime.Now,
            IsSuccess = success,
            DeviceInfo = DeviceInfo.Current.Model
        });

        if (success)
            CurrentUser = user;

        return success;
    }

    public void Logout()
    {
        CurrentUser = null;
    }

    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public async Task ChangePasswordAsync(string newPassword)
    {
        if (CurrentUser == null) return;
        CurrentUser.Password = newPassword;
        await _db.UpdateAsync(CurrentUser);
    }

    public Task<List<User>> GetUsersAsync()
    {
        return _db.Table<User>().ToListAsync();
    }

    public Task<int> SaveUserAsync(User user)
    {
        if (user.Id == 0)
            return _db.InsertAsync(user);

        return _db.UpdateAsync(user);
    }

    public Task<int> DeleteUserAsync(User user)
    {
        return _db.DeleteAsync(user);
    }
}
