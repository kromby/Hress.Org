using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class Dish: EntityBase<int>
{
    public Dish(int id, int eventID, string text)
    {
        ID = id;
        Name = text;
        EventID = eventID;
    }
    public Dish(int id, int eventID, string text, int year)
    {
        ID = id;
        Name = text;
        EventID = eventID;
        Year = year;
    }

    public int EventID { get; set; }
    public int Year { get; set; }
}
