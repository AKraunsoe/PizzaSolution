using PizzaPlace.Models;

namespace PizzaPlace.Services;

public interface IStockService
{
    Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos, ComparableList<StockDto> stock);

    Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos);

    Task<ComparableList<StockDto>> TakeStock(ComparableList<StockDto> stock);
}
