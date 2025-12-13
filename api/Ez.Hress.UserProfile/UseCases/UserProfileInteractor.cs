using Ez.Hress.Shared.Entities;
using Ez.Hress.UserProfile.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.UserProfile.UseCases;

public class UserProfileInteractor
{
    private readonly IUserProfileDataAccess _userDataAccess;
    private readonly ILogger<UserProfileInteractor> _log;

    public UserProfileInteractor(IUserProfileDataAccess userDataAccess, ILogger<UserProfileInteractor> log)
    {
        _userDataAccess = userDataAccess;
        _log = log;
    }

    public async Task<UserBasicEntity?> GetUserAsync(int userID)
    {
        return await _userDataAccess.GetUser(userID);
    }

    public async Task<BalanceSheet> GetBalanceSheetAsync(int userID, bool includePaid = false)
    {
        _log.LogInformation("[{Class}] GetBalanceSheet", GetType().Name);

        var transactionTask = _userDataAccess.GetTransactions(userID, includePaid);

        var relations = await _userDataAccess.GetRelations(userID);
        var entity = new BalanceSheet() { UserID = userID };

        transactionTask.Wait();
        entity.Transactions = transactionTask.Result;

        foreach (var relation in relations)
        {                
            var transactions = await _userDataAccess.GetTransactions(relation.RelatedUser.ID, includePaid);
            entity.Transactions = entity.Transactions.Union(transactions).ToList();
        }

        if (entity.Transactions.Count > 0)
        {
            entity.Transactions = entity.Transactions.OrderBy(t => t.Inserted).ToList();
            entity.Balance = entity.Transactions.Where(t => t.Deleted == null).Sum(t => t.Amount);
        }

        return entity;
    }

    public async Task<Lookup?> GetLookupAsync(int id)
    {
        _log.LogInformation("[{Class}] GetLookup id: {id}", GetType().Name, id);
        return await _userDataAccess.GetLookup(id);
    }

    public async Task<Lookup?> GetLookupByUserAndTypeAsync(int userId, int typeId)
    {
        _log.LogInformation("[{Class}] GetLookupByUserAndType userId: {userId}, typeId: {typeId}", GetType().Name, userId, typeId);
        return await _userDataAccess.GetLookupByUserAndType(userId, typeId);
    }

    public async Task<int> CreateLookupAsync(Lookup lookup)
    {
        _log.LogInformation("[{Class}] CreateLookup UserId: {UserId}, TypeId: {TypeId}, ValueId: {ValueId}", 
            GetType().Name, lookup.UserId, lookup.TypeId, lookup.ValueId);

        if (lookup.Inserted == default(DateTime))
        {
            lookup.Inserted = DateTime.UtcNow;
        }

        return await _userDataAccess.CreateLookup(lookup);
    }

    public async Task<int> UpdateLookupAsync(Lookup lookup)
    {
        _log.LogInformation("[{Class}] UpdateLookup Id: {Id}, UserId: {UserId}, TypeId: {TypeId}, ValueId: {ValueId}", 
            GetType().Name, lookup.ID, lookup.UserId, lookup.TypeId, lookup.ValueId);

        if (!lookup.UpdatedBy.HasValue && lookup.InsertedBy > 0)
        {
            lookup.UpdatedBy = lookup.InsertedBy;
        }

        return await _userDataAccess.UpdateLookup(lookup);
    }

    public async Task<int> DeleteLookupAsync(int id, int deletedBy)
    {
        _log.LogInformation("[{Class}] DeleteLookup Id: {Id}, DeletedBy: {DeletedBy}", 
            GetType().Name, id, deletedBy);

        return await _userDataAccess.DeleteLookup(id, deletedBy);
    }
}
