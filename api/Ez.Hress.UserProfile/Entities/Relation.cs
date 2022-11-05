using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.UserProfile.Entities
{
    public class Relation : EntityBase<int>
    {
        public Relation(int id, UserBasicEntity user, TypeEntity type)
        {
            ID = id;
            RelatedUser = user;
            Type = type;
        }

        public UserBasicEntity RelatedUser { get; set; }

        public TypeEntity Type { get; set; }
    }
}
