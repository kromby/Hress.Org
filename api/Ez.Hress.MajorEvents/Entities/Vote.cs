using Ez.Hress.Shared.Entities;

namespace Ez.Hress.MajorEvents.Entities;

public class Vote : EntityBase<int>
{
    public int TypeID { get; set; }

    public int CourseID { get; set; }
}
