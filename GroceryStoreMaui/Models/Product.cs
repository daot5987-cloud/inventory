using System.Data.SqlTypes;

namespace GroceryStoreMaui.Models;

public enum ProductStatus
{
    InStock,
    LowStock,
    OutOfStock
}

public class Product
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    [SQLite.Unique]
    public string Code { get; set; } = "";

    public string Name { get; set; } = "";

    public string Unit { get; set; } = "Cái";

    public string Category { get; set; } = "";

    public decimal ImportPrice { get; set; }

    public decimal SalePrice { get; set; }

    public int Quantity { get; set; }

    public string ImagePath { get; set; } = "";

    public ProductStatus Status { get; set; } = ProductStatus.InStock;
}
