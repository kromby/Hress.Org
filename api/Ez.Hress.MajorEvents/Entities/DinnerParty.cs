using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class DinnerParty : EntityBase<int>
    {
        public DinnerParty(int id, DateTime date, string location)
        {
            ID = id;
            Number = 0;
            Date = date;
            Location = location;
            Guests = new List<PartyUser>();
            Albums = new List<NameHrefEntity<int>>();
        }

        public DinnerParty(int id, int number, DateTime date, string location)
        {
            ID = id;
            Number = number;
            Date = date;
            Location = location;
            Guests = new List<PartyUser>();
            Albums = new List<NameHrefEntity<int>>();
        }

        public int Number { get; set; }
        public int Year { get => Date.Year; }
        public DateTime Date { get; set; }
        public string DateString
        {
            get
            {
                if (Date < DateTime.Now)
                    return Date.ToString("d. MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
                else
                    return Date.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("is-IS"));
            }
        }
        public string Location { get; set; }
        public string? Theme { get; set; }
        public int GuestCount { get; set; }
        public ImageHrefEntity? CoverImage { get; set; }

        public IList<PartyUser> Guests { get; set; }

        public IList<NameHrefEntity<int>> Albums { get; set; }

        public override string Name { get => $"Matar- og Rauðvínskvöld {Year}"; }


    }
}
