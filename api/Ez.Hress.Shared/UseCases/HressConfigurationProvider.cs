using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public class HressConfigurationProvider : ConfigurationProvider
    {
        private readonly IConfigurationDataAccess _configurationDataAccess;
        public HressConfigurationProvider(IConfigurationDataAccess configDataAccess)
        {
            _configurationDataAccess = configDataAccess;
        }

        public override void Load()
        {
            var task = _configurationDataAccess.GetConfiguration();
            task.Wait();
            Data = task.Result;
        }
    }
}
