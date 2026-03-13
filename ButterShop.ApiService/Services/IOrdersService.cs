using ButterShop.ApiService.Models;

namespace ButterShop.ApiService.Services;

public interface IOrdersService
{
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(Order order);
    Task<Order?> UpdateOrderAsync(int id, string customerName);
    Task<bool> AddProductToOrderAsync(int orderId, int productId, int quantity);
    Task<bool> RemoveProductFromOrderAsync(int orderId, int productId);
    Task<bool> DeleteOrderAsync(int id);
}
