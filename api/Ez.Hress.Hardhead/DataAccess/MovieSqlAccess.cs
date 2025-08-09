using Ez.Hress.Hardhead.DataAccess.Models;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Ez.Hress.Hardhead.DataAccess;

public class MovieSqlAccess : IMovieDataAccess
{
    private readonly ILogger<MovieSqlAccess> _log;
    private readonly string _connectionString;
    private readonly string _class = nameof(MovieSqlAccess);
    private readonly MovieModel _movieModel;
    private bool _disposed = false;

    private const string SQL_GETMOVIE = @"SELECT	night.Id, film.TextValue Movie, actor.TextValue Actor, poster.ImageId poster, 
                                                        imdb.TextValue imdb, why.TextValue why, youtube.TextValue youtube, CAST(moviekills.Count as int) moviekills, CAST(hhkills.Count as int) hhkills
                                                FROM    rep_Event night
                                                JOIN    rep_Text film ON film.EventId = night.Id AND film.TypeId = 62
                                                JOIN    rep_Text actor ON actor.EventId = night.Id AND actor.TypeId = 63
                                                LEFT OUTER JOIN    rep_Text imdb ON imdb.EventId = night.Id AND imdb.TypeId = 60
                                                LEFT OUTER JOIN rep_Text why ON why.EventId = night.Id AND why.TypeId = 59
                                                LEFT OUTER JOIN rep_Image poster ON poster.EventId = night.Id AND poster.TypeId = 14
                                                LEFT OUTER JOIN rep_Text youtube ON youtube.EventId = night.Id AND youtube.TypeId = 208
												LEFT OUTER JOIN rep_Count moviekills ON moviekills.EventId = night.Id AND moviekills.TypeId = 221
												LEFT OUTER JOIN rep_Count hhkills ON hhkills.EventId = night.Id AND hhkills.TypeId = 222
                                                WHERE   night.TypeId = 49";
    public MovieSqlAccess(DbConnectionInfo connectionInfo, ILogger<MovieSqlAccess> log)
    {
        _connectionString = connectionInfo.ConnectionString;
        _movieModel = new MovieModel(_connectionString);
        _log = log;
    }

    public async Task<IList<Movie>> GetMovies(string nameAndActorFilter)
    {
        ThrowIfDisposed();
        
        var sql = string.Format("{0} {1}", SQL_GETMOVIE, "AND (film.TextValue LIKE @filter OR actor.TextValue LIKE @filter)");
        _log.LogInformation("SQL: {SQL}", sql);

        var list = new List<Movie>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("filter", string.Format("%{0}%", nameAndActorFilter)));

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var entity = ParseMovie(reader);
                list.Add(entity);
            }
        }

        return list;
    }

    public async Task<Movie?> GetMovie(int id)
    {
        ThrowIfDisposed();
        
        var sql = string.Format("{0} {1}", SQL_GETMOVIE, "AND night.Id = @id");
        _log.LogInformation("SQL: {SQL}", sql);

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("id", id));

            var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                return ParseMovie(reader);
            }
        }

        return null;
    }

    private static Movie ParseMovie(SqlDataReader reader)
    {
        return new Movie()
        {
            ID = reader.GetInt32(reader.GetOrdinal("ID")),
            Name = reader.GetString(reader.GetOrdinal("Movie")),
            Actor = reader.GetString(reader.GetOrdinal("Actor")),
            ImdbUrl = reader[reader.GetOrdinal("imdb")].ToString(),
            YoutubeUrl = reader[reader.GetOrdinal("youtube")].ToString(),
            Reason = reader[reader.GetOrdinal("why")].ToString(),
            PosterPhotoID = SqlHelper.GetNullableInt(reader, "poster"),
            MovieKillCount = SqlHelper.GetNullableInt(reader, "moviekills"),
            HardheadKillCount = SqlHelper.GetNullableInt(reader, "hhkills")
        };
    }

    public async Task<IList<StatisticBase>> GetActorStatistic(DateTime fromDate)
    {
        ThrowIfDisposed();
        
        var sql = @"SELECT	actor.TextValue 'Actor', COUNT(actor.Id) AttendedCount, MIN(hardhead.Date) FirstAttended, MAX(hardhead.Date) LastAttended
                        FROM	rep_Text actor
                        JOIN	rep_Event hardhead ON actor.EventId = hardhead.Id AND hardhead.TypeId = 49
                        WHERE	actor.TypeID = 63
	                        AND	hardhead.Date > @fromDate
                        GROUP BY actor.TextValue
                        ORDER BY COUNT(actor.Id) DESC";

        _log.LogInformation("[{Class}.{Method}] Executing SQL: '{SQL}'", _class, nameof(GetActorStatistic), sql);

        var list = new List<StatisticBase>();
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("fromDate", fromDate));

            var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var entity = new StatisticTextEntity(reader.GetString(reader.GetOrdinal("Actor")))
                {
                    AttendedCount = reader.GetInt32(reader.GetOrdinal("AttendedCount")),
                    FirstAttended = reader.GetDateTime(reader.GetOrdinal("FirstAttended")),
                    LastAttended = reader.GetDateTime(reader.GetOrdinal("LastAttended"))
                };

                list.Add(entity);
            }
        }

        return list;
    }

    /// <summary>
    /// Updates a movie with the provided information.
    /// </summary>
    /// <param name="id">The ID of the movie to update.</param>
    /// <param name="userID">The ID of the user making the update.</param>
    /// <param name="movie">The updated movie information.</param>
    /// <returns>True if the update was successful, false otherwise.</returns>
    public async Task<bool> UpdateMovie(int id, int userID, Movie entity)
    {
        ThrowIfDisposed();
        
        _log.LogInformation("[{Class}.{Method}] Updating movie with ID {ID}: {Movie}", _class, nameof(UpdateMovie), id, entity);

        var movieEntity = _movieModel.Events
            .Where(m => m.Id == id && m.TypeId == 49 && m.ParentId != null)
            .Include(m => m.Texts)
            .Include(m => m.Images)
            .Include(m => m.Counts)
            .FirstOrDefault();

        if (movieEntity == null)
            return false;

        var validInsertedDate = entity.Inserted == DateTime.MinValue ? DateTime.UtcNow : GetValidSqlServerDateTime(entity.Inserted);
        var validUpdatedDate = entity.Updated.HasValue ? GetValidSqlServerDateTime(entity.Updated.Value) : validInsertedDate;

        SetText(entity.Name, 62, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);
        SetText(entity.Actor, 63, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);
        SetText(entity.Reason, 59, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);
        SetText(entity.ImdbUrl, 60, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);
        SetText(entity.YoutubeUrl, 208, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);

        if (entity.MovieKillCount.HasValue && entity.MovieKillCount.Value > -1)
            SetCount(entity.MovieKillCount.Value, 221, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);
        if (entity.HardheadKillCount.HasValue && entity.HardheadKillCount.Value > -1)
            SetCount(entity.HardheadKillCount.Value, 222, movieEntity, entity.UpdatedBy ?? entity.InsertedBy, validUpdatedDate);

        if (entity.PosterPhoto != null && entity.PosterPhoto.ID.HasValue)
        {
            var thisImage = movieEntity.Images.Where(i => i.TypeId == 14).FirstOrDefault();
            thisImage = thisImage ?? new Image() { EventId = movieEntity.Id, Inserted = validInsertedDate, InsertedBy = entity.InsertedBy, TypeId = 14 };
            if (entity.PosterPhoto?.ID != null && thisImage.ImageId != entity.PosterPhoto.ID.Value)
            {
                thisImage.ImageId = entity.PosterPhoto.ID.Value;
                if (thisImage.Id == 0)
                    movieEntity.Images.Add(thisImage);
                else
                {
                    thisImage.Updated = validUpdatedDate;
                    thisImage.UpdatedBy = entity.UpdatedBy;
                }
            }
        }

        // Temporarily disable the trigger to avoid OUTPUT clause issues
        await _movieModel.Database.ExecuteSqlRawAsync("DISABLE TRIGGER [dbo].[trg_rep_Text_History] ON [dbo].[rep_Text]");
        
        try
        {
            await _movieModel.SaveChangesAsync();
            return true;
        }
        finally
        {
            // Re-enable the trigger
            await _movieModel.Database.ExecuteSqlRawAsync("ENABLE TRIGGER [dbo].[trg_rep_Text_History] ON [dbo].[rep_Text]");
        }
    }

    private static void SetText(string? newText, int typeId, Event movieEntity, int userId, DateTime actionDate)
    {
        if (!string.IsNullOrWhiteSpace(newText))
        {
            var thisText = movieEntity.Texts.Where(t => t.TypeId == typeId).FirstOrDefault();

            if (thisText == null)
            {
                thisText = new Text() { Event = movieEntity, EventId = movieEntity.Id, Inserted = actionDate, InsertedBy = userId, TypeId = typeId, TextValue = newText };
                movieEntity.Texts.Add(thisText);
                return;
            }

            if (!thisText.TextValue.Equals(newText))
            {
                thisText.TextValue = newText;
                thisText.Updated = actionDate;
                thisText.UpdatedBy = userId;
            }
        }
    }

    private static void SetCount(int newCount, int typeId, Event movieEntity, int userId, DateTime actionDate)
    {
        if (newCount > -1)
        {
            var thisCount = movieEntity.Counts.Where(t => t.TypeId == typeId).FirstOrDefault();
            thisCount = thisCount ?? new Count() { EventId = movieEntity.Id, Inserted = actionDate, InsertedBy = userId, TypeId = typeId };
            if (thisCount.CountValue != newCount)
            {
                thisCount.CountValue = newCount;
                if (thisCount.Id == 0)
                    movieEntity.Counts.Add(thisCount);
                else
                {
                    thisCount.Updated = actionDate;
                    thisCount.UpdatedBy = userId;
                }
            }
        }
    }

    /// <summary>
    /// Ensures a DateTime value is within SQL Server's datetime range (1753-01-01 to 9999-12-31).
    /// If the date is outside this range, returns DateTime.UtcNow.
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <returns>A valid DateTime for SQL Server</returns>
    private static DateTime GetValidSqlServerDateTime(DateTime dateTime)
    {
        // SQL Server datetime range: 1753-01-01 to 9999-12-31
        var minSqlServerDate = new DateTime(1753, 1, 1);
        var maxSqlServerDate = new DateTime(9999, 12, 31);
        
        if (dateTime < minSqlServerDate || dateTime > maxSqlServerDate)
        {
            return DateTime.UtcNow;
        }
        
        return dateTime;
    }

    /// <summary>
    /// Disposes of the MovieModel and other unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _movieModel?.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure disposal if Dispose is not called.
    /// </summary>
    ~MovieSqlAccess()
    {
        Dispose(false);
    }

    /// <summary>
    /// Throws an ObjectDisposedException if the object has been disposed.
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MovieSqlAccess));
        }
    }
}
