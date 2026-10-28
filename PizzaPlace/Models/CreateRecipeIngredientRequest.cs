using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public record CreateRecipeIngredientRequest(StockType StockType, int Amount);
