using Azure.Data.Tables;
using Ez.Hress.Hardhead.DataAccess;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Ez.Hress.FunctionsApi.Startup))]

namespace Ez.Hress.FunctionsApi
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("host.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            //builder.Services.AddLogging();

            ConfigureServices(builder.Services, config);
        }

        public void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
        {
            var connectionString = config["TableConnectionString"];

            services.AddSingleton(new TableClient(connectionString, "HardheadNominations"));
            services.AddSingleton<AwardInteractor>();
            services.AddSingleton<IAwardDataAccess, AwardTableDataAccess>();
        }
    }
}
