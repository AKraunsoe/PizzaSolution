using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class OrderingControllerTests
{
    private static OrderingController GetController(Mock<IOrderingService> orderingService) =>
        new(orderingService.Object);

    [TestMethod]
    public async Task PlacePizzaOrder()
    {
        // Arrange
        var order = new PizzaOrder([new PizzaAmount(1, 1)]);
        var recipes = new ComparableList<PizzaRecipeDto>
        {
            new(PizzaRecipeType.StandardPizza, [], 10, 1)
        };
        var stock = new ComparableList<StockDto>();

        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        const long orderId = 1;
        orderingService.Setup(x => x.PizzaCreationPossible(order))
            .ReturnsAsync((recipes, stock));
        orderingService.Setup(x => x.CreatePizzaOrder(order)).ReturnsAsync(orderId);
        orderingService.Setup(x => x.HandlePizzaOrder(order, orderId, stock, recipes))
            .ReturnsAsync([]); // Doesn't matter.

        var controller = GetController(orderingService);

        // Act
        var actual = await controller.PlacePizzaOrder(order);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(actual);
        orderingService.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrderStatus_ReturnsStatus()
    {
        const long orderId = 12;
        const string expectedStatus = "cooking";
        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(service => service.GetOrderStatus(orderId))
            .ReturnsAsync(expectedStatus);
        var controller = GetController(orderingService);

        var result = await controller.GetOrderStatus(orderId);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        Assert.AreEqual(expectedStatus, response.GetType().GetProperty("order")!.GetValue(response));
        orderingService.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrderStatus_WhenServiceThrows_PropagatesError()
    {
        const long orderId = 12;
        var expectedException = new PizzaException("Order not found");
        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(service => service.GetOrderStatus(orderId))
            .ThrowsAsync(expectedException);
        var controller = GetController(orderingService);

        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => controller.GetOrderStatus(orderId));

        Assert.AreSame(expectedException, actualException);
        orderingService.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrder_ReturnsCustomerOrder()
    {
        const long orderId = 12;
        var expectedOrder = new CustomerOrder(
            orderId,
            [new PizzaAmount(3, 2)],
            "cooking",
            25m);
        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(service => service.GetOrder(orderId))
            .ReturnsAsync(expectedOrder);
        var controller = GetController(orderingService);

        var result = await controller.GetOrder(orderId);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var response = ((OkObjectResult)result).Value!;
        Assert.AreEqual(expectedOrder, response.GetType().GetProperty("order")!.GetValue(response));
        orderingService.VerifyAll();
    }

    [TestMethod]
    public async Task GetOrder_WhenServiceThrows_PropagatesError()
    {
        const long orderId = 12;
        var expectedException = new PizzaException("Order not found");
        var orderingService = new Mock<IOrderingService>(MockBehavior.Strict);
        orderingService.Setup(service => service.GetOrder(orderId))
            .ThrowsAsync(expectedException);
        var controller = GetController(orderingService);

        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => controller.GetOrder(orderId));

        Assert.AreSame(expectedException, actualException);
        orderingService.VerifyAll();
    }
}
