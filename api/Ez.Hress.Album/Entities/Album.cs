using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Albums.Entities
{
    public class Album : EntityBase<int>
    {
        public Album(int id, string name, string description, int imageCount) 
        {
            ID = id;
            Name = name;
            Description= description;   
            ImageCount = imageCount;
        }

        public new string Description { get; set; }
        public int ImageCount { get; set; }

        public HrefEntity Images { get { return new() { ID = this.ID, Href = $"/api/albums/{this.ID}/images" }; } }
    }
}
