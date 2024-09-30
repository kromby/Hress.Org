using Azure.Data.Tables;
using Ez.Hress.Administration.DataAccess;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Albums.DataAccess;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Hardhead.DataAccess;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.MajorEvents.DataAccess;
using Ez.Hress.MajorEvents.UseCases;
using Ez.Hress.Scripts.DataAccess;
using Ez.Hress.Scripts.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Ez.Hress.UserProfile.DataAccess;
using Ez.Hress.UserProfile.UseCases;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: FunctionsStartup(typeof(Ez.Hress.FunctionsApi.Startup))]

namespace Ez.Hress.FunctionsApi;

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

    public static void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
    {
        var dbConnectionString = config["Ez.Hress.Database.ConnectionString"];
        DbConnectionInfo dbConnectionInfo = new(dbConnectionString);
        var contentStorageConnectionString = config["Ez.Hress.Shared.ContentStorage.ConnectionString"];            

        var key = config["Ez.Hress.Shared.Authentication.Key"];
        var issuer = config["Ez.Hress.Shared.Authentication.Issuer"];
        var audience = config["Ez.Hress.Shared.Authentication.Audience"];
        var salt = config["Ez.Hress.Shared.Authentication.Salt"];

        IConfigurationDataAccess configurationDataAccess = new ConfigurationSqlAccess(dbConnectionInfo);
        config = new ConfigurationBuilder()
            .AddConfiguration(config)
            .Add(new HressConfigurationSource(configurationDataAccess))
            .Build();

        services.AddSingleton<IConfiguration>(config);

        services.AddMvcCore().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        });

        // Connection details
        services.AddSingleton(dbConnectionInfo);
        services.AddSingleton(new BlobConnectionInfo(contentStorageConnectionString));

        // Types
        services.AddSingleton<ITypeDataAccess, TypeSqlAccess>();
        services.AddSingleton<ITypeInteractor, TypeInteractor>();

        // Clients
        services.AddSingleton(new TableClient(contentStorageConnectionString, "DinnerPartyElection"));
        services.AddSingleton(new TableClient(contentStorageConnectionString, "HardheadVotes"));

        // Authentication
        services.AddSingleton(new AuthenticationInfo(key, issuer, audience, salt));
        services.AddSingleton<IAuthenticationDataAccess, AuthenticationSqlAccess>();
        services.AddSingleton<AuthenticationInteractor>();

        // User
        services.AddSingleton<IUserDataAccess, UserSqlDataAccess>();
        services.AddSingleton<IUserInteractor, UserInteractor>();

        services.AddSingleton<IUserProfileDataAccess, UserProfileSqlDataAccess>();
        services.AddSingleton<UserProfileInteractor>();

        // Menu
        services.AddSingleton<IMenuDataAccess, MenuSqlDataAccess>();
        services.AddSingleton<MenuInteractor>();

        // News
        services.AddSingleton<INewsDataAccess, NewsSqlDataAccess>();
        services.AddSingleton<NewsInteractor>();

        // Dinner Party
        services.AddSingleton<IDinnerPartyDataAccess, DinnerPartySqlDataAccess>();
        services.AddSingleton<DinnerPartyInteractor>();

        // Image
        services.AddSingleton<IImageInfoDataAccess, ImageInfoSqlDataAccess>();
        services.AddSingleton<IImageContentDataAccess, ImageContentHttpDataAccess>();
        services.AddSingleton<IImageContentDataAccess, ImageContentRelativeDataAccess>();
        services.AddSingleton<IImageContentDataAccess, ImageContentBlobDataAccess>();
        services.AddSingleton<ImageInteractor>();

        // Albums
        services.AddSingleton<IAlbumDataAccess, AlbumSqlDataAccess>();
        services.AddSingleton<AlbumInteractor>();

        // Election (Shared)
        services.AddSingleton<ElectionInteractor>();
        services.AddSingleton<IElectionVoterDataAccess, ElectionSqlAccess>();
        services.AddSingleton<IElectionVoteDataAccess, ElectionVoteTableAccess>();

        // Hardhead
        services.AddSingleton<HardheadInteractor>();
        services.AddSingleton<HardheadParser>();
        services.AddSingleton<IHardheadDataAccess, HardheadSqlAccess>();        

        // Hardhead - Movie
        services.AddSingleton<MovieInteractor>();
        services.AddSingleton<IMovieDataAccess, MovieSqlAccess>();
        services.AddSingleton<IMovieInformationDataAccess, MovieInformationDataAccess>();

        // Hardhead Election
        services.AddSingleton<HardheadElectionInteractor>();
        services.AddSingleton<IHardheadElectionDataAccess, HardheadElectionSqlAccess>();

        // Hardhead - Award
        services.AddSingleton<IAwardDataAccess, AwardSqlAccess>();
        services.AddSingleton<AwardNominateInteractor>();
        services.AddSingleton<IAwardNominateDataAccess, AwardNominateTableDataAccess>();
        services.AddSingleton<AwardNominationInteractor>();
        services.AddSingleton<IAwardNominationDataAccess, AwardNominateTableDataAccess>();

        // Hardhead - Stats
        services.AddSingleton<IHardheadStatisticsDataAccess, HardheadStatisticSqlDataAccess>();
        services.AddSingleton<HardheadStatisticsInteractor>();

        // Hardhead - Rules
        services.AddSingleton<IRuleChangeDataAccess, RuleChangeTableDataAccess>();
        services.AddSingleton<IRuleDataAccess, RuleSqlDataAccess>();
        services.AddSingleton<RuleInteractor>();

        // Hardhead - PostElection
        services.AddSingleton<PostElectionInteractor>();
    }
}
