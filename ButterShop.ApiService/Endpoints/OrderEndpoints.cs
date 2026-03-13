using Microsoft.EntityFrameworkCore;
using ButterShop.ApiService.Data;
using ButterShop.ApiService.Models;

namespace ButterShop.ApiService.Endpoints;

public static class OrderEndpoints
{
    public record CreateOrderRequest(string CustomerName, List<OrderItemRequest> Items);
    public record OrderItemRequest(int ProductId, int Quantity);
    public record UpdateOrderRequest(string CustomerName);

    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var orders = await db.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ToListAsync();

            return Results.Ok(orders.Select(o => new
            {
                o.Id,
                o.CustomerName,
                o.OrderDate,
                Items = o.OrderProducts.Select(op => new
                {
                    op.Product.Id,
                    op.Product.Name,
                    op.Product.Price,
                    op.Quantity
                })
            }));
        });

        group.MapGet("/{id}", async (int id, AppDbContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return Results.NotFound();

            return Results.Ok(new
            {
                order.Id,
                order.CustomerName,
                order.OrderDate,
                Items = order.OrderProducts.Select(op => new
                {
                    op.Product.Id,
                    op.Product.Name,
                    op.Product.Price,
                    op.Quantity
                })
            });
        });

        group.MapPost("/", async (CreateOrderRequest request, AppDbContext db) =>
        {
            var order = new Order
            {
                CustomerName = request.CustomerName,
                OrderDate = DateTime.UtcNow
            };

            foreach (var item in request.Items)
            {
                var product = await db.Products.FindAsync(item.ProductId);
                if (product is null)
                    return Results.BadRequest($"Product with id {item.ProductId} not found.");

                order.OrderProducts.Add(new OrderProduct
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            return Results.Created($"/api/orders/{order.Id}", new
            {
                order.Id,
                order.CustomerName,
                order.OrderDate,
                Items = order.OrderProducts.Select(op => new
                {
                    op.ProductId,
                    op.Quantity
                })
            });
        });

        group.MapPut("/{id}", async (int id, UpdateOrderRequest request, AppDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null) return Results.NotFound();

            order.CustomerName = request.CustomerName;
            await db.SaveChangesAsync();

            return Results.Ok(order);
        });

        group.MapPost("/{id}/products", async (int id, OrderItemRequest item, AppDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null) return Results.NotFound("Order not found.");

            var product = await db.Products.FindAsync(item.ProductId);
            if (product is null) return Results.BadRequest("Product not found.");

            var existing = await db.OrderProducts
                .FirstOrDefaultAsync(op => op.OrderId == id && op.ProductId == item.ProductId);

            if (existing is not null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                db.OrderProducts.Add(new OrderProduct
                {
                    OrderId = id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapDelete("/{orderId}/products/{productId}", async (int orderId, int productId, AppDbContext db) =>
        {
            var orderProduct = await db.OrderProducts
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (orderProduct is null) return Results.NotFound();

            db.OrderProducts.Remove(orderProduct);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var order = await db.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return Results.NotFound();

            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
