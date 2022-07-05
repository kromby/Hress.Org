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
        Task<IList<News>> GetNews(int top);
    }
}
