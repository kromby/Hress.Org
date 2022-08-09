using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class Guest : EntityBase<int>
    {
        public Guest(int userId, string username, int photoId, string roleName)
        {
            ID = userId;
            Name = username;
            User = new()
            {
                ID = userId,
                Username = username,
                ProfilePhotoId = photoId
            };
            Role = roleName;
        }

        public UserBasicEntity User { get; set; }
        public string Role { get; set; }
    }
}
