using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public record IngredientRequest(
    StockType StockType,
    [property: DefaultValue(1)]
    [Range(1, int.MaxValue)]
    int Amount);
