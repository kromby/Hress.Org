using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IAwardDataAccess
{
    Task<IList<Award>> GetAwards(int? year = null);
    Task<Award> GetAward(int id);
    Task<IList<WinnerEntity>> GetAwardWinners(int awardId, int? year = null, int? position = null);
}
