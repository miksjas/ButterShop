using ButterShop.ApiService.Models;

namespace ButterShop.ApiService.Services;

public interface IProductsService
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(Product product);
    Task<Product?> UpdateProductAsync(int id, Product product);
    Task<bool> DeleteProductAsync(int id);
}
