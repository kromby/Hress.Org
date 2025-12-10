using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Ez.Hress.UnitTest.Hardhead;

public class MovieInteractorTests
{
    private readonly Mock<IMovieDataAccess> _movieDataAccess;
    private readonly Mock<IMovieInformationDataAccess> _movieInfoDataAccess;
    private readonly Mock<TranslationService> _translationService;
    private readonly Mock<ILogger<MovieInteractor>> _log;

    public MovieInteractorTests()
    {
        _movieDataAccess = new Mock<IMovieDataAccess>();
        _movieInfoDataAccess = new Mock<IMovieInformationDataAccess>();
        _translationService = new Mock<TranslationService>();
        _log = new Mock<ILogger<MovieInteractor>>();
    }


    [Fact]
    public async Task SaveMovieInformation_ShouldSaveMovieInformation_Async()
    {
        // ARRANGE
        int hardheadID = 1;
        int userID = 2;
        DateTime hardheadDate = DateTime.UtcNow;
        MovieInfo movieInfo = new("Test Movie", "Test Plot", "Rating", "Country")
        {
            Year = 1988,
            Released = new DateTime(1988, 7, 20),
            Runtime = 120,
            Genre = new List<string> { "Action", "Adventure" },
            Crew = new List<CrewMember> { new() { Name = "John Doe", Role = Role.Director }, new() { Name = "Jane Doe", Role = Role.Writer }, new() { Name = "John Doe", Role = Role.Actor } },
            Language = new List<string> { "English" },
            Awards = "Best Movie",
            Ratings = new Dictionary<string, string> { { "IMDB", "9.9" } },
            Metascore = "99",
            ImdbRating = 9.9m,
            ImdbVotes = 1000,
            ImdbID = "tt123456"
        };

        var movieInteractor = new MovieInteractor(_movieDataAccess.Object, _movieInfoDataAccess.Object, _translationService.Object, _log.Object);

        // ACT
        await movieInteractor.SaveMovieInformationAsync(hardheadID, userID, hardheadDate, movieInfo);

        // ASSERT
        _movieInfoDataAccess.Verify(m => m.SaveMovieInformationAsync(movieInfo), Times.Once);
    }
}

