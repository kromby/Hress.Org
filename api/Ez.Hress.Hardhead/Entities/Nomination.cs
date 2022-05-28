using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class Nomination
    {
        public Guid ID { get; set; }

        public int TypeID { get; set; }
        
        public int NomineeID { get; set; }

        public string Description { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Description))
                throw new ArgumentException("Description is required", nameof(Description));

            if (TypeID <= 0)
                throw new ArgumentException("TypeID must be larger then zero", nameof(TypeID));

            if (NomineeID <= 0)
                throw new ArgumentException("NomineeID must be larger then zero", nameof(NomineeID));

            if (CreatedBy <= 0)
                throw new ArgumentException("CreatedBy must be larger then zero", nameof(CreatedBy));
        }
    }
}
