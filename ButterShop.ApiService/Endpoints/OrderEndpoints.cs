using ButterShop.ApiService.Models;
using ButterShop.ApiService.Services;

namespace ButterShop.ApiService.Endpoints;

public static class OrderEndpoints
{
    public record CreateOrderRequest(string CustomerName, List<OrderItemRequest> Items);
    public record OrderItemRequest(int ProductId, int Quantity);
    public record UpdateOrderRequest(string CustomerName);

    public static void MapOrderEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapGet("/", async (IOrdersService ordersService) =>
        {
            var orders = await ordersService.GetOrdersAsync();

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

        group.MapGet("/{id}", async (int id, IOrdersService ordersService) =>
        {
            var order = await ordersService.GetOrderByIdAsync(id);
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

        group.MapPost("/", async (CreateOrderRequest request, IOrdersService ordersService) =>
        {
            var order = new Order
            {
                CustomerName = request.CustomerName,
                OrderDate = DateTime.UtcNow,
                OrderProducts = request.Items.Select(item => new OrderProduct
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            try
            {
                var createdOrder = await ordersService.CreateOrderAsync(order);

                return Results.Created($"/api/orders/{createdOrder.Id}", new
                {
                    createdOrder.Id,
                    createdOrder.CustomerName,
                    createdOrder.OrderDate,
                    Items = createdOrder.OrderProducts.Select(op => new
                    {
                        op.ProductId,
                        ProductName = op.Product?.Name,
                        ProductPrice = op.Product?.Price,
                        op.Quantity
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ex.Message);
            }
        });

        group.MapPut("/{id}", async (int id, UpdateOrderRequest request, IOrdersService ordersService) =>
        {
            var order = await ordersService.UpdateOrderAsync(id, request.CustomerName);
            if (order is null) return Results.NotFound();

            return Results.Ok(order);
        });

        group.MapPost("/{id}/products", async (int id, OrderItemRequest item, IOrdersService ordersService, IProductsService productsService) =>
        {
            var order = await ordersService.GetOrderByIdAsync(id);
            if (order is null) return Results.NotFound("Order not found.");

            var product = await productsService.GetProductByIdAsync(item.ProductId);
            if (product is null) return Results.BadRequest("Product not found.");

            await ordersService.AddProductToOrderAsync(id, item.ProductId, item.Quantity);
            return Results.Ok();
        });

        group.MapDelete("/{orderId}/products/{productId}", async (int orderId, int productId, IOrdersService ordersService) =>
        {
            var removed = await ordersService.RemoveProductFromOrderAsync(orderId, productId);
            if (!removed) return Results.NotFound();
            return Results.NoContent();
        });

        group.MapDelete("/{id}", async (int id, IOrdersService ordersService) =>
        {
            var deleted = await ordersService.DeleteOrderAsync(id);
            if (!deleted) return Results.NotFound();
            return Results.NoContent();
        });
    }
}
