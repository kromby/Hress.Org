using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class Dish: EntityBase<int>
    {
        public Dish(int id, int eventID, string text)
        {
            ID = id;
            Name = text;
            EventID = eventID;
        }
        public Dish(int id, int eventID, string text, int year)
        {
            ID = id;
            Name = text;
            EventID = eventID;
            Year = year;
        }

        public int EventID { get; set; }
        public int Year { get; set; }
    }
}
