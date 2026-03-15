using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.UseCases;

public class UserInteractor : IUserInteractor
{
    private readonly IUserDataAccess _userDataAccess;
    private readonly ILogger<UserInteractor> _log;

    public UserInteractor(IUserDataAccess userDataAccess, ILogger<UserInteractor> log)
    {
        _userDataAccess = userDataAccess;
        _log = log;
    }

    public async Task<UserBasicEntity> GetUser(int id)
    {
        _log.LogInformation("[{Class}] Getting user with id {id}", nameof(UserInteractor), id);

        return await _userDataAccess.GetUser(id);
    }

    public async Task<IList<UserBasicEntity>> GetUsers(string? role = null)
    {
        if (!string.IsNullOrEmpty(role))
        {
            _log.LogInformation("[{Class}] Getting users by role {Role}", nameof(UserInteractor), role);
            return await _userDataAccess.GetUsersByRole(role);
        }

        _log.LogInformation("[{Class}] Getting all users", nameof(UserInteractor));
        return await _userDataAccess.GetUsers();
    }
}
