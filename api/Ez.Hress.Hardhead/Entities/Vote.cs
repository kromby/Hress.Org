using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class Vote
    {
        public int ID { get; set; }

        public int EventID { get; set; }

        public string? Value { get; set; }

        public string? Description { get; set; }

        public int? PollEntryID { get; set; }

        public DateTime Created { get; set; }

        public void Validate()
        {

            if (EventID <= 0)
                throw new ArgumentException("Must be greater than zero.", nameof(EventID));

            if (string.IsNullOrWhiteSpace(Value))
                throw new ArgumentException("Can not be null or empty.", nameof(Value));
        }
    }
}
