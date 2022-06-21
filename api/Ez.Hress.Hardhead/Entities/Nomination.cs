using Azure;
using Azure.Data.Tables;
using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class Nomination : EntityBase
    {
        public Nomination(int typeID, int nomineeID, string description)
        {
            TypeID = typeID;
            Nominee = new UserBasicEntity() { ID = nomineeID };
            Description = description;
        }

        public int TypeID { get; set; }       

        public UserBasicEntity Nominee { get; set; }        

        public string Description { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Description))
                throw new ArgumentException("Description is required", nameof(Description));

            if (TypeID <= 0)
                throw new ArgumentException("TypeID must be larger then zero", nameof(TypeID));

            if (CreatedBy <= 0)
                throw new ArgumentException("CreatedBy must be larger then zero", nameof(CreatedBy));

            if (Nominee == null || Nominee.ID <= 0)
                throw new ArgumentNullException(nameof(Nominee));
        }
    }
}
