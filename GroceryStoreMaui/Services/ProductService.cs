using GroceryStoreMaui.Models;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStoreMaui.Services
{
    public class ProductService
    {
        private readonly SQLiteAsyncConnection _db;

        public ProductService(DatabaseService database)
        {
            _db = database.Connection;
        }

        // Lấy danh sách sản phẩm + tìm kiếm
        public async Task<List<Product>> GetProductsAsync(string? search = null)
        {
            // Lấy tất cả sản phẩm trước (SQLite hiểu được)
            var products = await _db.Table<Product>().ToListAsync();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                // Lọc trong bộ nhớ, dùng ToLower / ?? thoải mái
                products = products
                    .Where(p =>
                        (!string.IsNullOrEmpty(p.Name) &&
                         p.Name.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(p.Code) &&
                         p.Code.ToLower().Contains(search)) ||
                        (!string.IsNullOrEmpty(p.Category) &&
                         p.Category.ToLower().Contains(search)))
                    .ToList();
            }

            return products
                .OrderBy(p => p.Name)
                .ToList();
        }

        public Task<Product?> GetByCodeAsync(string code)
        {
            return _db.Table<Product>()
                      .Where(p => p.Code == code)
                      .FirstOrDefaultAsync();
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

        // Xóa theo Id (đúng với chỗ bạn gọi _productService.DeleteProductAsync(product.Id))
        public Task<int> DeleteProductAsync(int productId)
        {
            return _db.Table<Product>().DeleteAsync(p => p.Id == productId);
        }
    }
}
