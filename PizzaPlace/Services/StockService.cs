using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Extensions;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{

    public ComparableList<StockDto> GetRequiredStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var prepareOrders = order.RequestedOrder
            .GroupBy(request => request.RecipeId)
            .Select(requests =>
            {
                var recipe = recipeDtos.FirstOrDefault(recipe => recipe.Id == requests.Key)
                    ?? throw new NullReferenceException($"Cannot check stock for missing recipe id {requests.Key}.");
                return new PizzaPrepareOrder(recipe, requests.Sum(request => (int)request.Amount));
            });

        return new ComparableList<StockDto>(prepareOrders.GetRequiredStock());
    }

    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos, ComparableList<StockDto> requiredStock)
    {

        var stock = await GetStock(order, recipeDtos);

        return !PizzaHelperExtensions.HasEnough(stock, requiredStock);
        /*var storage = new Dictionary<StockType, int>();

        for (int i = 0; i < order.RequestedOrder.Count; i++)
        {
            var recipeId = order.RequestedOrder[i].RecipeId;
            var quant = order.RequestedOrder[i].Amount;
            if (quant > 0)
            {
                var recipe = recipeDtos.FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null)
                {
                    throw new NullReferenceException($"Cannot check stock for missing recipe id {recipeId}.");
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

        return false;*/

    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var result = new ComparableList<StockDto>();
        for (int i = 0; i < order.RequestedOrder.Count; i++)
        {
            var recipeId = order.RequestedOrder[i].RecipeId;
            var quant = order.RequestedOrder[i].Amount;
            if (quant > 0)
            {
                var recipe = recipeDtos.FirstOrDefault(r => r.Id == recipeId);
                if (recipe == null)
                {
                    throw new NullReferenceException($"Cannot check stock for missing recipe id {recipeId}.");
                }

                for (int j = 0; j < recipe.Ingredients.Count; j++)
                {
                    var stockType = recipe.Ingredients[j].StockType;
                    var stock = await stockRepository.GetStock(stockType);
                    if (stock == null)
                    {
                        throw new NullReferenceException("Stock cannot be null");
                    }
                    result.Add(stock);
                }
            }
        }
        return result;
    }

    public async Task<ComparableList<StockDto>> TakeStock(ComparableList<StockDto> stock){
        var result = new ComparableList<StockDto>();
        foreach (var s in stock)
        {
            result.Add(await stockRepository.TakeStock(s.StockType, s.Amount));
        }
        return result;
    }
}
