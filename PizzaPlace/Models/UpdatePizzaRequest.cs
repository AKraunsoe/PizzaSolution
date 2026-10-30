using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public record UpdatePizzaRequest(
    PizzaRecipeType RecipeType,
    ComparableList<IngredientRequest> Ingredients,
    int CookingTimeMinutes,
    long Id = 0);
