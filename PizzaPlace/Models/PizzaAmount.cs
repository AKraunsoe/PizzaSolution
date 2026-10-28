using PizzaPlace.Models.Types;

namespace PizzaPlace;

public record PizzaAmount(PizzaRecipeType PizzaType, long RecipeId, ushort Amount);
