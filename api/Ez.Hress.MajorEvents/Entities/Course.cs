using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class Course : EntityBase<int>
{
    public Course(int id, string name)
    {
        ID = id;
        Name = name;
        Dishes = new List<Dish>();
    }

    public IList<Dish> Dishes { get; set; }

}
