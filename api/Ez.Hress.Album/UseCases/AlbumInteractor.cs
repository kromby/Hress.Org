using Ez.Hress.Albums.Entities;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Albums.UseCases
{
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

        public async Task AddImages(int albumID)
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

                var imageHrefEntity = await _imageInteractor.Save(imageEntity, 2630);

                sql.AppendLine($"INSERT INTO scr_Image(ComponentId, TypeId, ImageId, Align, InsertedBy) VALUES({albumID}, 46, {imageHrefEntity.ID}, 1, 2630)");
            }

            var smu = sql.ToString();
        }

        public async Task<IList<Album>> GetAlbums()
        {
            _log.LogInformation("GetAlbums");
            return await _albumDataAccess.GetAlbums();
        }

        public async Task<Album?> GetAlbum(int id)
        {
            return await _albumDataAccess.GetAlbum(id);
        }

        public async Task<IList<ImageEntity>> GetImages(int albumID)
        {
            return await _albumDataAccess.GetImages(albumID);
        }
    }
}
