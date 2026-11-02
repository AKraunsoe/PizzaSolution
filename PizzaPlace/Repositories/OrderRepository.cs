using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using Npgsql;

namespace PizzaPlace.Repositories;

public class OrderRepository(NpgsqlDataSource dataSource) : IOrderRepository
{
    public async Task<long> PlaceOrder(PizzaOrder order)
    {
        if (order.RequestedOrder.Count == 0)
            throw new PizzaException("Order must include at least 1 pizza.");

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        var cookingTime = 0;

        foreach (var pizza in order.RequestedOrder)
        {
            await using var getCookingTime = new NpgsqlCommand(
                "SELECT cooking_time_minutes FROM recipes WHERE id = @id;",
                connection,
                transaction);
            getCookingTime.Parameters.AddWithValue("id", pizza.RecipeId);
            var result = await getCookingTime.ExecuteScalarAsync();
            if (result is null)
            {
                throw new PizzaException("Could not find Recipe, you can't order recipes that don't exist");
            }

            cookingTime = Math.Max(Convert.ToInt32(result), cookingTime);
        }

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO orders (expected_end_time, cost)
            VALUES (@expectedTime, @cost)
            RETURNING id;
            """,
            connection,
            transaction);
        var startTime = DateTime.UtcNow;

        command.Parameters.AddWithValue("expectedTime", startTime.AddMinutes(cookingTime));
        command.Parameters.AddWithValue("cost", 0);

        var orderId = await command.ExecuteScalarAsync();
        if (orderId is not null)
        {
            foreach (var pizza in order.RequestedOrder)
            {
                await using var createPizzaCommand = new NpgsqlCommand(
                    """
                    INSERT INTO pizzas (recipe_id, amount, order_id)
                    VALUES (@recipeId, @amount, @orderId)
                    """,
                    connection,
                    transaction);
                createPizzaCommand.Parameters.AddWithValue("recipeId", pizza.RecipeId);
                createPizzaCommand.Parameters.AddWithValue("amount", (short)pizza.Amount);
                createPizzaCommand.Parameters.AddWithValue("orderId", Convert.ToInt64(orderId));
                await createPizzaCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return Convert.ToInt64(orderId);
        }

        throw new PizzaException("Could not create order");
    }

    public async Task UpdateOrderStatus(long id)
    {

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            """
            UPDATE orders
            SET status = 'done'
            WHERE id = @id
            RETURNING id;
            """,
            connection);
        command.Parameters.AddWithValue("id", id);

        var updatedId = await command.ExecuteScalarAsync();
        if (updatedId is null)
            throw new PizzaException($"Order with id {id} does not exist.");

    }

    public async Task<string> GetOrderStatus(long id)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT status FROM orders WHERE id = @id;",
            connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? reader.GetString(0)
            : string.Empty;
    }

    public async Task<CustomerOrder> GetOrder(long id)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var orderCommand = new NpgsqlCommand(
            """
            SELECT status, expected_end_time, cost::numeric
            FROM orders
            WHERE id = @id;
            """,
            connection);
        orderCommand.Parameters.AddWithValue("id", id);

        string status;
        decimal cost;
        DateTime expectedEndTime;
        await using (var orderReader = await orderCommand.ExecuteReaderAsync())
        {
            if (!await orderReader.ReadAsync())
                throw new PizzaException($"Order with id {id} does not exist.");

            status = orderReader.GetString(0);
            expectedEndTime = orderReader.GetDateTime(1).ToLocalTime();
            cost = orderReader.GetDecimal(2);
        }

        await using var pizzasCommand = new NpgsqlCommand(
            """
            SELECT recipe_id, amount
            FROM pizzas
            WHERE order_id = @id
            ORDER BY id;
            """,
            connection);
        pizzasCommand.Parameters.AddWithValue("id", id);

        ComparableList<PizzaAmount> requestedOrder = [];
        await using (var pizzasReader = await pizzasCommand.ExecuteReaderAsync())
        {
            while (await pizzasReader.ReadAsync())
            {
                requestedOrder.Add(new PizzaAmount(
                    pizzasReader.GetInt64(0),
                    checked((ushort)pizzasReader.GetInt64(1))));
            }
        }

        return new CustomerOrder(id, requestedOrder, expectedEndTime.ToString("HH:mm:ss yyyy-MM-dd"), status, cost);
    }
}
