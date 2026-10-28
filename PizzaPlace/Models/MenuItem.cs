using PizzaPlace.Models.Types;

namespace PizzaPlace;

public record MenuItem(string Description, long RecipeId, PizzaRecipeType PizzaType, double Price, bool SoldOut = false);
