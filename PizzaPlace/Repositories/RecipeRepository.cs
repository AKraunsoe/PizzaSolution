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
        await using var recipeCommand = new NpgsqlCommand(
            """
            SELECT id, recipe_type, cooking_time_minutes
            FROM recipes r
            WHERE r.id = @id;
            """,
            connection);
        recipeCommand.Parameters.AddWithValue("id", id);

        await using var recipeReader = await recipeCommand.ExecuteReaderAsync();
        if (!await recipeReader.ReadAsync())
            throw new PizzaException($"Recipe does not exist with id {id}.");

        var recipeType = Enum.Parse<PizzaRecipeType>(recipeReader.GetString(1));
        var cookingTime = recipeReader.GetInt32(2);
        recipeReader.Close();

        await using var ingredientsCommand = new NpgsqlCommand(
            """
            SELECT stock_type, amount
            FROM ingredients
            WHERE recipe_id = @id;
            """, connection);
        ingredientsCommand.Parameters.AddWithValue("id", id);

        var ingredients = new ComparableList<StockDto>();
        await using var ingredientReader = await ingredientsCommand.ExecuteReaderAsync();
        while (await ingredientReader.ReadAsync())
        {
            ingredients.Add(new StockDto(
                        Enum.Parse<StockType>(ingredientReader.GetString(0)),
                        ingredientReader.GetInt32(1)));
        }

        if(ingredients.Count == 0){
            throw new PizzaException($"Recipe with id {id} does not have any ingredients");
        }

        return new PizzaRecipeDto(
            recipeType,
            ingredients,
            cookingTime,
            id);
    }

    public async Task<ComparableList<PizzaRecipeDto>> GetRecipes()
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            """
            SELECT
                r.id,
                r.recipe_type,
                r.cooking_time_minutes,
                COALESCE(
                    jsonb_agg(jsonb_build_object(
                        'StockType', i.stock_type,
                        'Amount', i.amount,
                        'Id', i.id
                    )) FILTER (WHERE i.id IS NOT NULL),
                    '[]'::jsonb
                ) AS ingredients
            FROM recipes AS r
            LEFT JOIN ingredients AS i ON i.recipe_id = r.id
            GROUP BY r.id, r.recipe_type, r.cooking_time_minutes
            ORDER BY r.id;
            """,
            connection);
        await using var reader = await command.ExecuteReaderAsync();

        ComparableList<PizzaRecipeDto> recipes = [];
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var recipeType = Enum.Parse<PizzaRecipeType>(reader.GetString(1));
            var ingredients = JsonSerializer.Deserialize<List<StockDto>>(reader.GetString(3))
                ?? throw new InvalidOperationException($"Stored ingredients are invalid for recipe id {id}.");
            recipes.Add(new PizzaRecipeDto(
                recipeType,
                new ComparableList<StockDto>(ingredients),
                reader.GetInt32(2),
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
                cooking_time_minutes = @cookingTimeMinutes
            WHERE id = @id
            RETURNING id;
            """,
            connection);
        command.Parameters.AddWithValue("id", recipe.Id);
        command.Parameters.AddWithValue("recipeType", recipe.RecipeType.ToString());
        command.Parameters.AddWithValue("cookingTimeMinutes", recipe.CookingTimeMinutes);

        var id = await command.ExecuteScalarAsync();

        await using var removeIngredients = new NpgsqlCommand(
            """
            DELETE FROM ingredients
            WHERE recipe_id = @id;
            """,
            connection);

        removeIngredients.Parameters.AddWithValue("id", recipe.Id);

        await removeIngredients.ExecuteScalarAsync();

        foreach(var ingredient in recipe.Ingredients){
            await using var insertIngredient = new NpgsqlCommand(
                """
                INSERT INTO ingredients (stock_type, amount, recipe_id)
                VALUES (@stockType, @amount, @id)
                RETURNING id;
                """, connection);
            insertIngredient.Parameters.AddWithValue("stockType", ingredient.StockType.ToString());
            insertIngredient.Parameters.AddWithValue("amount", ingredient.Amount);
            insertIngredient.Parameters.AddWithValue("id", recipe.Id);

            var ingredientId = await insertIngredient.ExecuteScalarAsync();
            if (!(ingredientId is long stockId)){
                throw new InvalidOperationException("The database did not return the new ingredient ID.");
            }
        }

        return id is long recipeId
            ? recipeId
            : throw new PizzaException($"Recipe does not exist with id {recipe.Id}.");
    }
}
