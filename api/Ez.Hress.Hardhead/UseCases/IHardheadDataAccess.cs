﻿using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Hardhead.UseCases;

public interface IHardheadDataAccess
{
    Task<IList<HardheadUser>> GetHardheadUsers(int yearID);

    Task<HardheadNight> GetHardhead(int id);

    Task<IList<HardheadNight>> GetHardheads(DateTime fromDate, DateTime toDate);

    Task<IList<HardheadNight>> GetHardheads(int parentID);

    Task<IList<HardheadNight>> GetHardheads(IList<HardheadNight> idList);

    Task<IList<HardheadNight>> GetHardheadIDs(int userID, UserType? type);

    Task<IList<HardheadNight>> GetHardheadsByMovieFilterAsync(string nameAndActorFilter);

    Task<bool> AlterHardhead(HardheadNight hardhead);

    Task<bool> CreateHardhead(int hostID, DateTime nextDate, int currentUserID, DateTime changeDate);

    Task<IList<ComponentEntity>> GetActions(int hardheadID);

    Task<IList<UserBasicEntity>> GetGuests(int hardheadID);

    Task<int> AddGuest(int hardheadId, int guestId, int userId, DateTime createdDate);

    Task<IList<YearEntity>> GetYears();

    Task<IList<HardheadUserEntity>> GetUsersByYear(int yearId);

    Task<IDictionary<string, int>> GetMyRatingAsync(int id, int userId);

    Task<IDictionary<string, RatingInfo>> GetAverageRatingAsync(int Id);
}
