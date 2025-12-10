using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public class Supplier
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Address { get; set; } = "";

    public string Phone { get; set; } = "";

    public string Email { get; set; } = "";

    public decimal Debt { get; set; } // Công nợ nhà cung cấp
}
