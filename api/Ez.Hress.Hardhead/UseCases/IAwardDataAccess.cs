using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IAwardDataAccess
    {
        Task<IList<Award>> GetAwards(int? year = null);
    }
}
