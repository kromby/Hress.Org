namespace Ez.Hress.Shared.Entities;

public class TypeEntity : EntityBase<int>
{
    public TypeEntity(int id)
    {
        ID = id;
        Code = "UNKNOWN";
    }

    public TypeEntity(int id, string name, string code)
    {
        ID = id;
        Name = name;
        Code = code;
    }

    public string Code { get; set; }
    public int? ParentID { get; set; }
    public int GroupType { get; set; }
}
