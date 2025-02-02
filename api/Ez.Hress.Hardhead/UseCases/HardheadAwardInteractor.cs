using Ez.Hress.Hardhead.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

public class HardheadAwardInteractor
{
    private readonly IAwardDataAccess _awardDataAccess;
    private readonly ILogger<HardheadAwardInteractor> _log;
    private readonly string _class = nameof(HardheadAwardInteractor);

    public HardheadAwardInteractor(IAwardDataAccess awardDataAccess, ILogger<HardheadAwardInteractor> log)
    {
        _awardDataAccess = awardDataAccess;
        _log = log;
    }

    public async Task<Award> GetAwardAsync(int id)
    {
        _log.LogInformation("[{Class}] Getting Award '{ID}'", _class, id);
        return await _awardDataAccess.GetAward(id);
    }

    public async Task<IList<Award>> GetAwardsAsync(int? year = null)
    {
        if (year.HasValue)
            _log.LogInformation("[{Class}] Getting Awards for year '{Year}'", _class, year.Value);
        else
            _log.LogInformation("[{Class}] Getting all Awards", _class);

        return await _awardDataAccess.GetAwards(year);
    }
}
