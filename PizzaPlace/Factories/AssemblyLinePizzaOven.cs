using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Producing one line of pizza.
/// Taking 7 minutes to setup - and then 5 minutes less for every subsequent pizza of the same recipe type to a minimum of 4 minutes.
/// </summary>
public class AssemblyLinePizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int AssemblyLineCapacity = 1;
    public const int SetupTimeMinutes = 7;
    public const int SubsequentPizzaTimeSavingsInMinutes = 5;
    public const int MinimumCookingTimeMinutes = 4;

    protected override int Capacity => AssemblyLineCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        var delayUpdater = new Dictionary<PizzaRecipeType, int>();
        var cookingTime = 0;
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            if(delayUpdater.TryGetValue(recipe.RecipeType, out var time))
            {
                var newCookingTime = time - SubsequentPizzaTimeSavingsInMinutes;
                if (newCookingTime < MinimumCookingTimeMinutes)
                {
                    cookingTime = MinimumCookingTimeMinutes;
                }
                else
                {
                    cookingTime = newCookingTime;
                }
                delayUpdater[recipe.RecipeType] = cookingTime;
            }else{
                cookingTime = recipe.CookingTimeMinutes + SetupTimeMinutes;
                delayUpdater.Add(recipe.RecipeType, cookingTime);
            }
            _pizzaQueue.Enqueue((MakePizza(recipe, cookingTime), orderGuid));

        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe, int cookingTime) => async () =>
    {
        await CookPizza(cookingTime);

        return GetPizza(recipe.RecipeType);
    };
}
