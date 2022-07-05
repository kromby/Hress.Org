using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Scripts.Entities
{
    /// <summary>
    /// Specifies the alignment of an image in relation to the text of a Web page.
    /// </summary>
    public enum Align
    {
        Left = 1,
        Right = 2,
        Top = 4,
    }
    
    public class News : EntityBase<int>
    {
        public string? Content { get; set; }
        public Align ImageAlign { get; set; }
        public int? ImageID { private get; set; }
        public HrefEntity Image { get
            {
                return new HrefEntity
                {
                    ID = ImageID,
                    Href = $"/api/images/{ImageID}/content"
                };
            }
        }
        public UserBasicEntity Author { get; set; }
    }
}
