using Ez.Hress.Albums.Entities;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Ez.Hress.Albums.UseCases;

public class AlbumInteractor
{
    private readonly IAlbumDataAccess _albumDataAccess;
    private readonly ImageInteractor _imageInteractor;
    private readonly ILogger<AlbumInteractor> _log;

    public AlbumInteractor(IAlbumDataAccess albumDataAccess, ImageInteractor imageInteractor, ILogger<AlbumInteractor> log)
    {
        _albumDataAccess = albumDataAccess;
        _imageInteractor = imageInteractor;
        _log = log;
    }

    public async Task AddImagesAsync(int albumID)
    {            
        var list = Directory.GetFiles(@"C:\Temp\hress photos\MogR2023");

        var name = "Matar- og Rauðvínskvöld 2023";

        StringBuilder sql = new();

        foreach (var imagePath in list)
        {
            if(!imagePath.EndsWith(".jpg"))
                continue;

            var imageBytes = File.ReadAllBytes(imagePath);

            var imageEntity = new ImageEntity(0, name, "") { Container = ImageContainer.Album, Content = imageBytes };

            var imageHrefEntity = await _imageInteractor.SaveAsync(imageEntity, 2630);

            sql.AppendLine($"INSERT INTO scr_Image(ComponentId, TypeId, ImageId, Align, InsertedBy) VALUES({albumID}, 46, {imageHrefEntity.ID}, 1, 2630)");
        }

        var smu = sql.ToString();
        _log.LogInformation("SQL: {SQL}", smu);
    }

    public async Task<IList<Album>> GetAlbumsAsync()
    {
        _log.LogInformation("GetAlbums");
        return await _albumDataAccess.GetAlbums();
    }

    public async Task<Album?> GetAlbumAsync(int id)
    {
        return await _albumDataAccess.GetAlbum(id);
    }

    public async Task<IList<ImageEntity>> GetImagesAsync(int albumID)
    {
        return await _albumDataAccess.GetImages(albumID);
    }

    public async Task<Album> CreateAlbumAsync(Album album, int userId)
    {
        _log.LogInformation("[{Method}] Creating album: {AlbumName}", nameof(CreateAlbumAsync), album.Name);
        
        // Validate the album before proceeding
        album.Validate();
        
        // Set the InsertedBy property before passing to data access
        album.InsertedBy = userId;
        
        return await _albumDataAccess.CreateAlbum(album);
    }
}
