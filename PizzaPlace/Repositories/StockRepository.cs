using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using Npgsql;

namespace PizzaPlace.Repositories;

public class StockRepository(NpgsqlDataSource dataSource) : IStockRepository
{
    public async Task<StockDto> AddToStock(StockDto stock)
    {
        if (stock.Amount < 0)
            throw new PizzaException("Stock cannot have negative amount.");

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO stock (stock_type, amount)
            VALUES (@stockType, @amount)
            ON CONFLICT (stock_type)
            DO UPDATE SET amount = stock.amount + EXCLUDED.amount
            RETURNING id, stock_type, amount;
            """,
            connection);
        command.Parameters.AddWithValue("stockType", stock.StockType.ToString());
        command.Parameters.AddWithValue("amount", stock.Amount);

        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new StockDto(
            Enum.Parse<StockType>(reader.GetString(1)),
            reader.GetInt32(2),
            reader.GetInt64(0));
    }

    public async Task<StockDto> GetStock(StockType stockType)
    {
        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            "SELECT id, amount FROM stock WHERE stock_type = @stockType;",
            connection);
        command.Parameters.AddWithValue("stockType", stockType.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync()
            ? new StockDto(stockType, reader.GetInt32(1), reader.GetInt64(0))
            : new StockDto(stockType, 0);
    }

    public async Task<StockDto> TakeStock(StockType stockType, int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Unable to take zero or negative amount.");

        await using var connection = await dataSource.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(
            """
            UPDATE stock
            SET amount = amount - @amount
            WHERE stock_type = @stockType AND amount >= @amount
            RETURNING id;
            """,
            connection);
        command.Parameters.AddWithValue("stockType", stockType.ToString());
        command.Parameters.AddWithValue("amount", amount);

        var id = await command.ExecuteScalarAsync();
        if (id is null)
            throw new PizzaException("Not enough stock to take the given amount.");

        return new StockDto(stockType, amount);
    }
}
