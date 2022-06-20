using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class DbConnectionInfo
    {
        public DbConnectionInfo(string connectionString)
        {
            ConnectionString = connectionString;
        }        
        
        public string ConnectionString  { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new ArgumentNullException(nameof(ConnectionString));
        }
    }
}
