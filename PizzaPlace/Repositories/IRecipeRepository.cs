using PizzaPlace.Models;

namespace PizzaPlace.Repositories;

public interface IRecipeRepository
{
    Task<long> AddRecipe(PizzaRecipeDto recipe);
    Task<PizzaRecipeDto> GetRecipe(long id);
    Task<ComparableList<PizzaRecipeDto>> GetRecipes();

    Task<long> UpdateRecipe(PizzaRecipeDto recipe);
}
