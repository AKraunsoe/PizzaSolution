using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class RecipeService(IRecipeRepository recipeRepository) : IRecipeService
{
    public async Task<ComparableList<PizzaRecipeDto>> GetPizzaRecipes(PizzaOrder order)
    {
        var recipeIds = order.RequestedOrder
            .Select(x => x.RecipeId)
            .Distinct()
            .ToList();

        ComparableList<PizzaRecipeDto> recipes = [];
        foreach (var recipeId in recipeIds)
        {
            recipes.Add(await recipeRepository.GetRecipe(recipeId));
        }

        return recipes;
    }

    public Task<ComparableList<PizzaRecipeDto>> GetRecipes() =>
        recipeRepository.GetRecipes();

    public async Task<PizzaRecipeDto> AddRecipe(PizzaRecipeDto recipe)
    {
        var created = await recipeRepository.AddRecipe(recipe);
        if (created > 0)
        {
            return recipe with { Id = created };
        }
        throw new PizzaException("Not possible to AddRecipe");
    }

    public async Task<PizzaRecipeDto> UpdateRecipe(PizzaRecipeDto recipe)
    {
        var updated = await recipeRepository.UpdateRecipe(recipe);
        if (updated > 0)
        {
            return recipe with { Id = updated };
        }
        throw new PizzaException("Not possible to UpdateRecipe");
    }
}
