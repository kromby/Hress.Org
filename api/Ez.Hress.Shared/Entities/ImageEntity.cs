using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class ImageEntity : EntityBase<int>
    {
        public ImageEntity(int id, string name, string photoUrl)
        {
            ID = id;
            Name = name;
            PhotoUrl = photoUrl;
        }

        public string PhotoUrl { get; set; }

        public string? PhotoThumbUrl { get; set; }

        public byte[]? Content { get; set; }

        public ImageContainer Container { get; set; }

        public void Validate()
        {
            if (ID < 0)
                throw new ArgumentException("Can not be a negative number.", nameof(ID));

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Can not be null or empty.", nameof(Name));

            if (Content == null)
                throw new ArgumentException("Can not be null.", nameof(Content));
        }
    }

    public enum ImageContainer
    {
        Other,
        Hardhead,
        Profile,
        News,
        Album,
        ATVR        
    }
}
