using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var stock = await GetStock(order, recipeDtos);
        var storage = new Dictionary<StockType, int>();

        for (int i = 0; i < order.RequestedOrder.Count; i++)
        {
            var pizzaType = order.RequestedOrder[i].PizzaType;
            var quant = order.RequestedOrder[i].Amount;
            if (quant > 0)
            {
                var recipe = recipeDtos.FirstOrDefault(r => r.RecipeType == pizzaType);
                if (recipe == null)
                {
                    throw new NullReferenceException("Cannot check stock for non existant Pizza");
                }

                for (int j = 0; j < recipe.Ingredients.Count; j++)
                {
                    var stockType = recipe.Ingredients[j].StockType;
                    var ingredientAmount = recipe.Ingredients[j].Amount;
                    var availableStock = stock.FirstOrDefault(s=> s.StockType == recipe.Ingredients[j].StockType);
                    if (storage.TryGetValue(stockType, out var currentStock))
                    {
                        if (availableStock.Amount < currentStock + recipe.Ingredients[j].Amount * quant)
                        {
                            return true;
                        }
                        else
                        {
                            storage[stockType] += (recipe.Ingredients[j].Amount * quant);
                        }
                    }
                    else if (availableStock.Amount < recipe.Ingredients[j].Amount * quant)
                    {
                        return true;
                    }
                    else
                    {
                        storage.Add(stockType, ingredientAmount*quant);
                    }
                }
            }
        }

        return false;

    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var result = new ComparableList<StockDto>();
        for (int i = 0; i < order.RequestedOrder.Count; i++)
        {
            var pizzaType = order.RequestedOrder[i].PizzaType;
            var quant = order.RequestedOrder[i].Amount;
            if (quant > 0)
            {
                var recipe = recipeDtos.FirstOrDefault(r => r.RecipeType == pizzaType);
                if (recipe == null)
                {
                    throw new NullReferenceException("Cannot check stock for non existant Pizza");
                }

                for (int j = 0; j < recipe.Ingredients.Count; j++)
                {
                    var stockType = recipe.Ingredients[j].StockType;
                    var stock = await stockRepository.GetStock(stockType);
                    if(stock == null)
                    {
                        throw new NullReferenceException("Stock cannot be null");
                    }
                    result.Add(stock);
                }
            }
        }
        return result;
    }
}
