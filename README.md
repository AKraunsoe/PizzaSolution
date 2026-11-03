# Pizza Place

Pizza Place is an ASP.NET Core Web API for an automated pizza shop. It provides endpoints to view the menu, manage pizza recipes and stock, place orders, and check order status. Recipes, ingredients, stock, and orders are stored in PostgreSQL.

## Requirements

- .NET 8 SDK
- PostgreSQL server and a database/user the application can connect to

The application uses Npgsql for PostgreSQL access. On startup, it creates the required tables if they do not already exist.

## Configure and run

1. Copy `.env.example` in the repository root to `.env`.
2. Set `ConnectionStrings__PizzaPlace` in `.env` to your PostgreSQL connection string. For example:

   ```dotenv
   ConnectionStrings__PizzaPlace=Host=localhost;Port=5432;Database=PizzaPlace;Username=postgres;Password=your-password
   ```

3. From the repository root, restore and run the API:

   ```powershell
   dotnet restore .\PizzaSolution.sln
   dotnet run --project .\PizzaPlace\PizzaPlace.csproj
   ```

The app prints its listening address when it starts. Open `/swagger` on that address to explore and call the API.

## Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/welcome` | Returns a welcome message. |
| `GET` | `/api/menu` | Returns the current menu. |
| `POST` | `/api/order/place` | Checks required recipes and stock, creates an order, and starts preparing it. The response includes the order ID and a status-check hint. |
| `GET` | `/api/order/status/{id}` | Returns the status (`cooking` or `done`) for an order. |
| `GET` | `/api/order/{id}` | Returns the order details, including its pizzas, status, and cost. |
| `GET` | `/api/recipe` | Returns all recipes. |
| `GET` | `/api/recipe/{id}` | Returns a recipe by ID. |
| `POST` | `/api/recipe` | Creates a recipe. |
| `PUT` | `/api/recipe` | Updates an existing recipe. Include its ID in the request body. |
| `POST` | `/api/restocking` | Adds the specified ingredient amounts to stock. |

### Request examples

Enum values are sent as strings, such as `StandardPizza` and `Dough`.

Place an order:

```json
{
  "requestedOrder": [
    { "recipeId": 1, "amount": 2 }
  ]
}
```

Create a recipe:

```json
{
  "recipeType": "StandardPizza",
  "ingredients": [
    { "stockType": "Dough", "amount": 2 },
    { "stockType": "Tomatoes", "amount": 1 }
  ],
  "cookingTimeMinutes": 10
}
```

Update a recipe using the same fields plus its ID:

```json
{
  "id": 1,
  "recipeType": "StandardPizza",
  "ingredients": [
    { "stockType": "Dough", "amount": 2 },
    { "stockType": "Tomatoes", "amount": 1 }
  ],
  "cookingTimeMinutes": 10
}
```

Restock ingredients (send a JSON array):

```json
[
  { "stockType": "Dough", "amount": 20 },
  { "stockType": "Tomatoes", "amount": 10 }
]
```
