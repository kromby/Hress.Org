using Ez.Hress.Hardhead.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IHardheadElectionDataAccess
{
    Task<int> SaveVote(Vote entity);
}
