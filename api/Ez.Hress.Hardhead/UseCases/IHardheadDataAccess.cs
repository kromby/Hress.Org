using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IHardheadDataAccess
{
    Task<IList<HardheadUser>> GetHardheadUsers(int yearID);

    Task<HardheadNight> GetHardhead(int id);

    Task<IList<HardheadNight>> GetHardheads(DateTime fromDate, DateTime toDate);

    Task<IList<HardheadNight>> GetHardheads(int parentID);

    /// <summary>
    /// Nights dated on <paramref name="day"/>, at any time that day. Only ID, Number, Date, Host,
    /// Location, Inserted and Updated are filled in.
    /// </summary>
    Task<IList<HardheadNight>> GetHardheadsOnDay(DateTime day);

    /// <summary>Distinct user IDs of the hosts and guests of nights dated after fromDate and on or before toDate.</summary>
    Task<IList<int>> GetAttendeeIDs(DateTime fromDate, DateTime toDate);

    Task<IList<HardheadNight>> GetHardheads(IList<HardheadNight> idList);

    Task<IList<HardheadNight>> GetHardheadIDs(int userID, UserType? type);

    Task<IList<HardheadNight>> GetHardheadsByMovieFilterAsync(string nameAndActorFilter);

    Task<bool> AlterHardhead(HardheadNight hardhead);

    /// <summary>Sets the night's location, or removes it when <paramref name="location"/> is empty.</summary>
    Task SaveLocation(int hardheadID, string location, int userID, DateTime changeDate);

    Task<bool> CreateHardhead(int hostID, DateTime nextDate, int currentUserID, DateTime changeDate);

    Task<IList<ComponentEntity>> GetActions(int hardheadID);

    Task<IList<UserBasicEntity>> GetGuests(int hardheadID);

    Task<IDictionary<int, IList<UserBasicEntity>>> GetGuestsForHardheads(IList<int> hardheadIDs);

    Task<int> AddGuest(int hardheadId, int guestId, int userId, DateTime createdDate);

    Task<int> RemoveGuest(int hardheadID, int guestID);

    Task<IList<YearEntity>> GetYears();

    Task<IList<HardheadUserEntity>> GetUsersByYear(int yearId);

    Task<IDictionary<string, int>> GetMyRatingAsync(int id, int userId);

    Task<IDictionary<string, RatingInfo>> GetAverageRatingAsync(int Id);

    Task<int> InsertRatingAsync(int eventId, int userId, string typeCode, int rating);

    Task<int> UpdateRatingAsync(int eventId, int userId, string typeCode, int rating);
}
