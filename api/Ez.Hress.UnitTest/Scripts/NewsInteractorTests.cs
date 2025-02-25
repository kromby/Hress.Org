﻿using Ez.Hress.Scripts.Entities;
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
}
