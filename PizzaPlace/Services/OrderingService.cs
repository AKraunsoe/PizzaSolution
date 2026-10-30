using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IPizzaOven pizzaOven) : IOrderingService
{
    public async Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order)
    {
        var recipes = await recipeService.GetPizzaRecipes(order);
        var stock = await stockService.GetStock(order, recipes);
        if (await stockService.HasInsufficientStock(order, recipes, stock))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        await stockService.TakeStock(stock);
        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.RecipeId)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        return await pizzaOven.PreparePizzas(prepareOrder);

        PizzaRecipeDto GetPizzaRecipe(long recipeId, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.Id == recipeId) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for id {recipeId} which was expected.");
    }
}
