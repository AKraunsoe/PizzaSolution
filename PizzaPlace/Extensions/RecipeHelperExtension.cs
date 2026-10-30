using Microsoft.AspNetCore.Mvc.ModelBinding;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Extensions;

public static class RecipeHelperExtensions
{
    public static List<object>? GetRecipeValidationErrors(
        this ModelStateDictionary modelState,
        IEnumerable<StockType> ingredientStockTypes)
    {
        var stockTypes = ingredientStockTypes.ToList();
        var badRequestMessage = new List<object>();

        if (stockTypes.Count == 0)
        {
            badRequestMessage.Add(new
            {
                message = "You must include at least 1 ingredient to create your pizza"
            });
        }

        if (modelState.IsValid)
        {
            return badRequestMessage.Count > 0 ? badRequestMessage : null;
        }

        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .Select(entry => new
            {
                Key = entry.Key,
                Value = entry.Value!.AttemptedValue,
                ErrorMessage = entry.Value.Errors.Select(error => error.ErrorMessage).ToList()
            }).ToList();

        foreach (var error in errors)
        {
            var propertyName = error.Key[(error.Key.LastIndexOf('.') + 1)..];
            if (propertyName.Equals("recipeType", StringComparison.OrdinalIgnoreCase))
            {
                badRequestMessage.Add(new
                {
                    message = $"Invalid recipeType '{error.Value}'.",
                    possibleRecipeTypes = $"Possible values you can use: {string.Join(", ", Enum.GetNames<PizzaRecipeType>())}"
                });
            }
            else if (propertyName.Equals("stockType", StringComparison.OrdinalIgnoreCase))
            {
                badRequestMessage.Add(new
                {
                    message = $"Invalid stockType '{error.Value}'.",
                    possibleRecipeTypes = $"Possible values you can use: {string.Join(", ", Enum.GetNames<StockType>())}"
                });
            }
            else if (propertyName.Equals("amount", StringComparison.OrdinalIgnoreCase))
            {
                var ingredientIndex = int.Parse(error.Key.Split('[', ']')[1]);
                var stockType = stockTypes[ingredientIndex];
                badRequestMessage.Add(new
                {
                    message = $"Amount for {stockType} has to be greater than 0."
                });
            }
            else
            {
                badRequestMessage.Add(new
                {
                    message = error.ErrorMessage.FirstOrDefault()
                });
            }
        }

        return badRequestMessage;
    }
}
