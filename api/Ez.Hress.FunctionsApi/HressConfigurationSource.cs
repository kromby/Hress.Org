using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.FunctionsApi
{
    internal class HressConfigurationSource : IConfigurationSource
    {
        private readonly IConfigurationDataAccess _configurationDataAccess;

        public HressConfigurationSource(IConfigurationDataAccess configurationDataAccess) 
        { 
            _configurationDataAccess = configurationDataAccess;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new HressConfigurationProvider(_configurationDataAccess);
        }
    }
}
