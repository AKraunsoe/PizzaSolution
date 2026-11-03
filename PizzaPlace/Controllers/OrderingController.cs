using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/order")]
public class OrderingController(
    IOrderingService orderingService) : ControllerBase
{

    [HttpPost("place")]
    public async Task<IActionResult> PlacePizzaOrder([FromBody] PizzaOrder pizzaOrder)
    {
        var stockTuple = await orderingService.PizzaCreationPossible(pizzaOrder);
        var orderId = await orderingService.CreatePizzaOrder(pizzaOrder);
        orderingService.HandlePizzaOrder(pizzaOrder, orderId, requiredStock, recipes);
        return Ok(new
        {
            status = "Accepted",
            Id = orderId,
            message = "You order is being prepared, you can use the status endpoint to check it's status to see if it is done or not"
        });
    }

    [HttpGet("status/{id:long}")]
    public async Task<IActionResult> GetOrderStatus(long id)
    {
        return Ok(new{
            order = await orderingService.GetOrderStatus(id)
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOrder(long id)
    {
        return Ok(new
        {
            order = await orderingService.GetOrder(id)
        });
    }


}
