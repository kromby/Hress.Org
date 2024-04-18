using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IAwardDataAccess
{
    Task<IList<Award>> GetAwards(int? year = null);
}
