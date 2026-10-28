using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class RecipeServiceTests
{
    private static RecipeService GetService(Mock<IRecipeRepository> recipeRepository) =>
        new(recipeRepository.Object);

    [TestMethod]
    public async Task GetPizzaRecipes()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 11, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 12, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 11, 20),
        ]);
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1, 11);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100, 12);
        ComparableList<PizzaRecipeDto> expected = [rareRecipe, oddRecipe];

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.GetRecipe(11))
            .ReturnsAsync(rareRecipe);
        recipeRepository.Setup(x => x.GetRecipe(12))
            .ReturnsAsync(oddRecipe);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.GetPizzaRecipes(order);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task AddPizzaRecipe_Success()
    {
        // Arrange
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.AddRecipe(rareRecipe))
            .ReturnsAsync(1);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.AddRecipe(rareRecipe);

        // Assert
        Assert.AreEqual(rareRecipe with { Id = 1 }, actual);
    }
    [TestMethod]
    public async Task AddPizzaRecipe_Error()
    {
        // Arrange
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.AddRecipe(rareRecipe))
            .ReturnsAsync(0);

        var service = GetService(recipeRepository);

        // Act
        await Assert.ThrowsExceptionAsync<PizzaException>(() => service.AddRecipe(rareRecipe));

    }

    [TestMethod]
    public async Task UpdatePizzaRecipe_Success()
    {
        // Arrange
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1, 42);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.UpdateRecipe(rareRecipe))
            .ReturnsAsync(42);

        var service = GetService(recipeRepository);

        // Act
        var actual = await service.UpdateRecipe(rareRecipe);

        // Assert
        Assert.AreEqual(rareRecipe, actual);
    }

    [TestMethod]
    public async Task UpdatePizzaRecipe_Error()
    {
        // Arrange
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);

        var recipeRepository = new Mock<IRecipeRepository>(MockBehavior.Strict);
        recipeRepository.Setup(x => x.UpdateRecipe(rareRecipe))
            .ReturnsAsync(0);

        var service = GetService(recipeRepository);

        // Act
        await Assert.ThrowsExceptionAsync<PizzaException>(() => service.UpdateRecipe(rareRecipe));
    }
}
