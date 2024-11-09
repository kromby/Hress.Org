using Ez.Hress.Albums.Entities;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Albums;

public class AlbumInteractorTests
{
    private readonly Mock<ILogger<AlbumInteractor>> _logAlbumInteractor;
    private readonly Mock<IAlbumDataAccess> _albumDataAccessMock;

    private readonly Mock<IImageInfoDataAccess> _imagesDataAccessMock;
    private readonly Mock<IList<IImageContentDataAccess>> _contentDataAccessListMock;
    private readonly Mock<ILogger<ImageInteractor>> _logImageInteractorMock;

    private readonly ImageInteractor _imageInteractor;

    private readonly AlbumInteractor _albumInteractor;

    public AlbumInteractorTests()
    {
        _logAlbumInteractor = new Mock<ILogger<AlbumInteractor>>();
        _albumDataAccessMock = new Mock<IAlbumDataAccess>();

        _imagesDataAccessMock = new Mock<IImageInfoDataAccess>();
        _contentDataAccessListMock = new Mock<IList<IImageContentDataAccess>>();
        _logImageInteractorMock = new Mock<ILogger<ImageInteractor>>();

        _imageInteractor = new ImageInteractor(_imagesDataAccessMock.Object, _contentDataAccessListMock.Object, _logImageInteractorMock.Object);

        _albumInteractor = new AlbumInteractor(
            _albumDataAccessMock.Object,
            _imageInteractor,
            _logAlbumInteractor.Object
        );
    }

    [Fact]
    public async Task CreateAlbumAsync_ValidAlbum_ReturnsCreatedAlbum()
    {
        // Arrange
        var album = new Album(0, "Test Album", "Test Description", 0);
        var userId = 123;

        _albumDataAccessMock
            .Setup(x => x.CreateAlbum(It.IsAny<Album>()))
            .ReturnsAsync((Album a) => { a.ID = 1; return a; });

        // Act
        var result = await _albumInteractor.CreateAlbumAsync(album, userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ID);
        Assert.Equal(userId, result.InsertedBy);
        Assert.Equal("Test Album", result.Name);
        Assert.Equal("Test Description", result.Description);
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("A", "Description")]
    [InlineData("Name", "")]
    public async Task CreateAlbumAsync_InvalidData_ThrowsArgumentException(string name, string description)
    {
        // Arrange
        var album = new Album(0, name, description, 0);
        var userId = 123;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _albumInteractor.CreateAlbumAsync(album, userId));
    }

    [Fact]
    public async Task CreateAlbumAsync_FutureDate_ThrowsArgumentException()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var album = new Album(0, "Test Album", "Test Description", 0, futureDate);
        var userId = 123;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _albumInteractor.CreateAlbumAsync(album, userId));
        Assert.Contains("future", exception.Message);
    }

    [Fact]
    public async Task GetAlbumsAsync_ReturnsListOfAlbums()
    {
        // Arrange
        var albums = new List<Album> 
        { 
            new Album(1, "Album 1", "Description 1", 10),
            new Album(2, "Album 2", "Description 2", 5)
        };

        _albumDataAccessMock
            .Setup(x => x.GetAlbums())
            .ReturnsAsync(albums);

        // Act
        var result = await _albumInteractor.GetAlbumsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            album => Assert.Equal("Album 1", album.Name),
            album => Assert.Equal("Album 2", album.Name)
        );
    }

    [Fact]
    public async Task GetAlbumAsync_ExistingId_ReturnsAlbum()
    {
        // Arrange
        var album = new Album(1, "Test Album", "Test Description", 10);
        _albumDataAccessMock
            .Setup(x => x.GetAlbum(1))
            .ReturnsAsync(album);

        // Act
        var result = await _albumInteractor.GetAlbumAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ID);
        Assert.Equal("Test Album", result.Name);
    }

    [Fact]
    public async Task GetAlbumAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        _albumDataAccessMock
            .Setup(x => x.GetAlbum(999))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _albumInteractor.GetAlbumAsync(999);

        // Assert
        Assert.Null(result);
    }
}
