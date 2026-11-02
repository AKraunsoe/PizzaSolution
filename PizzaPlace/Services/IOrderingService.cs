using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order, long orderId, ComparableList<StockDto> requiredStock, ComparableList<PizzaRecipeDto> recipes);
    Task<long> CreatePizzaOrder(PizzaOrder order);
    Task<string> GetOrderStatus(long id);
    Task<CustomerOrder> GetOrder(long id);
    Task<(ComparableList<PizzaRecipeDto>, ComparableList<StockDto>)> PizzaCreationPossible(PizzaOrder order);

}
