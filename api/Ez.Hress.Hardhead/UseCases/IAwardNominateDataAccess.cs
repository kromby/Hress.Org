using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IAwardNominateDataAccess
{
    Task<int> SaveNomination(Nomination nomination);
}
