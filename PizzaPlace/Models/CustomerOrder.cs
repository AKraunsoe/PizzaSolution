using PizzaPlace.Models.Types;

namespace PizzaPlace.Models;

public record CustomerOrder(long Id, ComparableList<PizzaAmount> RequestedOrder, string expectedTime, string Status, decimal Cost);
