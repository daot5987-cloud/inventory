using GroceryStoreMaui.Models;
using SQLite;

namespace GroceryStoreMaui.Services;

public class PurchaseService
{
    private readonly SQLiteAsyncConnection _db;
    private readonly ProductService _productService;

    public PurchaseService(DatabaseService database, ProductService productService)
    {
        _db = database.Connection;
        _productService = productService;
    }

    // Lấy danh sách phiếu nhập (mới nhất trước)
    public Task<List<Purchase>> GetPurchasesAsync()
    {
        return _db.Table<Purchase>()
                  .OrderByDescending(p => p.Date)
                  .ToListAsync();
    }

    // Tạo phiếu nhập hàng:
    // - Cộng tồn kho sản phẩm
    // - Cập nhật công nợ nhà cung cấp
    // - Ghi phiếu chi (CashTransaction Out)
    public async Task<Purchase> CreatePurchaseAsync(
        int supplierId,
        List<(Product product, int qty, decimal importPrice)> items,
        decimal paidAmount,
        string createdBy)
    {
        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Không có mặt hàng nào trong phiếu nhập.");

        decimal total = items.Sum(i => i.importPrice * i.qty);
        if (total < 0) total = 0;

        var purchase = new Purchase
        {
            SupplierId = supplierId,
            Date = DateTime.Now,
            TotalAmount = total,
            PaidAmount = paidAmount,
            CreatedBy = createdBy
        };

        await _db.RunInTransactionAsync(conn =>
        {
            // Lưu phiếu nhập
            conn.Insert(purchase);

            // Lưu chi tiết + cập nhật tồn kho
            foreach (var (product, qty, importPrice) in items)
            {
                var item = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = product.Id,
                    Quantity = qty,
                    UnitPrice = importPrice
                };
                conn.Insert(item);

                // Cộng tồn kho
                product.Quantity += qty;

                if (product.Quantity <= 0)
                    product.Status = ProductStatus.OutOfStock;
                else if (product.Quantity < 5)
                    product.Status = ProductStatus.LowStock;
                else
                    product.Status = ProductStatus.InStock;

                conn.Update(product);
            }

            // Cập nhật công nợ nhà cung cấp
            var supplier = conn.Find<Supplier>(supplierId);
            if (supplier != null)
            {
                decimal debtIncrease = total - paidAmount;
                if (debtIncrease < 0) debtIncrease = 0; // tránh âm
                supplier.Debt += debtIncrease;
                conn.Update(supplier);
            }

            // Ghi phiếu chi (tiền đã trả)
            if (paidAmount > 0)
            {
                var cash = new CashTransaction
                {
                    Date = DateTime.Now,
                    Amount = paidAmount,
                    Type = CashType.Out,
                    Description = $"Nhập hàng PN#{purchase.Id}",
                    PurchaseId = purchase.Id
                };
                conn.Insert(cash);
            }
        });

        return purchase;
    }
}
