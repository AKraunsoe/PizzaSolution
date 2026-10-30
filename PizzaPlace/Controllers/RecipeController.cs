using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Extensions;
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
        var validationErrors = ModelState.GetRecipeValidationErrors(
            request.Ingredients.Select(ingredient => ingredient.StockType));
        if (validationErrors is not null)
        {
            return BadRequest(validationErrors);
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
    public async Task<IActionResult> UpdatePizzaRecipe([FromBody] UpdatePizzaRequest request)
    {
        if (request.Id <= 0)
        {
            ModelState.AddModelError(nameof(request.Id), "Recipe id must be greater than 0.");
        }

        var validationErrors = ModelState.GetRecipeValidationErrors(
            request.Ingredients.Select(ingredient => ingredient.StockType));
        if (validationErrors is not null)
        {
            return BadRequest(validationErrors);
        }

        var recipe = new PizzaRecipeDto(
            request.RecipeType,
            new ComparableList<StockDto>(request.Ingredients
                .Select(ingredient => new StockDto(ingredient.StockType, ingredient.Amount))),
            request.CookingTimeMinutes,
            request.Id);

        return Ok(new
        {
            recipe = await recipeService.UpdateRecipe(recipe)
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetRecipe(long id)
    {
        if(id <= 0){
            return BadRequest(new { message = "ID has to be a positive integer" });
        }

        return Ok(new
        {
            recipe = await recipeService.GetRecipe(id)
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecipes()
    {
        return Ok(new
        {
            recipes = await recipeService.GetRecipes()
        });
    }
}
