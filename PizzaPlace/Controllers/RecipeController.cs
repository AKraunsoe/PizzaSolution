using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/recipe")]
public class RecipeController(
    IRecipeService recipeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddPizzaRecipe([FromBody] CreatePizzaRecipeRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                        .SelectMany(value => value.Errors)
                        .Select(error => error.ErrorMessage);

            return BadRequest();
        }

        var recipe = new PizzaRecipeDto(
            request.RecipeType,
            new ComparableList<StockDto>(request.Ingredients
                .Select(ingredient => new StockDto(ingredient.StockType, ingredient.Amount))),
            request.CookingTimeMinutes);

        return Ok(new
        {
            recipe = await recipeService.AddRecipe(recipe)
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePizzaRecipe([FromBody] PizzaRecipeDto recipe)
    {
        return Ok(new
        {
            recipe = await recipeService.UpdateRecipe(recipe)
        });
    }
}
