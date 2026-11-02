namespace PizzaPlace;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

public record PizzaAmount(
    [property: DefaultValue(1)]
    [Range(1, int.MaxValue)]
    long RecipeId,
    [property: DefaultValue(1)]
    [Range(1, int.MaxValue)]
    ushort Amount);
