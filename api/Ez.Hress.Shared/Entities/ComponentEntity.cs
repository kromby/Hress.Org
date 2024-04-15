namespace Ez.Hress.Shared.Entities;

public class ComponentEntity : EntityBase<int>
{
    public HrefEntity? Link { get; set; }
    public bool Public { get; set; }
    public bool IsLegacy { get; set; }
}
