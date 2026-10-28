using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public record CreatePizzaRecipeRequest(
    PizzaRecipeType RecipeType,
    ComparableList<CreateRecipeIngredientRequest> Ingredients,
    int CookingTimeMinutes);
