using Ez.Hress.Albums.Entities;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.DataAccess;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ez.Hress.UnitTest.Albums
{
    public class AlbumInteractorTests
    {
        private readonly Mock<ILogger<ImageInfoSqlDataAccess>> _logImageInfoSql;
        private readonly Mock<ILogger<ImageContentBlobDataAccess>> _logImageContentBlob;
        private readonly Mock<ILogger<ImageInteractor>> _logImageInteractor;
        private readonly Mock<ILogger<AlbumInteractor>> _logAlbumInteractor;
        private readonly Mock<IAlbumDataAccess> _albumDataAccessMock;

        public AlbumInteractorTests()
        {
            _logImageInfoSql = new Mock<ILogger<ImageInfoSqlDataAccess>>();
            _logImageContentBlob = new Mock<ILogger<ImageContentBlobDataAccess>>();
            _logImageInteractor = new Mock<ILogger<ImageInteractor>>();
            _logAlbumInteractor = new Mock<ILogger<AlbumInteractor>>();
            _albumDataAccessMock= new Mock<IAlbumDataAccess>(); 
        }

        [Fact]
        public async void AddImages_Test()
        {
            var dbConnectionInfo = new DbConnectionInfo("");
            var blobConnectionInfo = new BlobConnectionInfo("");

            IImageInfoDataAccess imageInfoDataAccess = new ImageInfoSqlDataAccess(dbConnectionInfo, _logImageInfoSql.Object);
            IImageContentDataAccess imageContentDataAccess = new ImageContentBlobDataAccess(blobConnectionInfo, _logImageContentBlob.Object);
            IList<IImageContentDataAccess> imageContentDataAccesses = new List<IImageContentDataAccess>
            {
                imageContentDataAccess
            };

            var imageInteractor = new ImageInteractor(imageInfoDataAccess, imageContentDataAccesses, _logImageInteractor.Object);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var albumInteractor = new AlbumInteractor(null, imageInteractor, _logAlbumInteractor.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            await albumInteractor.AddImages(46114);
        }

        [Fact]
        public async void GetAlbums_Test()
        {
            // ARRANGE
            IList<Album> albums = new List<Album>() { new Album(1, "name", "description", 10)};
            _albumDataAccessMock.Setup(a => a.GetAlbums()).Returns(Task.FromResult(albums));
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var albumInteractor = new AlbumInteractor(_albumDataAccessMock.Object, null, _logAlbumInteractor.Object);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            // ACT
            var result = await albumInteractor.GetAlbums();

            // ASSERT
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }
    }
}
