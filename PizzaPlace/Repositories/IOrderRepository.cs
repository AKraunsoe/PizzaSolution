using PizzaPlace.Models;

namespace PizzaPlace.Repositories;

public interface IOrderRepository
{
    Task<long> PlaceOrder(PizzaOrder order);
    Task UpdateOrderStatus(long id);
    Task<CustomerOrder> GetOrder(long id);
    Task<string> GetOrderStatus(long id);
}
