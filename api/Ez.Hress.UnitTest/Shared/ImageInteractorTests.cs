﻿using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Shared;

public class ImageInteractorTests
{
    private readonly Mock<IImageInfoDataAccess> _infoMock;
    private readonly Mock<IImageContentDataAccess> _contentHttpMock;
    private readonly Mock<IImageContentDataAccess> _contentBlobMock;
    private readonly Mock<IImageContentDataAccess> _contentRelativeMock;        
    private readonly Mock<ILogger<ImageInteractor>> _log;
    
    public ImageInteractorTests()
    {
        _infoMock = new();
        _contentHttpMock = new(MockBehavior.Strict);
        _contentHttpMock.Setup(c => c.Prefix).Returns("http");
        _contentBlobMock = new(MockBehavior.Strict);
        _contentBlobMock.Setup(c => c.Prefix).Returns("blob");
        _contentRelativeMock = new(MockBehavior.Strict);
        _contentRelativeMock.Setup(c => c.Prefix).Returns("~/");
        _log = new();
    }

    [Fact]
    public async Task GetContentHttpOK_TestAsync()
    {
        // ARRANGE
        ImageEntity? entity = new(1979, "Test", "https://asdfkjasf.com") { };
        _infoMock.Setup(i => i.GetImage(It.IsAny<int>())).Returns(Task.FromResult<ImageEntity?>(entity));
        _contentHttpMock.Setup(c => c.GetContent(It.Is<string>(entity.PhotoUrl, StringComparer.Ordinal))).Returns(Task.FromResult<byte[]?>(new [] { new byte() }));
        var contentList = new List<IImageContentDataAccess>() { _contentHttpMock.Object, _contentBlobMock.Object, _contentRelativeMock.Object };
        var interactor = new ImageInteractor(_infoMock.Object, contentList, _log.Object);

        // ACT
        var result = await interactor.GetContentAsync(1979, 250,250);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        _contentHttpMock.Verify();
    }

    [Fact]
    public async Task GetContentBlobOK_TestAsync()
    {
        // ARRANGE
        ImageEntity? entity = new(1979, "Test", "BLOB://asdfkjasf.com") { };
        _infoMock.Setup(i => i.GetImage(It.IsAny<int>())).Returns(Task.FromResult<ImageEntity?>(entity));
        _contentBlobMock.Setup(c => c.GetContent(It.Is<string>(entity.PhotoUrl, StringComparer.Ordinal))).Returns(Task.FromResult<byte[]?>(new [] { new byte() }));
        var contentList = new List<IImageContentDataAccess>() { _contentHttpMock.Object, _contentBlobMock.Object, _contentRelativeMock.Object };
        var interactor = new ImageInteractor(_infoMock.Object, contentList, _log.Object);

        // ACT
        var result = await interactor.GetContentAsync(1979, 350,350);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        _contentBlobMock.Verify();
    }

    [Fact]
    public async Task GetContentRelativeOK_TestAsync()
    {
        // ARRANGE
        ImageEntity? entity = new(1979, "Test", "~/images/external/prump.png") { };
        _infoMock.Setup(i => i.GetImage(It.IsAny<int>())).Returns(Task.FromResult<ImageEntity?>(entity));
        _contentRelativeMock.Setup(c => c.GetContent(It.Is<string>(entity.PhotoUrl, StringComparer.Ordinal))).Returns(Task.FromResult<byte[]?>(new [] { new byte() }));
        var contentList = new List<IImageContentDataAccess>() { _contentHttpMock.Object, _contentBlobMock.Object, _contentRelativeMock.Object };
        var interactor = new ImageInteractor(_infoMock.Object, contentList, _log.Object);

        // ACT
        var result = await interactor.GetContentAsync(1979, 500,500);

        // ASSERT
        Assert.NotNull(result);
        Assert.NotNull(result.Content);
        _contentBlobMock.Verify();
    }
}
