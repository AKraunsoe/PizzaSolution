using PizzaPlace;
using PizzaPlace.Factories;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using Npgsql;

var envFile = FindEnvironmentFile();
if (envFile is not null)
    DotNetEnv.Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy("allowAll",
        policy =>
        {
            policy.WithOrigins(
            // "whatEver.homepage.com"
            );
            policy.AllowAnyHeader();
            policy.AllowCredentials();
            policy.AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(d =>
{
    d.Title = "Pizza Place";
    d.Version = "v1";
});

// Register services:
var services = builder.Services;
services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("PizzaPlace")
    ?? throw new InvalidOperationException(
        "Missing database connection string. Set ConnectionStrings__PizzaPlace in the environment or root .env file.");
services.AddSingleton(NpgsqlDataSource.Create(connectionString));
services.AddSingleton<PostgresDatabase>();
services.AddTransient<IStockRepository, StockRepository>();
services.AddTransient<IRecipeRepository, RecipeRepository>();
services.AddTransient<IOrderRepository, OrderRepository>();

services.AddTransient<IPizzaOven, NormalPizzaOven>();
services.AddTransient<IPizzaOven, AssemblyLinePizzaOven>();
services.AddTransient<IPizzaOven, GiantRevolvingPizzaOven>();

services.AddTransient<IStockService, StockService>();
services.AddTransient<IRecipeService, RecipeService>();
services.AddTransient<IOrderingService, OrderingService>();
services.AddTransient<IMenuService, MenuService>();

//builder.Services.AddHostedService<ClearOrders>();


var app = builder.Build();

await app.Services.GetRequiredService<PostgresDatabase>().InitializeAsync();

app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors();

app.MapControllers();

app.Run();

static string? FindEnvironmentFile()
{
    foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var directory = new DirectoryInfo(startPath); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
                return candidate;
        }
    }

    return null;
}
