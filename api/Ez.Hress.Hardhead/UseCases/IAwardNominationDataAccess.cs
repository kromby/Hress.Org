using Ez.Hress.Hardhead.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.UseCases
{
    public interface IAwardNominationDataAccess
    {
        /// <summary>
        /// Gets all nominations with a specific type. 
        /// </summary>
        /// <param name="typeID">Unique identifier of the type.</param>
        /// <returns>A list of nominations.</returns>
        Task<IList<Nomination>> GetNominations(int typeID);
    }
}
