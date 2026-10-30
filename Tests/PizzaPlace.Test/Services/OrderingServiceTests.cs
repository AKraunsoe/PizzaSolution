using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class OrderingServiceTests
{
    private static OrderingService GetService(IStockService stockService, IRecipeService recipeService, IPizzaOven pizzaOven) =>
        new OrderingService(stockService, recipeService, pizzaOven);

    [TestMethod]
    public async Task HandlePizzaOrder()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 54),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2, 2),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 0),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 4),
        ];
        var order = new PizzaOrder(requests);
        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10, 1);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15, 2);
        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];
        ComparableList<PizzaPrepareOrder> prepareOrders =
        [
            new PizzaPrepareOrder(standardRecipe, 58),
            new PizzaPrepareOrder(tastyRecipe, 2),
        ];
        var pizzas = new List<Pizza>{
            new StandardPizza(),
            new ExtremelyTastyPizza(),
        };
        ComparableList<StockDto> stock =
        [
            new StockDto(StockType.Dough, 100),
            new StockDto(StockType.Tomatoes, 100),
            new StockDto(StockType.UnicornDust, 100),
            new StockDto(StockType.BellPeppers, 100),
        ];

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(false);
        stockService.Setup(x => x.TakeStock(stock))
            .ReturnsAsync(stock);
        pizzaOven.Setup(x => x.PreparePizzas(prepareOrders))
            .ReturnsAsync(pizzas);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object);

        // Act
        var actual = await service.HandlePizzaOrder(order);

        // Assert
        CollectionAssert.AreEqual(pizzas, actual.ToList());
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_InsufficientStock()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2, 2),
        ];
        var order = new PizzaOrder(requests);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15, 2);
        ComparableList<PizzaRecipeDto> recipes = [tastyRecipe];

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        ComparableList<StockDto> stock = [];
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(true);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order));

        // Assert
        Assert.AreEqual("Unable to take in order. Insufficient stock.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_BrokenRecipeServiceDoesNotReturnRecipe()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1, 1),
        ];
        var order = new PizzaOrder(requests);
        ComparableList<PizzaRecipeDto> recipes = [];

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        ComparableList<StockDto> stock = [];
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(false);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order));

        // Assert
        Assert.AreEqual("Missing recipe. Recipe service did not return a recipe for id 1 which was expected.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }
}
