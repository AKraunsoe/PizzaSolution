namespace PizzaPlace.Services;

public class MenuService : IMenuService
{
    public Menu GetMenu(DateTimeOffset menuDate)
    {
        DateTimeOffset eleven = new DateTimeOffset(DateTime.Today.AddHours(11)).ToUniversalTime();
        DateTimeOffset two = new DateTimeOffset(DateTime.Today.AddHours(14)).ToUniversalTime();
        DateTimeOffset menuDateNew = menuDate.ToUniversalTime();
        if (menuDateNew >= eleven && menuDateNew <= two)
        {
            return createMenu(true);
        }
        else
        {
            return createMenu(false);
        }
    }

    private Menu createMenu(bool lunchMenu)
    {

        Random random = new Random();
        ComparableList<MenuItem> items = new ComparableList<MenuItem>();
        for (int i = 0; i < 12; i++)
        {
            bool soldOut = random.Next(2) != 0 ? false : true;
            string size = i%2 == 0 ? "small" : "large";
            switch (i)
            {
                case 0:
                case 1:
                    items.Add(new MenuItem($"Standard Pizza {size}", i, Models.Types.PizzaRecipeType.StandardPizza, lunchMenu ? 40*(i%2 == 0 ? 1 : 1.5) : 80 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
                case 2:
                case 3:
                    items.Add(new MenuItem($"Extremely Tasty Pizza {size}", i, Models.Types.PizzaRecipeType.ExtremelyTastyPizza, lunchMenu ? 50 * (i % 2 == 0 ? 1 : 1.5) : 100 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
                case 4:
                case 5:
                    items.Add(new MenuItem($"Odd Pizza {size}", i, Models.Types.PizzaRecipeType.OddPizza, lunchMenu ? 45 * (i % 2 == 0 ? 1 : 1.5) : 90 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
                case 6:
                case 7:
                    items.Add(new MenuItem($"Rare Pizza {size}", i, Models.Types.PizzaRecipeType.RarePizza, lunchMenu ? 60 * (i % 2 == 0 ? 1 : 1.5) : 120 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
                case 8:
                case 9:
                    items.Add(new MenuItem($"Horse Radish Pizza {size}", i, Models.Types.PizzaRecipeType.HorseRadishPizza, lunchMenu ? 5 * (i%2 == 0 ? 1 : 1.5) : 10 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
                case 10:
                case 11:
                    items.Add(new MenuItem($"Empty Pizza {size}", i, Models.Types.PizzaRecipeType.EmptyPizza, lunchMenu ? 20 * (i%2 == 0 ? 1 : 1.5) : 40 * (i % 2 == 0 ? 1 : 1.5), soldOut));
                    break;
               }

        }
        string menuTitle = lunchMenu ? "Today's Lunch Menu" : "Today's Menu";
        return new Menu($"{menuTitle}", items);

    }
}
