using Ez.Hress.Shared.Entities;
using Ez.Hress.UserProfile.Entities;

namespace Ez.Hress.UserProfile.UseCases;

public interface IUserProfileDataAccess
{
    Task<UserBasicEntity?> GetUser(int userID);

    Task<IList<Transaction>> GetTransactions(int userID, bool includePaid);

    Task<IList<Relation>> GetRelations(int userID);

    Task<Lookup?> GetLookup(int id);

    Task<Lookup?> GetLookupByUserAndType(int userId, int typeId);

    Task<int> CreateLookup(Lookup lookup);

    Task<int> UpdateLookup(Lookup lookup);

    Task<int> DeleteLookup(int id, int deletedBy);
}
