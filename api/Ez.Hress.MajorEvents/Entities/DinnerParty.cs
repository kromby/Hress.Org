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
        //SELECT ROW_NUMBER() OVER(ORDER BY dinner.Inserted ASC) 'Number', dinner.Id, dinner.Date, 
        //  dinner.Inserted, dinner.InsertedBy, tLocation.TextValue 'Location', theme.TextValue 'Theme', img.ImageId, gImg.Description
        //FROM    rep_Event dinner
        //JOIN rep_Text tLocation ON tLocation.EventId = dinner.Id AND tLocation.TypeId = 67
        //LEFT OUTER JOIN rep_Text theme ON theme.EventId = dinner.Id AND theme.TypeId = 194
        //LEFT OUTER JOIN rep_Image img ON img.EventId = dinner.Id AND img.TypeId = 14
        //LEFT OUTER JOIN gen_Image gImg ON gImg.Id = img.ImageId
        //WHERE dinner.TypeId = 56 AND dinner.ParentId IS NULL
        //ORDER BY dinner.Inserted DESC

        public DinnerParty(int id, int number, DateTime date, string location)
        {
            ID = id;
            Number = number;
            Date = date;
            Location = location;
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

        public override string Name { get => $"Matar- og Rauðvínskvöld {Year}"; }


    }
}
