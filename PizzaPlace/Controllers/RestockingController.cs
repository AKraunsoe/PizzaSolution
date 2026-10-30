using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Controllers;

[Route("api/restocking")]
public class RestockingController(IStockRepository stockRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Restock([FromBody] ComparableList<IngredientRequest> stock)
    {
        var result = new ComparableList<StockDto>();
        foreach (var s in stock)
        {
            result.Add(await stockRepository.AddToStock(new StockDto(s.StockType, s.Amount)));
        }

        return Ok(result);
    }
}
