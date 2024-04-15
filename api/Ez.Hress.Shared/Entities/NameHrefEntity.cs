namespace Ez.Hress.Shared.Entities;

public class NameHrefEntity<T> : EntityBase<T>
{
    public NameHrefEntity(T id)
    {
        ID = id;            
    }
    
    public string Href { get => $"/api/albums/{ID}/images"; }
}