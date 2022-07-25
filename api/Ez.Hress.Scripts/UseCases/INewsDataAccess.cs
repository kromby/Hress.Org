using Ez.Hress.Scripts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Scripts.UseCases
{
    public interface INewsDataAccess
    {
        Task<IList<News>> GetLatestNews(int top);

        Task<News> GetNews(int id);

        Task<IList<News>> GetNews(DateTime date, bool useDay, bool useMonth, bool useYear);

        Task<IList<StatisticNewsByDate>> GetNewsStatisticsByDate(int? year);
    }
}
