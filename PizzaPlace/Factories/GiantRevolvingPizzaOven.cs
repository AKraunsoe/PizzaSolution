using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var batches = recipeOrders.GroupBy(order => order.Recipe.CookingTimeMinutes).SelectMany(group => group.Chunk(Capacity));

        BatchPizzas(batches);

    }

    private async void BatchPizzas(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)[]> batches){
        var tasks = new List<Task>();
        foreach (var batch in batches)
        {
            foreach (var (recipe, orderGuid) in batch)
            {
                var pizzaTask = MakePizza(recipe);
                _pizzaQueue.Enqueue((pizzaTask, orderGuid));
                tasks.Add(Wrap(pizzaTask(), orderGuid));
            }
            if(tasks.Count != Capacity){
                await Task.WhenAll(tasks);
            }
            tasks = [];
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(recipe.CookingTimeMinutes);

        return GetPizza(recipe.RecipeType);
    };
}
