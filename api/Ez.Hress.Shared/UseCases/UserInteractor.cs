﻿using Ez.Hress.Shared.Entities;
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
}
