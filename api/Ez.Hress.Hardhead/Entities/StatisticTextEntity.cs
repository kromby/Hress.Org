using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.Entities
{
    public class StatisticTextEntity : StatisticBase
    {
        public StatisticTextEntity(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
    }
}
