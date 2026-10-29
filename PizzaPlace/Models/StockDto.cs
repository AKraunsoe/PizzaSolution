using PizzaPlace.Models.Types;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PizzaPlace.Models;

public record StockDto(StockType StockType,
    [property: DefaultValue(1)]
    [Range(1, int.MaxValue)]
    int Amount, long Id = 0) : Dto(Id);
