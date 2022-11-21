using Ez.Hress.Albums.Entities;
using Ez.Hress.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Albums.UseCases
{
    public interface IAlbumDataAccess
    {
        public Task<IList<Album>> GetAlbums();

        public Task<Album?> GetAlbum(int id);

        public Task<IList<ImageEntity>> GetImages(int albumID);
    }
}
