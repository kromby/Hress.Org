using Azure.Data.Tables;
using Ez.Hress.Administration.DataAccess;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Hardhead.DataAccess;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
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

            ConfigureServices(builder.Services, config);
        }

        public void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
        {
            var tableConnectionString = config["Ez.Hress.AzureStorage.TableConnectionString"];
            var dbConnectionString = config["Ez.Hress.Database.ConnectionString"];

            var key = config["Ez.Hress.Shared.Authentication.Key"];
            var issuer = config["Ez.Hress.Shared.Authentication.Issuer"];
            var audience = config["Ez.Hress.Shared.Authentication.Audience"];
            var salt = config["Ez.Hress.Shared.Authentication.Salt"];

            services.AddSingleton(new DbConnectionInfo(dbConnectionString));

            services.AddSingleton(new AuthenticationInfo(key, issuer, audience, salt));
            services.AddSingleton<IAuthenticationDataAccess, AuthenticationSqlAccess>();
            services.AddSingleton<AuthenticationInteractor>();

            services.AddSingleton<IUserDataAccess, UserSqlDataAccess>();
            services.AddSingleton<IUserInteractor, UserInteractor>();

            services.AddSingleton<IMenuDataAccess, MenuSqlDataAccess>();
            services.AddSingleton<MenuInteractor>();

            services.AddSingleton(new TableClient(tableConnectionString, "HardheadNominations"));
            services.AddSingleton<AwardNominateInteractor>();
            services.AddSingleton<IAwardNominateDataAccess, AwardNominateTableDataAccess>();
            services.AddSingleton<AwardNominationInteractor>();
            services.AddSingleton<IAwardNominationDataAccess, AwardNominateTableDataAccess>();
        }
    }
}
