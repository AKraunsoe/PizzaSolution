using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using Npgsql;
using NpgsqlTypes;

namespace PizzaPlace.Repositories;

public class RecipeRepository(NpgsqlDataSource dataSource) : IRecipeRepository
{
    public async Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var insertRecipe = new NpgsqlCommand(
            """
            INSERT INTO recipes (recipe_type, cooking_time_minutes)
            VALUES (@recipeType, @cookingTimeMinutes)
            RETURNING id;
            """,
            connection);
        insertRecipe.Parameters.AddWithValue("recipeType", recipe.RecipeType.ToString());
        insertRecipe.Parameters.AddWithValue("cookingTimeMinutes", recipe.CookingTimeMinutes);

        var id = await insertRecipe.ExecuteScalarAsync();

        foreach(var ingredient in recipe.Ingredients){
            await using var insertIngredient = new NpgsqlCommand(
                """
                INSERT INTO ingredients (stock_type, amount, recipe_id)
                VALUES (@stockType, @amount, @id)
                RETURNING id;

                """, connection);
            insertIngredient.Parameters.AddWithValue("stockType", ingredient.StockType.ToString());
            insertIngredient.Parameters.AddWithValue("amount", ingredient.Amount);
            insertIngredient.Parameters.AddWithValue("id", id);

            var ingredientId = await insertIngredient.ExecuteScalarAsync();
            if (!(ingredientId is long stockId)){
                throw new InvalidOperationException("The database did not return the new ingredient ID.");
            }
        }

        return id is long recipeId
            ? recipeId
            : throw new InvalidOperationException("The database did not return the new recipe ID.");
    }

    public async Task<PizzaRecipeDto> GetRecipe(long id)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, recipe_type, ingredients, cooking_time_minutes FROM recipes WHERE id = @id;",
            connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new PizzaException($"Recipe does not exist with id {id}.");

        var recipeType = Enum.Parse<PizzaRecipeType>(reader.GetString(1));
        var ingredients = JsonSerializer.Deserialize<List<StockDto>>(reader.GetString(2))
            ?? throw new InvalidOperationException($"Stored ingredients are invalid for recipe id {id}.");
        return new PizzaRecipeDto(
            recipeType,
            new ComparableList<StockDto>(ingredients),
            reader.GetInt32(3),
            reader.GetInt64(0));
    }

    public async Task<ComparableList<PizzaRecipeDto>> GetRecipes()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, recipe_type, ingredients, cooking_time_minutes FROM recipes ORDER BY id;",
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        ComparableList<PizzaRecipeDto> recipes = [];
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var recipeType = Enum.Parse<PizzaRecipeType>(reader.GetString(1));
            var ingredients = JsonSerializer.Deserialize<List<StockDto>>(reader.GetString(2))
                ?? throw new InvalidOperationException($"Stored ingredients are invalid for recipe id {id}.");
            recipes.Add(new PizzaRecipeDto(
                recipeType,
                new ComparableList<StockDto>(ingredients),
                reader.GetInt32(3),
                id));
        }

        return recipes;
    }

    public async Task<long> UpdateRecipe(PizzaRecipeDto recipe)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            """
            UPDATE recipes
            SET recipe_type = @recipeType,
                ingredients = @ingredients,
                cooking_time_minutes = @cookingTimeMinutes
            WHERE id = @id
            RETURNING id;
            """,
            connection);
        command.Parameters.AddWithValue("id", recipe.Id);
        command.Parameters.AddWithValue("recipeType", recipe.RecipeType.ToString());
        command.Parameters.Add("ingredients", NpgsqlDbType.Jsonb).Value = JsonSerializer.Serialize(recipe.Ingredients.ToList());
        command.Parameters.AddWithValue("cookingTimeMinutes", recipe.CookingTimeMinutes);

        var id = await command.ExecuteScalarAsync();
        return id is long recipeId
            ? recipeId
            : throw new PizzaException($"Recipe does not exist with id {recipe.Id}.");
    }
}
