namespace Ez.Hress.Shared.UseCases;

public interface IConfigurationDataAccess
{
    Task<IDictionary<string, string>> GetConfiguration();
}
