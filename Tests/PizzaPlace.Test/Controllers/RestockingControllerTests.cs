using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Controllers;

[TestClass]
public class RestockingControllerTests
{
    [TestMethod]
    public async Task Restock_AddsStockAndReturnsResults()
    {
        var requests = new ComparableList<IngredientRequest>
        {
            new(StockType.Dough, 10),
            new(StockType.Tomatoes, 5)
        };
        var expected = new ComparableList<StockDto>
        {
            new(StockType.Dough, 10, 1),
            new(StockType.Tomatoes, 5, 2)
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository
            .Setup(repository => repository.AddToStock(new StockDto(StockType.Dough, 10, 0)))
            .ReturnsAsync(expected[0]);
        stockRepository
            .Setup(repository => repository.AddToStock(new StockDto(StockType.Tomatoes, 5, 0)))
            .ReturnsAsync(expected[1]);

        var controller = new RestockingController(stockRepository.Object);

        var result = await controller.Restock(requests);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        Assert.AreEqual(expected, ((OkObjectResult)result).Value);
        stockRepository.VerifyAll();
    }
}
