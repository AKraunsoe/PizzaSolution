using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class OrderRepositoryTests
{
    private static Mock<IOrderRepository> GetOrderRepository() => new Mock<IOrderRepository>(MockBehavior.Strict);


    [TestMethod]
    public async Task PlaceOrder_Success()
    {
        // Arrange
        var requestedOrders = new ComparableList<PizzaAmount>
        {
            new PizzaAmount(1, 1)
        };
        var order = new PizzaOrder(requestedOrders);
        var repository = GetOrderRepository();
        repository.Setup(x => x.PlaceOrder(order)).ReturnsAsync(1);

        // Act
        var actual = await repository.Object.PlaceOrder(order);

        // Assert
        Assert.AreEqual(1, actual);
        repository.Verify(x => x.PlaceOrder(order), Times.Once);
    }

    [TestMethod]
    public async Task PlaceOrder_Throws()
    {
        // Arrange
        var order = new PizzaOrder(
            []);
        var expectedException = new PizzaException("Order must include at least 1 pizza.");
        var repository = GetOrderRepository();
        repository
            .Setup(x => x.PlaceOrder(order))
            .ThrowsAsync(expectedException);

        // Act
        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repository.Object.PlaceOrder(order));

        // Assert
        Assert.AreSame(expectedException, actualException);
        repository.Verify(x => x.PlaceOrder(order), Times.Once);
    }

    [TestMethod]
    public async Task UpdateOrderStatus_Success()
    {
        const long orderId = 12;
        var repository = GetOrderRepository();

        repository.Setup(x => x.UpdateOrderStatus(orderId)).Returns(Task.CompletedTask);

        await repository.Object.UpdateOrderStatus(orderId);

        repository.Verify(x => x.UpdateOrderStatus(orderId), Times.Once);
    }

    [TestMethod]
    public async Task UpdateOrderStatus_Throws()
    {
        const long orderId = 12;
        var expectedException = new PizzaException($"Order with id {orderId} does not exist.");
        var repository = GetOrderRepository();
        repository
            .Setup(x => x.UpdateOrderStatus(orderId))
            .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repository.Object.UpdateOrderStatus(orderId));

        Assert.AreSame(expectedException, actualException);
        repository.Verify(x => x.UpdateOrderStatus(orderId), Times.Once);
    }

    [TestMethod]
    public async Task GetOrderStatus_Success()
    {
        const long orderId = 12;
        const string expectedStatus = "done";
        var repository = GetOrderRepository();
        repository.Setup(x => x.GetOrderStatus(orderId)).ReturnsAsync(expectedStatus);

        var actualStatus = await repository.Object.GetOrderStatus(orderId);

        Assert.AreEqual(expectedStatus, actualStatus);
        repository.Verify(x => x.GetOrderStatus(orderId), Times.Once);
    }

    [TestMethod]
    public async Task GetOrderStatus_Throws()
    {
        const long orderId = 12;
        var expectedException = new PizzaException("Order not found");
        var repository = GetOrderRepository();
        repository
            .Setup(x => x.GetOrderStatus(orderId))
            .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repository.Object.GetOrderStatus(orderId));

        Assert.AreSame(expectedException, actualException);
        repository.Verify(x => x.GetOrderStatus(orderId), Times.Once);
    }

    [TestMethod]
    public async Task GetOrder_Success()
    {
        const long orderId = 12;
        var expectedOrder = new CustomerOrder(
            orderId,
            [new PizzaAmount(1, 2)],
            "sometime",
            "cooking",
            15m);
        var repository = GetOrderRepository();
        repository.Setup(x => x.GetOrder(orderId)).ReturnsAsync(expectedOrder);

        var actualOrder = await repository.Object.GetOrder(orderId);

        Assert.AreSame(expectedOrder, actualOrder);
        repository.Verify(x => x.GetOrder(orderId), Times.Once);
    }

    [TestMethod]
    public async Task GetOrder_Throws()
    {
        const long orderId = 12;
        var expectedException = new PizzaException($"Order with id {orderId} does not exist.");
        var repository = GetOrderRepository();
        repository
            .Setup(x => x.GetOrder(orderId))
            .ThrowsAsync(expectedException);

        var actualException = await Assert.ThrowsExceptionAsync<PizzaException>(
            () => repository.Object.GetOrder(orderId));

        Assert.AreSame(expectedException, actualException);
        repository.Verify(x => x.GetOrder(orderId), Times.Once);
    }

}
