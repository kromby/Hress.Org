using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities;

public class YearEntity : EntityBase<int>
{
    public int GuestCount { get; set; }
    public int? PhotoID { private get; set; }
    public HrefEntity? Photo
    {
        get
        {
            if (PhotoID.HasValue)
                return new HrefEntity()
                {
                    Href = string.Format("/api/images/{0}/content", PhotoID.Value)
                };

            return null;
        }
    }

    public UserBasicEntity? Hardhead { get; set; }
}