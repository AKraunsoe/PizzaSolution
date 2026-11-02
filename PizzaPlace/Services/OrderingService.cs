using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Pizzas;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven,
    IOrderRepository orderRepository) : IOrderingService
{

    private Dictionary<long, Task> pizzaTasks = new();

    public async Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order, long orderId, ComparableList<StockDto> requiredStock, ComparableList<PizzaRecipeDto> recipes)
    {
        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.RecipeId)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        await stockService.TakeStock(requiredStock);
        var prep = pizzaOven.PreparePizzas(prepareOrder);
        pizzaTasks.Add(orderId, prep);
        var pizzas = await prep;
        await orderRepository.UpdateOrderStatus(orderId);
        pizzaTasks.Remove(orderId);
        Console.WriteLine("Pizzas Done");
        return pizzas;

        PizzaRecipeDto GetPizzaRecipe(long recipeId, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.Id == recipeId) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for id {recipeId} which was expected.");
    }

    public async Task<(ComparableList<PizzaRecipeDto>, ComparableList<StockDto>)> PizzaCreationPossible(PizzaOrder order){
        var recipes = await recipeService.GetPizzaRecipes(order);
        var requiredStock = stockService.GetRequiredStock(order, recipes);
        if (await stockService.HasInsufficientStock(order, recipes, requiredStock))
            throw new PizzaException("Unable to take in order. Insufficient stock.");
        return (recipes, requiredStock);
    }

    public async Task<long> CreatePizzaOrder(PizzaOrder order)
    {
        var created = await orderRepository.PlaceOrder(order);
        if (created > 0)
        {
            return created;
        }
        throw new PizzaException("Not possible to place Order");
    }

    public async Task<string> GetOrderStatus(long id)
    {
        string status = await orderRepository.GetOrderStatus(id);
        if (string.IsNullOrEmpty(status))
        {
            throw new PizzaException("Order not found");
        }
        return status;
    }

    public async Task<CustomerOrder> GetOrder(long id)
    {
        var order = await orderRepository.GetOrder(id);
        if (order != null)
        {
            return order;
        }
        throw new PizzaException("Order not found");
    }

}
