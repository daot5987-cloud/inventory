using GroceryStoreMaui.Models;
using SQLite;

namespace GroceryStoreMaui.Services;

public class ProductService
{
    private readonly SQLiteAsyncConnection _db;

    public ProductService(DatabaseService database)
    {
        _db = database.Connection;
    }

    public Task<List<Product>> GetProductsAsync(string? search = null)
    {
        var query = _db.Table<Product>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Code.ToLower().Contains(search) ||
                p.Category.ToLower().Contains(search));
        }

        return query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetByCodeAsync(string code)
    {
        return await _db.Table<Product>().Where(p => p.Code == code).FirstOrDefaultAsync();
    }

    public async Task<int> SaveProductAsync(Product product)
    {
        // Cập nhật trạng thái tồn kho
        if (product.Quantity <= 0)
            product.Status = ProductStatus.OutOfStock;
        else if (product.Quantity < 5)
            product.Status = ProductStatus.LowStock;
        else
            product.Status = ProductStatus.InStock;

        if (product.Id == 0)
            return await _db.InsertAsync(product);

        return await _db.UpdateAsync(product);
    }

    public Task<int> DeleteProductAsync(Product product)
        => _db.DeleteAsync(product);
}
