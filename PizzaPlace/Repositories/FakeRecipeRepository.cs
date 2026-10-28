using PizzaPlace.Models;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Repositories;

public class FakeRecipeRepository : FakeDatabase<PizzaRecipeDto>, IRecipeRepository
{
    private static readonly object _lock = new();

    public Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        lock (_lock)
        {
            var id = Insert(recipe);
            return Task.FromResult(id);
        }
    }

    public Task<long> UpdateRecipe(PizzaRecipeDto recipe)
    {
        lock (_lock)
        { 
            if (Get(recipe.Id) is null)
                throw new PizzaException($"Recipe does not exist with id {recipe.Id}.");

            Update(recipe, recipe.Id);
            return Task.FromResult(recipe.Id);
        }
    }

    public Task<PizzaRecipeDto> GetRecipe(long id)
    {
        var recipe = Get(id) ?? throw new PizzaException($"Recipe does not exist with id {id}.");

        return Task.FromResult(recipe);
    }

    public Task<ComparableList<PizzaRecipeDto>> GetRecipes() =>
        Task.FromResult(new ComparableList<PizzaRecipeDto>(Get(_ => true)));



    public void AddStandardRecipes()
    {
        if (Get(_ => true).Any())
            return;

        foreach (var recipe in GetStandardRecipes())
            AddRecipe(recipe).GetAwaiter().GetResult();

        static List<PizzaRecipeDto> GetStandardRecipes() =>
        [
            new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2),
                new StockDto(StockType.GratedCheese, 1),
                new StockDto(StockType.GenericSpices, 1)
            ], 10),
            new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, [
                new StockDto(StockType.FermentedDough, 1),
                new StockDto(StockType.Tomatoes, 1),
                new StockDto(StockType.RottenTomatoes, 1),
                new StockDto(StockType.GratedCheese, 1),
                new StockDto(StockType.UngenericSpices, 1)
            ], 10),
        ];
    }
}
