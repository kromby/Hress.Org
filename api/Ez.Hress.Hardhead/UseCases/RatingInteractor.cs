namespace Ez.Hress.Hardhead.UseCases;

public class RatingInteractor
{
    //private readonly IRatingDataAccess _ratingDataAccess;
    //private readonly HardheadInteractor _hardheadInteractor;
    //private readonly ILogger<RatingInteractor> _log;

    //public RatingInteractor(IRatingDataAccess ratingDataAccess, HardheadInteractor hardheadInteractor, ILogger<RatingInteractor> log)
    //{
    //    _ratingDataAccess = ratingDataAccess;
    //    _hardheadInteractor = hardheadInteractor;
    //    _log = log;
    //}

    //public async Task<RatingEntity> GetRating(int ID, int? userID = null, DateTime? date = null)
    //{
    //    var currentDate = date.HasValue ? date : DateTime.Now;

    //    var night = await _hardheadInteractor.GetHardhead(ID);

    //    var ratings = new RatingEntity()
    //    {
    //        ID = ID,
    //        UserID = userID
    //    };

    //    ratings.Ratings = new List<RatingInfo>(); // TODO Read from database GEN_TYPE table WHERE GroupType = 330
    //    ratings.Ratings.Add(new RatingInfo("Einkunn", "REP_C_RTNG"));
    //    ratings.Ratings.Add(new RatingInfo("Einkunn myndar", "REP_C_MRTNG"));

    //    if (night.Date.Year == currentDate.Value.Year || (currentDate.Value.Month == 1 && night.Date.Year == currentDate.Value.Year - 1))
    //    {
    //        if (!userID.HasValue)
    //            return null;
    //        else
    //        {
    //            var list = _hardheadInteractor.GetGuests(ID);

    //            if (list != null && list.Any() && list.Where(g => g.ID == userID.Value).Any()) // User attended night
    //            {
    //                var myRating = await _ratingDataAccess.GetMyRating(ID, userID.Value).ConfigureAwait(false);
    //                foreach (var rating in ratings.Ratings)
    //                {
    //                    if (myRating.ContainsKey(rating.Code))
    //                        rating.MyRating = myRating[rating.Code];
    //                }
    //                return ratings;
    //            }
    //            else // user is host or did not attend
    //            {
    //                Task<IDictionary<string, RatingInfo>> avgRatingsTask = _ratingDataAccess.GetAverageRating(ID);
    //                ratings.Readonly = true;
    //                avgRatingsTask.Wait();
    //                var result = avgRatingsTask.Result;

    //                foreach (var rating in ratings.Ratings)
    //                {
    //                    if (result.ContainsKey(rating.Code))
    //                        rating.NumberOfRatings = result[rating.Code].NumberOfRatings;
    //                }

    //                return ratings;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        ratings.Readonly = true;

    //        Task<IDictionary<string, RatingInfo>> avgRatingsTask = _ratingDataAccess.GetAverageRating(ID);
    //        Task<IDictionary<string, int>> myRatingTask = null;
    //        if (userID.HasValue)
    //        {
    //            myRatingTask = _ratingDataAccess.GetMyRating(ID, userID.Value);
    //        }
    //        avgRatingsTask.Wait();
    //        var result = avgRatingsTask.Result;

    //        foreach (var rating in ratings.Ratings)
    //        {
    //            if (result.ContainsKey(rating.Code))
    //            {
    //                rating.NumberOfRatings = result[rating.Code].NumberOfRatings;
    //                rating.AverageRating = result[rating.Code].AverageRating.HasValue ? Math.Round(result[rating.Code].AverageRating.Value, 1) : result[rating.Code].AverageRating;
    //            }

    //        }

    //        if (myRatingTask != null)
    //        {
    //            myRatingTask.Wait();
    //            foreach (var rating in ratings.Ratings)
    //            {
    //                if (myRatingTask.Result.ContainsKey(rating.Code))
    //                    rating.MyRating = myRatingTask.Result[rating.Code];
    //            }
    //        }

    //        return ratings;
    //    }
    //}
}
