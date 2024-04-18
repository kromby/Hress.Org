using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IElectionVoteDataAccess
{
    Task<bool> SaveVote(VoteEntity vote);

    Task<IList<VoteEntity>> GetVotes(Guid stepID);
}
