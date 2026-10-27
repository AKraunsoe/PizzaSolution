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
    private static long StandardRecipeId { get; set; }

    [TestMethod]
    public async Task AddRecipe()
    {
        // Arrange
        if (StandardRecipeId > 0)
            return;

        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), StandardCookingTime);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.AddRecipe(recipe)).ReturnsAsync(1);
        // Act
        var actual = await repository.Object.AddRecipe(recipe);

        // Assert
        Assert.IsTrue(actual > 0, "Recipe has an id.");
        StandardRecipeId = actual;
        repository.Verify(x => x.AddRecipe(recipe), Times.Once);
    }

    [TestMethod]
    public async Task AddRecipe_AlreadyAdded()
    {
        // Arrange
        await AddRecipe();
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [new StockDto(StockType.UnicornDust, 123), new StockDto(StockType.Anchovies, 1)], StandardCookingTime);
        var repository = GetRecipeRepository();
        repository.Setup(x => x.AddRecipe(recipe))
        .ThrowsAsync(new PizzaException("Recipe already added for StandardPizza."));
        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.Object.AddRecipe(recipe));

        // Assert
        Assert.AreEqual("Recipe already added for StandardPizza.", ex.Message);
        repository.Verify(x => x.AddRecipe(recipe), Times.Once);
    }

    [TestMethod]
    public async Task UpdateRecipe_RecipeFound()
    {
        // Arrange
        await AddRecipe();
        if (StandardRecipeId > 0)
            return;

        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), 25);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.UpdateRecipe(recipe)).ReturnsAsync(1);

        // Act
        var actual = await repository.Object.UpdateRecipe(recipe);

        // Assert
        Assert.IsTrue(actual > 0, "Recipe has an id.");
        repository.Verify(x => x.UpdateRecipe(recipe),
        Times.Once);
    }

    [TestMethod]
    public async Task UpdateRecipe_RecipeNotFound()
    {
        // Arrange
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [new StockDto(StockType.UnicornDust, 123), new StockDto(StockType.Anchovies, 1)], StandardCookingTime);
        var repository = GetRecipeRepository();
        repository.Setup(x => x.UpdateRecipe(recipe))
         .ThrowsAsync(new PizzaException("Recipe does not exists of type StandardPizza."));

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.Object.UpdateRecipe(recipe));

        // Assert
        Assert.AreEqual("Recipe does not exists of type StandardPizza.", ex.Message);
        repository.Verify(x => x.UpdateRecipe(recipe),
        Times.Once);
    }

    [TestMethod]
    public async Task GetRecipe()
    {
        // Arrange
        var pizzaType = PizzaRecipeType.StandardPizza;
        await AddRecipe();
        var expected = new PizzaRecipeDto(pizzaType, GetStandardIngredients(), StandardCookingTime, StandardRecipeId);
        var repository = GetRecipeRepository();
        repository.Setup(x =>
        x.GetRecipe(pizzaType)).ReturnsAsync(expected);
        // Act
        var actual = await repository.Object.GetRecipe(pizzaType);

        // Assert
        Assert.AreEqual(expected, actual);
        repository.Verify(x => x.GetRecipe(pizzaType),
        Times.Once);
    }

    [TestMethod]
    public async Task GetRecipe_DoesNotExist()
    {
        // Arrange
        var pizzaType = PizzaRecipeType.ExtremelyTastyPizza;
        var repository = GetRecipeRepository();
        repository.Setup(x => x.GetRecipe(pizzaType))
        .ThrowsAsync(new PizzaException("Recipe does not exists of type ExtremelyTastyPizza."));

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.Object.GetRecipe(pizzaType));

        // Assert
        Assert.AreEqual("Recipe does not exists of type ExtremelyTastyPizza.", ex.Message);
        repository.Verify(x => x.GetRecipe(pizzaType),
        Times.Once);
    }
}
