namespace GroceryStoreMaui.Models;

public enum UserRole
{
    Admin,
    Staff
}

public class User
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    [SQLite.Unique]
    public string Username { get; set; } = "";

    public string Password { get; set; } = ""; // Demo: lưu plain text cho đơn giản

    public string FullName { get; set; } = "";

    public UserRole Role { get; set; } = UserRole.Staff;

    public bool IsActive { get; set; } = true;
}
