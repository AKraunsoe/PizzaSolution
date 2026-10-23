using Namotion.Reflection;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTests
{
    [TestMethod]
    public void LunchMenu()
    {

        var menuService = new MenuService();
        DateTimeOffset lunchTime = new DateTimeOffset(DateTime.Today.AddHours(12)).ToUniversalTime();

        Menu myMenu = menuService.GetMenu(lunchTime);

        Assert.AreEqual(myMenu.Title, "Today's Lunch Menu");
        Assert.IsTrue(myMenu.Items.Count() == 12);
    }

    [TestMethod]
    public void DinnerMenu()
    {
        var menuService = new MenuService();
        DateTimeOffset lunchTime = new DateTimeOffset(DateTime.Today.AddHours(17)).ToUniversalTime();

        Menu myMenu = menuService.GetMenu(lunchTime);

        Assert.AreEqual(myMenu.Title, "Today's Menu");
        Assert.IsTrue(myMenu.Items.Count() == 12);
    }
}