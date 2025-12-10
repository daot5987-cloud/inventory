using GroceryStoreMaui.Models;
using SQLite;

namespace GroceryStoreMaui.Services;

public class SalesService
{
    private readonly SQLiteAsyncConnection _db;
    private readonly ProductService _productService;

    public SalesService(DatabaseService database, ProductService productService)
    {
        _db = database.Connection;
        _productService = productService;
    }

    public async Task<Sale> CreateSaleAsync(
        List<(Product product, int qty)> items,
        decimal discount,
        PaymentMethod method,
        string seller,
        int? customerId = null)
    {
        decimal total = items.Sum(i => i.product.SalePrice * i.qty) - discount;
        if (total < 0) total = 0;

        var sale = new Sale
        {
            Date = DateTime.Now,
            CustomerId = customerId,
            TotalAmount = total,
            DiscountAmount = discount,
            PaymentMethod = method,
            Seller = seller
        };

        await _db.RunInTransactionAsync(conn =>
        {
            conn.Insert(sale);

            foreach (var (product, qty) in items)
            {
                var item = new SaleItem
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = qty,
                    UnitPrice = product.SalePrice
                };
                conn.Insert(item);

                // Trừ tồn kho
                product.Quantity -= qty;
                if (product.Quantity <= 0)
                    product.Status = ProductStatus.OutOfStock;
                else if (product.Quantity < 5)
                    product.Status = ProductStatus.LowStock;
                else
                    product.Status = ProductStatus.InStock;

                conn.Update(product);
            }

            // Ghi phiếu thu
            var cash = new CashTransaction
            {
                Date = DateTime.Now,
                Amount = total,
                Type = CashType.In,
                Description = $"Bán hàng HĐ#{sale.Id}",
                SaleId = sale.Id
            };
            conn.Insert(cash);
        });

        return sale;
    }
}
