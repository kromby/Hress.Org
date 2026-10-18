using Ez.Hress.Scripts.Entities;
using Ez.Hress.Scripts.UseCases;
using Ez.Hress.Shared.Entities;
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

    [Fact]
    public async Task CreateNewsOK_TestAsync()
    {
        // ARRANGE
        const int userId = 2630;
        News? saved = null;
        _newsDataAccess.Setup(x => x.CreateNews(It.IsAny<News>()))
            .Callback<News>(n => saved = n)
            .Returns(Task.FromResult(1234));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = "  Ný frétt  ", Content = "<p>Halló</p>", Public = true };

        // ACT
        var result = await interactor.CreateNewsAsync(news, userId);

        // ASSERT
        Assert.Equal(1234, result.ID);
        Assert.NotNull(saved);
        Assert.Equal("Ný frétt", saved!.Name);
        Assert.Equal("<p>Halló</p>", saved.Content);
        Assert.Equal(userId, saved.InsertedBy);
        Assert.True(saved.Public);
        Assert.NotEqual(default, saved.Inserted);
        Assert.Null(saved.Image);
    }

    [Theory]
    [InlineData(null, "<p>Body</p>")]
    [InlineData("   ", "<p>Body</p>")]
    [InlineData("Headline", null)]
    [InlineData("Headline", "  ")]
    public async Task CreateNewsMissingFields_TestAsync(string? name, string? content)
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = name, Content = content };

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateNewsAsync(news, 2630));
        _newsDataAccess.Verify(x => x.CreateNews(It.IsAny<News>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateNewsInvalidUser_TestAsync(int userId)
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = "Headline", Content = "<p>Body</p>" };

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateNewsAsync(news, userId));
        _newsDataAccess.Verify(x => x.CreateNews(It.IsAny<News>()), Times.Never);
    }

    [Theory]
    [InlineData("<a href=\"http://hress.org\">x</a>", "<a target=\"_blank\" href=\"http://hress.org\">x</a>")]
    [InlineData("<A href=\"https://hress.org\">x</A>", "<a target=\"_blank\" href=\"https://hress.org\">x</a>")]
    [InlineData("<a target=\"_self\" href=\"http://hress.org\">x</a>", "<a target=\"_self\" href=\"http://hress.org\">x</a>")]
    [InlineData("<a href=\"/news/1\">x</a>", "<a href=\"/news/1\">x</a>")]
    [InlineData("<a title=\"story\" href=\"http://hress.org\">x</a>", "<a target=\"_blank\" title=\"story\" href=\"http://hress.org\">x</a>")]
    [InlineData("<a href=\"http://hress.org\" data-target=\"x\">x</a>", "<a target=\"_blank\" href=\"http://hress.org\">x</a>")]
    [InlineData("<a href=\"http://hress.org\" TARGET='_top'>x</a>", "<a href=\"http://hress.org\" target=\"_top\">x</a>")]
    [InlineData("<p title=\"http://x\">x</p>", "<p title=\"http://x\">x</p>")]
    public async Task CreateNewsOpensExternalLinksInNewTab_TestAsync(string content, string expected)
    {
        // ARRANGE
        News? saved = null;
        _newsDataAccess.Setup(x => x.CreateNews(It.IsAny<News>()))
            .Callback<News>(n => saved = n)
            .Returns(Task.FromResult(1));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT
        await interactor.CreateNewsAsync(new News { Name = "Headline", Content = content }, 2630);

        // ASSERT
        Assert.Equal(expected, saved!.Content);
    }

    [Fact]
    public async Task CreateNewsImageDefaultsToLeftAlign_TestAsync()
    {
        // ARRANGE
        News? saved = null;
        _newsDataAccess.Setup(x => x.CreateNews(It.IsAny<News>()))
            .Callback<News>(n => saved = n)
            .Returns(Task.FromResult(1));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = "Headline", Content = "<p>Body</p>", Image = new ImageHrefEntity(55, string.Empty) };

        // ACT
        await interactor.CreateNewsAsync(news, 2630);

        // ASSERT
        Assert.Equal(55, saved!.Image!.ID);
        Assert.Equal(Align.Left, saved.ImageAlign);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(8)]
    public async Task CreateNewsInvalidImageAlign_TestAsync(int align)
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News
        {
            Name = "Headline",
            Content = "<p>Body</p>",
            Image = new ImageHrefEntity(55, string.Empty),
            ImageAlign = (Align)align,
        };

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateNewsAsync(news, 2630));
        _newsDataAccess.Verify(x => x.CreateNews(It.IsAny<News>()), Times.Never);
    }

    [Fact]
    public async Task CreateNewsInvalidImageId_TestAsync()
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = "Headline", Content = "<p>Body</p>", Image = new ImageHrefEntity(0, string.Empty) };

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateNewsAsync(news, 2630));
    }

    [Theory]
    [InlineData("<p>Texti</p><script>alert(1)</script>", "<script")]
    [InlineData("<p>Texti</p><img src=\"x.png\" onerror=\"alert(1)\">", "onerror")]
    [InlineData("<p><a href=\"javascript:alert(1)\">Texti</a></p>", "javascript:")]
    [InlineData("<p style=\"background:url(x)\">Texti</p>", "style")]
    [InlineData("<p>Texti</p><iframe src=\"https://evil.example\"></iframe>", "<iframe")]
    public async Task CreateNewsSanitizesBody_TestAsync(string content, string forbidden)
    {
        // ARRANGE
        News? saved = null;
        _newsDataAccess.Setup(x => x.CreateNews(It.IsAny<News>()))
            .Callback<News>(n => saved = n)
            .Returns(Task.FromResult(1));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT
        await interactor.CreateNewsAsync(new News { Name = "Headline", Content = content }, 2630);

        // ASSERT
        Assert.DoesNotContain(forbidden, saved!.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Texti", saved.Content);
    }

    [Fact]
    public async Task CreateNewsKeepsEditorFormatting_TestAsync()
    {
        // ARRANGE
        const string content = "<h2>Fyrirsögn</h2><p><strong>Feitt</strong> <em>skáletrað</em> <s>yfirstrikað</s></p>"
            + "<ul><li><p>Liður</p></li></ul><ol><li><p>Númer</p></li></ol>"
            + "<blockquote><p>Tilvitnun</p></blockquote><p><a href=\"/news/1\">innri</a></p>";
        News? saved = null;
        _newsDataAccess.Setup(x => x.CreateNews(It.IsAny<News>()))
            .Callback<News>(n => saved = n)
            .Returns(Task.FromResult(1));
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);

        // ACT
        await interactor.CreateNewsAsync(new News { Name = "Headline", Content = content }, 2630);

        // ASSERT
        Assert.Equal(content, saved!.Content);
    }

    [Fact]
    public async Task CreateNewsBodyEmptyAfterSanitizing_TestAsync()
    {
        // ARRANGE
        var interactor = new NewsInteractor(_newsDataAccess.Object, _log.Object);
        var news = new News { Name = "Headline", Content = "<script>alert(1)</script>" };

        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentException>(() => interactor.CreateNewsAsync(news, 2630));
        _newsDataAccess.Verify(x => x.CreateNews(It.IsAny<News>()), Times.Never);
    }
}
