using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.MajorEvents.Entities
{
    public class PartyQuiz
    {
        public PartyQuiz(int id, string question, string answer)
        {
            ID = id;
            Question = question;
            Answer = answer;
        }

        public int ID { get; set; }
        public string Question { get; set; }

        public  string Answer { get; set; }
    }
}
