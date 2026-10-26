using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/recipe")]
public class RecipeController(
    IRecipeService recipeService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddPizzaRecipe([FromBody] PizzaRecipeDto recipe)
    {
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

