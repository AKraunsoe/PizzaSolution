using PizzaPlace.Models.Types;
using System.ComponentModel.DataAnnotations;

namespace PizzaPlace.Models;

public record CreatePizzaRecipeRequest(
    PizzaRecipeType RecipeType,
    [param: MinLength(1)]
    ComparableList<CreateRecipeIngredientRequest> Ingredients,
    int CookingTimeMinutes);
