using Ez.Hress.Shared.Entities;
using System;

namespace Ez.Hress.Albums.Entities;

public class Album : EntityBase<int>
{
    public Album(int id, string name, string description, int imageCount, DateTime? inserted = null) 
    {
        ID = id;
        Name = name;
        Description = description;   
        ImageCount = imageCount;
        Inserted = inserted ?? DateTime.UtcNow;
    }

    public Album() 
    {
        Name = string.Empty;
        Description = string.Empty;
        Inserted = DateTime.UtcNow;
    }

    public new string Description { get; set; }
    public int ImageCount { get; set; }

    public HrefEntity Images { get => new()
    {
        ID = this.ID,
        Href = $"/api/albums/{this.ID}/images"
    }; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Name cannot be null or empty", nameof(Name));

        if (Name.Length < 2)
            throw new ArgumentException("Name must be at least 2 characters long", nameof(Name));

        if (string.IsNullOrWhiteSpace(Description))
            throw new ArgumentException("Description cannot be null or empty", nameof(Description));

        if (Inserted > DateTime.UtcNow)
            throw new ArgumentException("Inserted date cannot be in the future", nameof(Inserted));
    }
}
