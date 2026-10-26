using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class RecipeService(IRecipeRepository recipeRepository) : IRecipeService
{
    public async Task<ComparableList<PizzaRecipeDto>> GetPizzaRecipes(PizzaOrder order)
    {
        var pizzaTypes = order.RequestedOrder
            .Select(x => x.PizzaType)
            .Distinct()
            .ToList();

        ComparableList<PizzaRecipeDto> recipes = [];
        foreach (var pizzaType in pizzaTypes)
        {
            recipes.Add(await recipeRepository.GetRecipe(pizzaType));
        }

        return recipes;
    }

    public async Task<PizzaRecipeDto> AddRecipe(PizzaRecipeDto recipe)
    {
        var created = await recipeRepository.AddRecipe(recipe);
        if (created > 0) 
        {
            return recipe;
        }
        throw new PizzaException("Not possible to AddRecipe");
    }

    public async Task<PizzaRecipeDto> UpdateRecipe(PizzaRecipeDto recipe)
    {
        var created = await recipeRepository.UpdateRecipe(recipe);
        if (created > 0)
        {
            return recipe;
        }
        throw new PizzaException("Not possible to AddRecipe");
    }
}
