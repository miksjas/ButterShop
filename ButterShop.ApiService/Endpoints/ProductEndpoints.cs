using ButterShop.ApiService.Models;
using ButterShop.ApiService.Services;

namespace ButterShop.ApiService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (IProductsService productsService) =>
        {
            var products = await productsService.GetProductsAsync();
            return Results.Ok(products);
        });

        group.MapGet("/{id}", async (int id, IProductsService productsService) =>
        {
            var product = await productsService.GetProductByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        group.MapPost("/", async (Product product, IProductsService productsService) =>
        {
            var createdProduct = await productsService.CreateProductAsync(product);
            return Results.Created($"/api/products/{createdProduct.Id}", createdProduct);
        });

        group.MapPut("/{id}", async (int id, Product updated, IProductsService productsService) =>
        {
            var product = await productsService.UpdateProductAsync(id, updated);
            if (product is null) return Results.NotFound();
            return Results.Ok(product);
        });

        group.MapDelete("/{id}", async (int id, IProductsService productsService) =>
        {
            var deleted = await productsService.DeleteProductAsync(id);
            if (!deleted) return Results.NotFound();
            return Results.NoContent();
        });
    }
}
