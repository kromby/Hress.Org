using Ez.Hress.Shared.Entities;
using Ez.Hress.UserProfile.Entities;

namespace Ez.Hress.UserProfile.UseCases;

public interface IUserProfileDataAccess
{
    Task<UserBasicEntity?> GetUser(int userID);

    Task<IList<Transaction>> GetTransactions(int userID);

    Task<IList<Relation>> GetRelations(int userID);
}
