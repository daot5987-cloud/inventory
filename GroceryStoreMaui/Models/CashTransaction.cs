using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public enum CashType
{
    In,
    Out
}

public class CashTransaction
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public decimal Amount { get; set; }

    public CashType Type { get; set; }

    public string Description { get; set; } = "";

    public int? SaleId { get; set; }

    public int? PurchaseId { get; set; }
}
