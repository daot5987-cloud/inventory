using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public class Customer
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Address { get; set; } = "";

    public int LoyaltyPoints { get; set; } = 0;
}
