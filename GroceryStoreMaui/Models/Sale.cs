using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public enum PaymentMethod
{
    Cash,
    BankTransfer
}

public class Sale
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int? CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public string Seller { get; set; } = "";
}
