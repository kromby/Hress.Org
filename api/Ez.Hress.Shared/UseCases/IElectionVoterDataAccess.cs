using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IElectionVoterDataAccess
{
    Task<VoterEntity?> GetVoter(int userID);

    Task<int> SaveVoter(VoterEntity voter);        
}
