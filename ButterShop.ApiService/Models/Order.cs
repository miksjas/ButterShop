namespace ButterShop.ApiService.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string CustomerName { get; set; } = string.Empty;

    public List<OrderProduct> OrderProducts { get; set; } = new();
}
