namespace GroceryStoreMaui.Models;

public class LoginHistory
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public string Username { get; set; } = "";

    public DateTime LoginTime { get; set; }

    public bool IsSuccess { get; set; }

    public string DeviceInfo { get; set; } = "";
}
