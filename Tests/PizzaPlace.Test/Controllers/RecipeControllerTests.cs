using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class RecipeControllerTests
{
    private readonly Mock<IRecipeService> recipeService;
    private readonly RecipeController controller;

    public RecipeControllerTests()
    {
        recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        controller = new RecipeController(recipeService.Object);
    }

    [TestMethod]
    public async Task AddRecipe_RecipeError()
    {
        // Arrange
        const string invalidRecipeType = "InvalidRecipe";
        controller.ModelState.SetModelValue(
            "recipeType",
            new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(
                new Microsoft.Extensions.Primitives.StringValues(invalidRecipeType),
                System.Globalization.CultureInfo.InvariantCulture));
        controller.ModelState.AddModelError("recipeType", "Invalid recipe type.");
        var request = new CreatePizzaRecipeRequest(
            default,
            new ComparableList<CreateRecipeIngredientRequest>(){
                new(default, 1)
            },
            1);

        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequest = (BadRequestObjectResult)result;
        var errors = (List<object>)badRequest.Value!;
        Assert.AreEqual(1, errors.Count);

        var error = errors[0];
        var message = error.GetType().GetProperty("message")!.GetValue(error);
        var possibleRecipeTypes = error.GetType().GetProperty("possibleRecipeTypes")!.GetValue(error);

        Assert.AreEqual($"Invalid recipeType '{invalidRecipeType}'.", message);
        Assert.AreEqual(
            $"Possible values you can use: {string.Join(", ", Enum.GetNames<PizzaRecipeType>())}",
            possibleRecipeTypes);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AddRecipe_StockTypeError()
    {
        const string invalidStockType = "InvalidStock";
        controller.ModelState.SetModelValue(
            "stockType",
            new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(
                new Microsoft.Extensions.Primitives.StringValues(invalidStockType),
                System.Globalization.CultureInfo.InvariantCulture));
        controller.ModelState.AddModelError("stockType", "Invalid stock type.");
        var request = new CreatePizzaRecipeRequest(
            PizzaRecipeType.EmptyPizza,
            new ComparableList<CreateRecipeIngredientRequest>
            {
                new(default, 1)
            },
            1);

        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequest = (BadRequestObjectResult)result;
        var errors = (List<object>)badRequest.Value!;
        Assert.AreEqual(1, errors.Count);

        var error = errors[0];
        var message = error.GetType().GetProperty("message")!.GetValue(error);
        var possibleStockTypes = error.GetType().GetProperty("possibleRecipeTypes")!.GetValue(error);

        Assert.AreEqual($"Invalid stockType '{invalidStockType}'.", message);
        Assert.AreEqual(
            $"Possible values you can use: {string.Join(", ", Enum.GetNames<StockType>())}",
            possibleStockTypes);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AddRecipe_SecondIngredientStockTypeError()
    {
        const string invalidStockType = "InvalidStock";
        controller.ModelState.SetModelValue(
            "Ingredients[1].StockType",
            new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(
                new Microsoft.Extensions.Primitives.StringValues(invalidStockType),
                System.Globalization.CultureInfo.InvariantCulture));
        controller.ModelState.AddModelError("Ingredients[1].StockType", "Invalid stock type.");
        var request = new CreatePizzaRecipeRequest(
            PizzaRecipeType.EmptyPizza,
            new ComparableList<CreateRecipeIngredientRequest>
            {
                new(StockType.Dough, 1),
                new(default, 1)
            },
            1);

        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        Assert.AreEqual(1, errors.Count);
        var error = errors[0];
        var message = error.GetType().GetProperty("message")!.GetValue(error);
        var possibleStockTypes = error.GetType().GetProperty("possibleRecipeTypes")!.GetValue(error);
        Assert.AreEqual($"Invalid stockType '{invalidStockType}'.", message);
        Assert.AreEqual(
            $"Possible values you can use: {string.Join(", ", Enum.GetNames<StockType>())}",
            possibleStockTypes);
        recipeService.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task AddRecipe_AmountMustBeGreaterThanZero(int amount)
    {
        // Arrange
        var ingredient = new CreateRecipeIngredientRequest(StockType.Dough, amount);
        controller.ModelState.AddModelError("Ingredients[0].Amount", "Amount must be at least 1.");

        var request = new CreatePizzaRecipeRequest(
            PizzaRecipeType.StandardPizza,
            [ingredient],
            1);

        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        var errorMessage = errors[0].GetType().GetProperty("message")!.GetValue(errors[0]);
        Assert.AreEqual($"Amount for {ingredient.StockType} has to be greater than 0.", errorMessage);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AddRecipe_SecondIngredientAmountMustBeGreaterThanZero()
    {
        // Arrange
        var ingredient = new CreateRecipeIngredientRequest(StockType.Bacon, 0);
        controller.ModelState.AddModelError("Ingredients[1].Amount", "Amount must be at least 1.");

        var request = new CreatePizzaRecipeRequest(
            PizzaRecipeType.StandardPizza,
            [
                new CreateRecipeIngredientRequest(StockType.Dough, 1),
                ingredient
            ],
            1);

        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        Assert.AreEqual(1, errors.Count);
        var errorMessage = errors[0].GetType().GetProperty("message")!.GetValue(errors[0]);
        Assert.AreEqual($"Amount for {ingredient.StockType} has to be greater than 0.", errorMessage);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task AddRecipe_Success()
    {
        // Arrange
        var request = new CreatePizzaRecipeRequest(
            PizzaRecipeType.StandardPizza,
            [new CreateRecipeIngredientRequest(StockType.Dough, 1)],
            5);
        var expectedRecipe = new PizzaRecipeDto(
            PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 1)],
            5,
            42);
        recipeService.Setup(service => service.AddRecipe(It.Is<PizzaRecipeDto>(recipe =>
                recipe.RecipeType == expectedRecipe.RecipeType
                && recipe.Ingredients.Equals(expectedRecipe.Ingredients)
                && recipe.CookingTimeMinutes == expectedRecipe.CookingTimeMinutes)))
            .ReturnsAsync(expectedRecipe);
        // Act
        var result = await controller.AddPizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        var actualRecipe = response.GetType().GetProperty("recipe")!.GetValue(response);
        Assert.AreEqual(expectedRecipe, actualRecipe);
        recipeService.VerifyAll();
    }

    [TestMethod]
    public async Task UpdateRecipe_IdMustBeGreaterThanZero()
    {
        // Arrange
        var recipe = new UpdatePizzaRequest(
            PizzaRecipeType.StandardPizza,
            new ComparableList<IngredientRequest>(){
                new(default, 1)
            },
            5,
            0);

        // Act
        var result = await controller.UpdatePizzaRecipe(recipe);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        Assert.AreEqual(1, errors.Count);
        var errorMessage = errors[0].GetType().GetProperty("message")!.GetValue(errors[0]);
        Assert.AreEqual("Recipe id must be greater than 0.", errorMessage);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task UpdateRecipe_InvalidRecipeType()
    {
        // Arrange
        const string invalidRecipeType = "InvalidRecipe";
        controller.ModelState.SetModelValue(
            "RecipeType",
            new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(
                new Microsoft.Extensions.Primitives.StringValues(invalidRecipeType),
                System.Globalization.CultureInfo.InvariantCulture));
        controller.ModelState.AddModelError("RecipeType", "Invalid recipe type.");
        var recipe = new UpdatePizzaRequest(
            default,
            new ComparableList<IngredientRequest>(){
                new(default, 1)
            },
            5,
            42);

        // Act
        var result = await controller.UpdatePizzaRecipe(recipe);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        Assert.AreEqual(1, errors.Count);
        var message = errors[0].GetType().GetProperty("message")!.GetValue(errors[0]);
        Assert.AreEqual($"Invalid recipeType '{invalidRecipeType}'.", message);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task UpdateRecipe_InvalidStockType()
    {
        // Arrange
        const string invalidStockType = "InvalidStock";
        controller.ModelState.SetModelValue(
            "Ingredients[0].StockType",
            new Microsoft.AspNetCore.Mvc.ModelBinding.ValueProviderResult(
                new Microsoft.Extensions.Primitives.StringValues(invalidStockType),
                System.Globalization.CultureInfo.InvariantCulture));
        controller.ModelState.AddModelError("Ingredients[0].StockType", "Invalid stock type.");
        var recipe = new UpdatePizzaRequest(
            PizzaRecipeType.StandardPizza,
            [new IngredientRequest(default, 1)],
            5,
            42);

        // Act
        var result = await controller.UpdatePizzaRecipe(recipe);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var errors = (List<object>)((BadRequestObjectResult)result).Value!;
        Assert.AreEqual(1, errors.Count);
        var error = errors[0];
        var message = error.GetType().GetProperty("message")!.GetValue(error);
        var possibleStockTypes = error.GetType().GetProperty("possibleRecipeTypes")!.GetValue(error);
        Assert.AreEqual($"Invalid stockType '{invalidStockType}'.", message);
        Assert.AreEqual(
            $"Possible values you can use: {string.Join(", ", Enum.GetNames<StockType>())}",
            possibleStockTypes);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task UpdateRecipe_Success()
    {
        // Arrange
        var request = new UpdatePizzaRequest(
            PizzaRecipeType.StandardPizza,
            [new IngredientRequest(StockType.Dough, 2)],
            5,
            42);
        var recipe = new PizzaRecipeDto(
            request.RecipeType,
            [new StockDto(StockType.Dough, 2)],
            request.CookingTimeMinutes,
            request.Id);
        recipeService.Setup(service => service.UpdateRecipe(recipe))
            .ReturnsAsync(recipe);

        // Act
        var result = await controller.UpdatePizzaRecipe(request);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        var actualRecipe = response.GetType().GetProperty("recipe")!.GetValue(response);
        Assert.AreEqual(recipe, actualRecipe);
        recipeService.VerifyAll();
    }

    [TestMethod]
    public async Task GetRecipe_NotFound_ThrowsError()
    {
        const long recipeId = 999;
        var exception = new PizzaException($"Recipe does not exist with id {recipeId}.");
        recipeService.Setup(service => service.GetRecipe(recipeId))
            .ThrowsAsync(exception);

        var actual = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => controller.GetRecipe(recipeId));

        Assert.AreSame(exception, actual);
        recipeService.VerifyAll();
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public async Task GetRecipe_IdMustBePositive_ReturnsBadRequest(long recipeId)
    {
        var result = await controller.GetRecipe(recipeId);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var response = ((BadRequestObjectResult)result).Value!;
        var message = response.GetType().GetProperty("message")!.GetValue(response);
        Assert.AreEqual("ID has to be a positive integer", message);
        recipeService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task GetRecipe_Found_ReturnsRecipe()
    {
        const long recipeId = 42;
        var expectedRecipe = new PizzaRecipeDto(
            PizzaRecipeType.StandardPizza,
            [new StockDto(StockType.Dough, 2)],
            5,
            recipeId);
        recipeService.Setup(service => service.GetRecipe(recipeId))
            .ReturnsAsync(expectedRecipe);

        var result = await controller.GetRecipe(recipeId);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        var actualRecipe = response.GetType().GetProperty("recipe")!.GetValue(response);
        Assert.AreEqual(expectedRecipe, actualRecipe);
        recipeService.VerifyAll();
    }

    [TestMethod]
    public async Task GetRecipes_ReturnsRecipeList()
    {
        ComparableList<PizzaRecipeDto> expectedRecipes =
        [
            new PizzaRecipeDto(
                PizzaRecipeType.StandardPizza,
                [new StockDto(StockType.Dough, 2)],
                5,
                42),
            new PizzaRecipeDto(
                PizzaRecipeType.EmptyPizza,
                [new StockDto(StockType.Tomatoes, 3)],
                8,
                43)
        ];
        recipeService.Setup(service => service.GetRecipes())
            .ReturnsAsync(expectedRecipes);

        var result = await controller.GetRecipes();

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        var actualRecipes = response.GetType().GetProperty("recipes")!.GetValue(response);
        Assert.AreEqual(expectedRecipes, actualRecipes);
        recipeService.VerifyAll();
    }

    [TestMethod]
    public async Task GetRecipes_ReturnsEmptyListWhenNoRecipesFound()
    {
        ComparableList<PizzaRecipeDto> expectedRecipes = [];
        recipeService.Setup(service => service.GetRecipes())
            .ReturnsAsync(expectedRecipes);

        var result = await controller.GetRecipes();

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        var actualRecipes = response.GetType().GetProperty("recipes")!.GetValue(response);
        Assert.AreEqual(expectedRecipes, actualRecipes);
        recipeService.VerifyAll();
    }
}
