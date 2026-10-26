using PizzaPlace.Models;

namespace PizzaPlace.Services;

public interface IRecipeService
{
    Task<ComparableList<PizzaRecipeDto>> GetPizzaRecipes(PizzaOrder order);
    Task<PizzaRecipeDto> AddRecipe(PizzaRecipeDto recipe);

    Task<PizzaRecipeDto> UpdateRecipe(PizzaRecipeDto recipe);
}
