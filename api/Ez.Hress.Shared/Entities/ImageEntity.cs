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
    }
}
