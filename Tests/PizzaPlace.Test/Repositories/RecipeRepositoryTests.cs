using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class RecipeRepositoryTests
{
    private static Mock<IRecipeRepository> GetRecipeRepository() => new Mock<IRecipeRepository>(MockBehavior.Strict);

    private static ComparableList<StockDto> GetStandardIngredients() =>
    [
        new StockDto(StockType.Dough, 3),
        new StockDto(StockType.Tomatoes, 10),
        new StockDto(StockType.Bacon, 2),
    ];
    private const int StandardCookingTime = 19;
    private const long StandardRecipeId = 1;

    [TestMethod]
    public async Task AddRecipe()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), StandardCookingTime);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.AddRecipe(recipe)).ReturnsAsync(StandardRecipeId);
        // Act
        var actual = await repository.Object.AddRecipe(recipe);

        // Assert
        Assert.AreEqual(StandardRecipeId, actual);
        repository.Verify(x => x.AddRecipe(recipe), Times.Once);
    }

    [TestMethod]
    public async Task AddRecipe_AllowsDuplicateRecipeType()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [new StockDto(StockType.UnicornDust, 123), new StockDto(StockType.Anchovies, 1)], StandardCookingTime);
        var repository = GetRecipeRepository();
        repository.Setup(x => x.AddRecipe(recipe))
            .ReturnsAsync(2);

        // Act
        var actual = await repository.Object.AddRecipe(recipe);

        // Assert
        Assert.AreEqual(2, actual);
        repository.Verify(x => x.AddRecipe(recipe), Times.Once);
    }

    [TestMethod]
    public async Task UpdateRecipe_RecipeFound()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), 25, StandardRecipeId);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.UpdateRecipe(recipe)).ReturnsAsync(StandardRecipeId);

        // Act
        var actual = await repository.Object.UpdateRecipe(recipe);

        // Assert
        Assert.AreEqual(StandardRecipeId, actual);
        repository.Verify(x => x.UpdateRecipe(recipe),
        Times.Once);
    }

    [TestMethod]
    public async Task UpdateRecipe_RecipeNotFound()
    {
        // Arrange
        const long missingRecipeId = 999;
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [new StockDto(StockType.UnicornDust, 123), new StockDto(StockType.Anchovies, 1)], StandardCookingTime, missingRecipeId);
        var repository = GetRecipeRepository();
        repository.Setup(x => x.UpdateRecipe(recipe))
            .ThrowsAsync(new PizzaException($"Recipe does not exist with id {missingRecipeId}."));

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.Object.UpdateRecipe(recipe));

        // Assert
        Assert.AreEqual($"Recipe does not exist with id {missingRecipeId}.", ex.Message);
        repository.Verify(x => x.UpdateRecipe(recipe),
        Times.Once);
    }

    [TestMethod]
    public async Task GetRecipe()
    {
        // Arrange
        var recipeId = StandardRecipeId;
        var expected = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), StandardCookingTime, recipeId);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.GetRecipe(recipeId)).ReturnsAsync(expected);
        // Act
        var actual = await repository.Object.GetRecipe(recipeId);

        // Assert
        Assert.AreEqual(expected, actual);
        repository.Verify(x => x.GetRecipe(recipeId),
        Times.Once);
    }

    [TestMethod]
    public async Task GetRecipe_DoesNotExist()
    {
        // Arrange
        const long missingRecipeId = 999;
        var repository = GetRecipeRepository();
        repository.Setup(x => x.GetRecipe(missingRecipeId))
            .ThrowsAsync(new PizzaException($"Recipe does not exist with id {missingRecipeId}."));

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.Object.GetRecipe(missingRecipeId));

        // Assert
        Assert.AreEqual($"Recipe does not exist with id {missingRecipeId}.", ex.Message);
        repository.Verify(x => x.GetRecipe(missingRecipeId),
        Times.Once);
    }
}
