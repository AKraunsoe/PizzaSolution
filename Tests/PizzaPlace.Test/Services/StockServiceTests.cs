using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTests
{
    private readonly Mock<IStockRepository> stockRepository;
    private readonly StockService stockService;

    public StockServiceTests()
    {
        stockRepository = new();
        stockService = new StockService(stockRepository.Object);
    }

    [TestMethod]
    public async Task GetStock_NoRecipeError()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 2);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
           .ReturnsAsync(new StockDto(StockType.Dough, 4));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
           .ReturnsAsync(new StockDto(StockType.Tomatoes, 4));

        await Assert.ThrowsExceptionAsync<NullReferenceException>(() => stockService.GetStock(order, recipes));

    }

    [TestMethod]
    public async Task GetStock_NoStockError()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        await Assert.ThrowsExceptionAsync<NullReferenceException>(() => stockService.GetStock(order, recipes));

    }

    [TestMethod]
    public async Task GetStock_GetStockNone()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.BellPeppers, 2),
                new StockDto(StockType.GenericSpices, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.BellPeppers))
        .ReturnsAsync(new StockDto(StockType.BellPeppers, 0));

        stockRepository.Setup(x => x.GetStock(StockType.GenericSpices))
        .ReturnsAsync(new StockDto(StockType.GenericSpices, 0));

        var result = await stockService.GetStock(order, recipes);
        var expected = new ComparableList<StockDto> { new StockDto(StockType.BellPeppers, 0), new StockDto(StockType.GenericSpices, 0) };

        Assert.AreEqual(expected, result);

    }

    [TestMethod]
    public async Task GetStock_GetStockFound()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 2));

        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 1));

        var result = await stockService.GetStock(order, recipes);

        var expected = new ComparableList<StockDto> { new StockDto(StockType.Dough, 2), new StockDto(StockType.Tomatoes, 1) };

        Assert.AreEqual(expected, result);

    }

    [TestMethod]
    public async Task GetStock_RecipeNotFound()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 2);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        await Assert.ThrowsExceptionAsync<NullReferenceException>(
            () => stockService.GetStock(order, recipes));

    }

    [TestMethod]
    public async Task HasInSufficientStock_InsufficientForOne()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 300),
                new StockDto(StockType.Tomatoes, 200),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 4));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 4));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsTrue(result);

    }

    [TestMethod]
    public async Task HasInSufficientStock_InSufficientForTwo()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 2)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Bacon, 2),
                new StockDto(StockType.UnicornDust, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Bacon))
        .ReturnsAsync(new StockDto(StockType.Bacon, 2));

        stockRepository.Setup(x => x.GetStock(StockType.UnicornDust))
        .ReturnsAsync(new StockDto(StockType.UnicornDust, 1));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsTrue(result);

    }

    [TestMethod]
    public async Task HasInSufficientStock_sufficientForOne()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 2));

        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 1));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsFalse(result);

    }

    [TestMethod]
    public async Task HasInSufficientStock_sufficientForTwo()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 2)
        ];
        var order = new PizzaOrder(requests);
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        ComparableList<PizzaRecipeDto> recipes = [recipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 4));

        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 2));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsFalse(result);

    }

    [TestMethod]
    public async Task HasInSufficientStock_sufficientForTwoTypes()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 2),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2, 1)
        ];
        var order = new PizzaOrder(requests);
        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.Bacon, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 2);
        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
        .ReturnsAsync(new StockDto(StockType.Dough, 4));

        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
        .ReturnsAsync(new StockDto(StockType.Tomatoes, 4));

        stockRepository.Setup(x => x.GetStock(StockType.Bacon))
        .ReturnsAsync(new StockDto(StockType.Bacon, 4));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsFalse(result);

    }

    [TestMethod]
    public async Task HasInSufficientStock_inSufficientForTwoTypes()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 2),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2, 1)
        ];
        var order = new PizzaOrder(requests);
        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.Bacon, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 2);
        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];

        stockRepository.Setup(x => x.GetStock(StockType.Dough))
            .ReturnsAsync(new StockDto(StockType.Dough, 4));
        stockRepository.Setup(x => x.GetStock(StockType.Bacon))
            .ReturnsAsync(new StockDto(StockType.Bacon, 2));
        stockRepository.Setup(x => x.GetStock(StockType.Tomatoes))
            .ReturnsAsync(new StockDto(StockType.Tomatoes, 2));

        var stock = await stockService.GetStock(order, recipes);
        var result = await stockService.HasInsufficientStock(order, recipes, stock);

        Assert.IsTrue(result);

    }

}