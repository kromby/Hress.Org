using Ez.Hress.Scripts.Entities;
using Ez.Hress.Scripts.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Scripts;

public class NewsInteractorTests
{
    private readonly Mock<INewsDataAccess> _newsDataAccess;
    private readonly Mock<ILogger<NewsInteractor>> _log;

    public NewsInteractorTests()
    {
        _newsDataAccess = new Mock<INewsDataAccess>();
        _log = new Mock<ILogger<NewsInteractor>>();
    }

    [Fact]
    public async Task GetLatestNewsOK_TestAsync()
    {
        // ARRANGE
        IList<News> list = new List<News>();
        _newsDataAccess.Setup(x => x.GetLatestNews(It.IsAny<int>())).Returns(Task.FromResult(list));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT
        var result = await interactor.GetLatestNewsAsync();

        // ASSERT
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetNewsByAuthorOK_TestAsync()
    {
        // ARRANGE
        const int userId = 2630;
        IList<News> list = new List<News>
        {
            new() { ID = 1, Name = "Frétt 1", InsertedBy = userId, Hits = 5, Public = true },
            new() { ID = 2, Name = "Frétt 2", InsertedBy = userId, Hits = 0, Public = false },
        };
        _newsDataAccess.Setup(x => x.GetNewsByAuthor(userId)).Returns(Task.FromResult(list));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT
        var result = await interactor.GetNewsByAuthorAsync(userId);

        // ASSERT
        Assert.Same(list, result);
        Assert.Equal(2, result.Count);
        _newsDataAccess.Verify(x => x.GetNewsByAuthor(userId), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetNewsByAuthorInvalidUser_TestAsync(int userId)
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.GetNewsByAuthorAsync(userId));
        _newsDataAccess.Verify(x => x.GetNewsByAuthor(It.IsAny<int>()), Times.Never);
    }
}
