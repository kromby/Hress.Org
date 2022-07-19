using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.Entities
{
    public class BlobConnectionInfo
    {
        public BlobConnectionInfo(string connectionString)
        {
            ConnectionString = connectionString;
        }
        
        public string ConnectionString { get; set; }
    }
}
