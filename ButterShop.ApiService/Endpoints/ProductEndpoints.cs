using Microsoft.EntityFrameworkCore;
using ButterShop.ApiService.Data;
using ButterShop.ApiService.Models;

namespace ButterShop.ApiService.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var products = await db.Products.ToListAsync();
            return Results.Ok(products);
        });

        group.MapGet("/{id}", async (int id, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        group.MapPost("/", async (Product product, AppDbContext db) =>
        {
            product.OrderProducts = new List<OrderProduct>();
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/api/products/{product.Id}", product);
        });

        group.MapPut("/{id}", async (int id, Product updated, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = updated.Name;
            product.Price = updated.Price;
            product.Description = updated.Description;

            await db.SaveChangesAsync();
            return Results.Ok(product);
        });

        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product is null) return Results.NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
