using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public class Purchase
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public DateTime Date { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public string CreatedBy { get; set; } = "";
}
