using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
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
        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(order, recipes);

        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.RecipeId)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        return await pizzaOven.PreparePizzas(prepareOrder, stock);

        PizzaRecipeDto GetPizzaRecipe(long recipeId, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.Id == recipeId) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for id {recipeId} which was expected.");
    }
}
