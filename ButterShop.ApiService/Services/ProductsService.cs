using ButterShop.ApiService.Data;
using ButterShop.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ButterShop.ApiService.Services;

public class ProductsService(AppDbContext db) : IProductsService
{
    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await db.Products.ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await db.Products.FindAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        product.OrderProducts = [];
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> UpdateProductAsync(int id, Product updatedProduct)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return null;
        }

        product.Name = updatedProduct.Name;
        product.Price = updatedProduct.Price;
        product.Description = updatedProduct.Description;

        await db.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }
}
