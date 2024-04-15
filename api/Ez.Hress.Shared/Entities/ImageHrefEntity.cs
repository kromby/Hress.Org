namespace Ez.Hress.Shared.Entities;

public class ImageHrefEntity : HrefEntity
{
    public ImageHrefEntity(int id, string name)
    {
        ID = id;
        Name = name;
        Href = $"/api/images/{ID}/content";
    }

    public string Name { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
}
