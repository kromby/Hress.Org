using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Hardhead.UseCases;

public class HardheadInteractor
{
    private readonly IHardheadDataAccess _hardheadDataAccess;
    private readonly ILogger<HardheadInteractor> _log;
    private readonly string _class = nameof(HardheadInteractor);
    public HardheadInteractor(IHardheadDataAccess hardheadDataAccess, ILogger<HardheadInteractor> log)
    {
        _hardheadDataAccess = hardheadDataAccess;
        _log = log;
    }

    public async Task<HardheadNight> GetHardheadAsync(int id)
    {
        _log.LogInformation("[{Class}] Getting Hardhead '{ID}'", _class, id);
        return await _hardheadDataAccess.GetHardhead(id);
    }

    public async Task<IList<HardheadNight>> GetHardheadsAsync(DateTime fromDate)
    {
        _log.LogInformation("[{Class}] Getting all Hardheads from '{from}' until now", _class, fromDate);
        return await _hardheadDataAccess.GetHardheads(fromDate, DateTime.UtcNow.AddDays(-1));
    }

    public async Task<IList<HardheadNight>> GetHardheadsAsync(int parentID)
    {
        _log.LogInformation("[{Class}] Getting all Hardheads for parent '{ParentID}'", _class, parentID);
        return await _hardheadDataAccess.GetHardheads(parentID);
    }

    public async Task<IList<HardheadNight>> GetHardheadsByMovieAsync(string nameAndActorFilter)
    {
        _log.LogInformation("[{Class}] Getting all Hardheads by movie filter '{Filter}'", _class, nameAndActorFilter);
        return await _hardheadDataAccess.GetHardheadsByMovieFilterAsync(nameAndActorFilter);
    }

    public async Task<IList<HardheadNight>> GetNextHardheadAsync()
    {
        var list = await _hardheadDataAccess.GetHardheads(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddMonths(2));
        return list;
    }

    public async Task<IList<HardheadNight>> GetHardheadsAsync(int userID, UserType type)
    {
        var idList = await _hardheadDataAccess.GetHardheadIDs(userID, type).ConfigureAwait(false);

        if (idList != null && idList.Any())
        {
            return await _hardheadDataAccess.GetHardheads(idList).ConfigureAwait(false);
        }
        return new List<HardheadNight>();
    }

    public async Task SaveHardheadAsync(HardheadNight night, int userID)
    {
        night.Validate();

        var oldNight = await _hardheadDataAccess.GetHardhead(night.ID);
        if (oldNight.Date.Year != night.Date.Year || oldNight.Date.Month != night.Date.Month)
        {
            throw new ArgumentException("Ekki má breyta ári eða mánuði harðhausakvölds.", nameof(night));
        }

        // It's handled in the stored procedure to do nothing when description is empty.
        if (night.Description == oldNight.Description)
            night.Description = string.Empty;

        night.Updated = DateTime.UtcNow;
        night.UpdatedBy = userID;

        if (!await _hardheadDataAccess.AlterHardhead(night))
            throw new SystemException("Saving failed");

        if (night.NextHostID.HasValue && night.NextHostID.Value != night.Host.ID)
        {
            var tempDate = night.Date.AddMonths(1);
            var nextFirstDate = new DateTime(tempDate.Year, tempDate.Month, 1);
            var nextDate = new DateTime(tempDate.Year, tempDate.Month, DateTime.DaysInMonth(tempDate.Year, tempDate.Month));

            var nextHardheadList = await _hardheadDataAccess.GetHardheads(nextFirstDate, nextDate);

            if (nextHardheadList.Count == 0 && !await _hardheadDataAccess.CreateHardhead(night.NextHostID.Value, nextDate, userID, DateTime.UtcNow))
            {
                throw new SystemException("Creating new hardhead failed");
            }
        }
    }

    public async Task<IList<ComponentEntity>> GetActionsAsync(int hardheadID, int userID)
    {
        var actionsTask = _hardheadDataAccess.GetActions(hardheadID);
        var hardhead = await _hardheadDataAccess.GetHardhead(hardheadID).ConfigureAwait(false);
        if (userID == hardhead.Host.ID || userID == 2630)
        {
            actionsTask.Wait();
            return actionsTask.Result;
        }

        return new List<ComponentEntity>();

        //Some ideas regarding implementation
        //To start with the only action will be change the hardhead
        //Future actions could be "I was there", "Tilnefna vonbrigði"...
    }

    public async Task<IList<UserBasicEntity>> GetGuestsAsync(int hardheadID)
    {
        return await _hardheadDataAccess.GetGuests(hardheadID);
    }

    public async Task<int> AddGuestAsync(int id, int guestId, int userId)
    {
        var list = await _hardheadDataAccess.GetGuests(guestId);

        if (list.Any(g => g.ID == guestId))
        {
            throw new SystemException("Guest already registered.");
        }

        return await _hardheadDataAccess.AddGuest(id, guestId, userId, DateTime.UtcNow);
    }

    public async Task<IList<YearEntity>> GetYearsAsync()
    {
        _log.LogInformation("[{Class}] Getting all Hardhead years", _class);
        return await _hardheadDataAccess.GetYears();
    }

    public async Task<IList<HardheadUserEntity>> GetUsersByYearAsync(int yearId, int minAttendance = 0)
    {
        _log.LogInformation("[{Class}] Getting users for year {yearId} with minimum attendance {minAttendance}", _class, yearId, minAttendance);
        var users = await _hardheadDataAccess.GetUsersByYear(yearId);

        if (minAttendance > 0)
        {
            users = users.Where(u => u.Attended >= minAttendance).ToList();
        }

        return users;
    }

    public async Task<RatingEntity?> GetRatingAsync(int ID, int? userID = null, DateTime? date = null)
    {
        var currentDate = date.HasValue ? date : DateTime.UtcNow;

        var night = await _hardheadDataAccess.GetHardhead(ID).ConfigureAwait(false);

        var ratings = new RatingEntity()
        {
            ID = ID,
            UserID = userID
        };

        ratings.Ratings = new List<RatingInfo>
    {
        new RatingInfo("Einkunn", "REP_C_RTNG"),
        new RatingInfo("Einkunn myndar", "REP_C_MRTNG")
    }; // TODO Read from database GEN_TYPE table WHERE GroupType = 330

        if (night.Date.Year == currentDate.Value.Year || (currentDate.Value.Month == 1 && night.Date.Year == currentDate.Value.Year - 1))
        {
            if (!userID.HasValue)
                return null;

            var list = await _hardheadDataAccess.GetGuests(ID);
            if (list != null && list.Any() && list.Count(g => g.ID == userID.Value) > 0) // User attended night
            {
                var myRating = await _hardheadDataAccess.GetMyRatingAsync(ID, userID.Value).ConfigureAwait(false);
                foreach (var rating in ratings.Ratings)
                {
                    if (myRating.TryGetValue(rating.Code, out var currentMyRating))
                        rating.MyRating = currentMyRating;
                }
            }
            else // user is host or did not attend
            {
                var result = await _hardheadDataAccess.GetAverageRatingAsync(ID);
                ratings.Readonly = true;

                foreach (var rating in ratings.Ratings)
                {
                    if (result.TryGetValue(rating.Code, out var ratingInfo))
                        rating.NumberOfRatings = ratingInfo.NumberOfRatings;
                }
            }
            return ratings;
        }
        else
        {
            ratings.Readonly = true;

            Task<IDictionary<string, RatingInfo>> avgRatingsTask = _hardheadDataAccess.GetAverageRatingAsync(ID);
            Task<IDictionary<string, int>>? myRatingTask = null;
            if (userID.HasValue)
            {
                myRatingTask = _hardheadDataAccess.GetMyRatingAsync(ID, userID.Value);
            }
            var result = await avgRatingsTask;

            foreach (var rating in ratings.Ratings)
            {
                if (result.TryGetValue(rating.Code, out var ratingInfo))
                {
                    rating.NumberOfRatings = ratingInfo.NumberOfRatings;
                    rating.AverageRating = ratingInfo.AverageRating.HasValue ? Math.Round(ratingInfo.AverageRating.Value, 1) : ratingInfo.AverageRating;
                }

            }

            if (myRatingTask != null)
            {
                var myRatings = await myRatingTask;
                foreach (var rating in ratings.Ratings)
                {
                    if (myRatings.TryGetValue(rating.Code, out int myRating))
                        rating.MyRating = myRating;

                }
            }

            return ratings;
        }
    }

}
