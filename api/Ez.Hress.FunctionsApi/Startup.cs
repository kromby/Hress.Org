using Azure.Data.Tables;
using Ez.Hress.Hardhead.DataAccess;
using Ez.Hress.Hardhead.UseCases;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Ez.Hress.FunctionApi.Startup))]

namespace Ez.Hress.FunctionApi
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddLogging();

            //Microsoft.Extensions.Configuration.

            //var connectionString = Configuration.GetConnectionString("TableConnectionString");
            var connectionString = "";



            builder.Services.AddSingleton<TableClient>(new TableClient(connectionString, "HardheadNominations"));
            builder.Services.AddScoped<AwardInteractor>();
            builder.Services.AddScoped<IAwardDataAccess, AwardTableDataAccess>();             
        }
    }   
}
