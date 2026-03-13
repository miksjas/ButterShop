using ButterShop.ApiService.Data;
using ButterShop.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ButterShop.ApiService.Services;

public class OrdersService(AppDbContext db) : IOrdersService
{
    public async Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return await db.Orders
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await db.Orders
            .Include(o => o.OrderProducts)
            .ThenInclude(op => op.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        foreach (var item in order.OrderProducts)
        {
            var productExists = await db.Products.AnyAsync(p => p.Id == item.ProductId);
            if (!productExists)
            {
                throw new InvalidOperationException($"Product with id {item.ProductId} not found.");
            }
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return await GetOrderByIdAsync(order.Id) ?? order;
    }

    public async Task<Order?> UpdateOrderAsync(int id, string customerName)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
        {
            return null;
        }

        order.CustomerName = customerName;
        await db.SaveChangesAsync();

        return await GetOrderByIdAsync(id) ?? order;
    }

    public async Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity)
    {
        var orderExists = await db.Orders.AnyAsync(o => o.Id == orderId);
        var productExists = await db.Products.AnyAsync(p => p.Id == productId);

        if (!orderExists || !productExists)
        {
            return false;
        }

        var existing = await db.OrderProducts
            .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

        if (existing is not null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            db.OrderProducts.Add(new OrderProduct
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity
            });
        }

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProductFromOrderAsync(int orderId, int productId)
    {
        var orderProduct = await db.OrderProducts
            .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

        if (orderProduct is null)
        {
            return false;
        }

        db.OrderProducts.Remove(orderProduct);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await db.Orders
            .Include(o => o.OrderProducts)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
        {
            return false;
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return true;
    }
}
