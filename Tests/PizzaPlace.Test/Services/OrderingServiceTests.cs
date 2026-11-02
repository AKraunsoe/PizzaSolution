using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Services;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Services;

[TestClass]
public class OrderingServiceTests
{
    private static OrderingService GetService(IStockService stockService, IRecipeService recipeService, IPizzaOven pizzaOven, IOrderRepository orderRepository) =>
        new OrderingService(stockService, recipeService, pizzaOven, orderRepository);

    private static OrderingService CreateService(Mock<IOrderRepository> orderRepository) =>
        new(
            new Mock<IStockService>().Object,
            new Mock<IRecipeService>().Object,
            new Mock<IPizzaOven>().Object,
            orderRepository.Object);

    [TestMethod]
    public async Task HandlePizzaOrder()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(1, 54),
            new PizzaAmount(2, 2),
            new PizzaAmount(1, 0),
            new PizzaAmount(1, 4),
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

        var stockService = new Mock<IStockService>();
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>();

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.GetRequiredStock(order, recipes))
            .Returns(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(false);
        stockService.Setup(x => x.TakeStock(stock))
            .ReturnsAsync(stock);
        pizzaOven.Setup(x => x.PreparePizzas(prepareOrders))
            .ReturnsAsync(pizzas);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object);

        // Act
        var actual = await service.HandlePizzaOrder(order, 1);

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
            new PizzaAmount(2, 2),
        ];
        var order = new PizzaOrder(requests);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15, 2);
        ComparableList<PizzaRecipeDto> recipes = [tastyRecipe];

        var stockService = new Mock<IStockService>();
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>();

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        ComparableList<StockDto> stock = [];
        stockService.Setup(x => x.GetRequiredStock(order, recipes))
            .Returns(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(true);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order, 1));

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
            new PizzaAmount(1, 1),
        ];
        var order = new PizzaOrder(requests);
        ComparableList<PizzaRecipeDto> recipes = [];

        var stockService = new Mock<IStockService>();
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var orderRepository = new Mock<IOrderRepository>();

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        ComparableList<StockDto> stock = [];
        stockService.Setup(x => x.GetRequiredStock(order, recipes))
            .Returns(stock);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes, stock))
            .ReturnsAsync(false);

        var service = GetService(stockService.Object, recipeService.Object, pizzaOven.Object, orderRepository.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order, 1));

        // Assert
        Assert.AreEqual("Missing recipe. Recipe service did not return a recipe for id 1 which was expected.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public async Task CreatePizzaOrder_ReturnsCreatedId()
    {
        var order = new PizzaOrder([new PizzaAmount(4, 2)]);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.PlaceOrder(order))
            .ReturnsAsync(17);
        var service = CreateService(orderRepository);

        var actual = await service.CreatePizzaOrder(order);

        Assert.AreEqual(17, actual);
        orderRepository.VerifyAll();
    }

    [TestMethod]
    public async Task CreatePizzaOrder_WhenRepositoryReturnsNoId_Throws()
    {
        var order = new PizzaOrder([new PizzaAmount(4, 2)]);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.PlaceOrder(order))
            .ReturnsAsync(0);
        var service = CreateService(orderRepository);

        var exception = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => service.CreatePizzaOrder(order));

        Assert.AreEqual("Not possible to place Order", exception.Message);
        orderRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrderStatus_ReturnsStatus()
    {
        const long orderId = 17;
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.GetOrderStatus(orderId))
            .ReturnsAsync("done");
        var service = CreateService(orderRepository);

        var actual = await service.GetOrderStatus(orderId);

        Assert.AreEqual("done", actual);
        orderRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrderStatus_WhenOrderIsMissing_Throws()
    {
        const long orderId = 17;
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.GetOrderStatus(orderId))
            .ReturnsAsync(string.Empty);
        var service = CreateService(orderRepository);

        var exception = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => service.GetOrderStatus(orderId));

        Assert.AreEqual("Order not found", exception.Message);
        orderRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrder_ReturnsCustomerOrder()
    {
        const long orderId = 17;
        var expectedOrder = new CustomerOrder(
            orderId,
            [new PizzaAmount(4, 2)],
            "cooking",
            25m);
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.GetOrder(orderId))
            .ReturnsAsync(expectedOrder);
        var service = CreateService(orderRepository);

        var actual = await service.GetOrder(orderId);

        Assert.AreEqual(expectedOrder, actual);
        orderRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrder_WhenOrderIsMissing_Throws()
    {
        const long orderId = 17;
        var orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepository.Setup(repository => repository.GetOrder(orderId))
            .ReturnsAsync((CustomerOrder)null!);
        var service = CreateService(orderRepository);

        var exception = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => service.GetOrder(orderId));

        Assert.AreEqual("Order not found", exception.Message);
        orderRepository.VerifyAll();
    }
}
