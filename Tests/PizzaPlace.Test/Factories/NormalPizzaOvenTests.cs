using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Test.TestExtensions;

namespace PizzaPlace.Test.Factories;

[TestClass]
public class NormalPizzaOvenTests
{
    private static NormalPizzaOven GetOven(TimeProvider timeProvider) => new(timeProvider);

    public const int StandardPizzaPrepareTime = 10;
    public const int TastyPizzaPrepareTime = 15;

    [TestMethod]
    public async Task PreparePizzas_OnePizza()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var order = new ComparableList<PizzaPrepareOrder>
        {
            new PizzaPrepareOrder(GetTestStandardPizzaRecipe(), 1),
        };
        var oven = GetOven(timeProvider);
        var expectedTime = StandardPizzaPrepareTime;
        var expectedPizzas = 1;

        // Act
        var pizzasTask = oven.PreparePizzas(order);
        timeProvider.PassTimeInMinuteIntervals(expectedTime - 1);
        var firstCheck = pizzasTask.IsCompleted;
        timeProvider.PassTimeInMinuteIntervals(1);
        var secondCheck = pizzasTask.IsCompleted;

        // Assert
        Assert.IsFalse(firstCheck);
        Assert.IsTrue(secondCheck);
        var pizzas = await pizzasTask;
        Assert.AreEqual(expectedPizzas, pizzas.Count());
        Assert.IsTrue(pizzas.All(x => x is StandardPizza), "Only standard pizzas");
    }

    [TestMethod]
    public async Task PreparePizzas_FivePizzas()
    {
        // Arrange
        var timeProvider = new FakeTimeProvider();
        var order = new ComparableList<PizzaPrepareOrder>
        {
            new PizzaPrepareOrder(GetTestStandardPizzaRecipe(), 2),
            new PizzaPrepareOrder(GetTestTastyPizzaRecipe(), 3)
        };
        var oven = GetOven(timeProvider);
        var expectedTime = StandardPizzaPrepareTime + TastyPizzaPrepareTime;
        var expectedPizzas = 5;
        var expectedStandard = 2;
        var expectedTasty = 3;

        // Act
        var pizzasTask = oven.PreparePizzas(order);
        timeProvider.PassTimeInMinuteIntervals(expectedTime - 1);
        var firstCheck = pizzasTask.IsCompleted;
        timeProvider.PassTimeInMinuteIntervals(1);
        var secondCheck = pizzasTask.IsCompleted;

        // Assert
        Assert.IsFalse(firstCheck);
        Assert.IsTrue(secondCheck);
        var pizzas = await pizzasTask;
        Assert.AreEqual(expectedPizzas, pizzas.Count());
        Assert.AreEqual(expectedStandard, pizzas.Count(x => x is StandardPizza));
        Assert.AreEqual(expectedTasty, pizzas.Count(x => x is ExtremelyTastyPizza));
    }

    public static PizzaRecipeDto GetTestStandardPizzaRecipe() =>
        new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [
                new StockDto(StockType.Dough, 1),
                new StockDto(StockType.Tomatoes, 2),
                new StockDto(StockType.GratedCheese, 1),
                new StockDto(StockType.GenericSpices, 1)
            ], StandardPizzaPrepareTime);

    public static PizzaRecipeDto GetTestTastyPizzaRecipe() =>
        new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza, [
                new StockDto(StockType.FermentedDough, 1),
                new StockDto(StockType.RottenTomatoes, 2),
                new StockDto(StockType.Bacon, 1),
                new StockDto(StockType.GenericSpices, 1)
            ], TastyPizzaPrepareTime);

}
