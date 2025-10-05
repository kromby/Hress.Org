using Azure.Data.Tables;
using Ez.Hress.Administration.DataAccess;
using Ez.Hress.Administration.UseCases;
using Ez.Hress.Albums.DataAccess;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.FunctionsApi;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

var config = new ConfigurationBuilder()
           .AddJsonFile("host.json", optional: true)
           .AddEnvironmentVariables()
           .Build();

string dbConnectionString = config["Ez.Hress.Database.ConnectionString"];
DbConnectionInfo dbConnectionInfo = new(dbConnectionString);
string contentStorageConnectionString = config["Ez.Hress.Shared.ContentStorage.ConnectionString"];

string key = config["Ez.Hress.Shared.Authentication.Key"];
string issuer = config["Ez.Hress.Shared.Authentication.Issuer"];
string audience = config["Ez.Hress.Shared.Authentication.Audience"];
string salt = config["Ez.Hress.Shared.Authentication.Salt"];

IConfigurationDataAccess configurationDataAccess = new ConfigurationSqlAccess(dbConnectionInfo);
config = new ConfigurationBuilder()
    .AddConfiguration(config)
    .Add(new HressConfigurationSource(configurationDataAccess))
    .Build();

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddMvcCore().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
        });

       ConfigureServices(services, config);
    })
.ConfigureAppConfiguration((hostContext, config) => //This is for facilitating the logging functionality.
{
    config.AddJsonFile("host.json", optional: true)
    .AddEnvironmentVariables()
    .Add(new HressConfigurationSource(configurationDataAccess));
})
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddApplicationInsights(console =>
        {
            console.IncludeScopes = true;
        });

        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    }).ConfigureLogging(logging => //This is for facilitating the logging functionality in Application Insights.
                                   // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override. // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs


    {
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options?.Rules?.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();

void ConfigureServices(IServiceCollection services, IConfigurationRoot config)
{
    // Connection details
    services.AddSingleton(dbConnectionInfo);
    services.AddSingleton(new BlobConnectionInfo(contentStorageConnectionString));

    // Types
    services.AddSingleton<ITypeDataAccess, TypeSqlAccess>();
    services.AddSingleton<ITypeInteractor, TypeInteractor>();

    // Clients
    services.AddSingleton(new TableClient(contentStorageConnectionString, "DinnerPartyElection"));
    services.AddSingleton(new TableClient(contentStorageConnectionString, "HardheadVotes"));

    // Register the list of TableClients
    services.AddSingleton<IList<TableClient>>(sp => 
        sp.GetServices<TableClient>().ToList());

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
    services.AddSingleton<IImageContentDataAccess, MediaContentBlobDataAccess>();
    services.AddSingleton<IList<IImageContentDataAccess>>(sp => 
        sp.GetServices<IImageContentDataAccess>().ToList());
    services.AddSingleton<ImageInteractor>();

    // Video
    services.AddSingleton<IVideoContentDataAccess, MediaContentBlobDataAccess>();
    services.AddSingleton<IVideoInfoDataAccess, VideoInfoTableAccess>();
    services.AddSingleton<VideoInteractor>();

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
    services.AddSingleton<HardheadAwardInteractor>();
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

    // Translation
    services.AddHttpClient();
    services.AddSingleton<ITranslationDataAccess, TranslationSqlDataAccess>();
    services.AddSingleton<TranslationService>();
}
