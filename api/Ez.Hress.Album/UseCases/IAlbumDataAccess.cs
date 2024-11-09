using Ez.Hress.Albums.Entities;
using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Albums.UseCases;

public interface IAlbumDataAccess
{
    public Task<IList<Album>> GetAlbums();

    public Task<Album?> GetAlbum(int id);

    public Task<IList<ImageEntity>> GetImages(int albumID);

    public Task<Album> CreateAlbum(Album album);
}
