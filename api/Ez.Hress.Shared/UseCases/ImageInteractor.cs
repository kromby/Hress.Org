using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public class ImageInteractor
    {
        private readonly IImageInfoDataAccess _imagesDataAccess;
        private readonly IList<IImageContentDataAccess> _contentDataAccessList;
        private readonly ILogger<ImageInteractor> _log;

        public ImageInteractor(IImageInfoDataAccess imageInfoDataAccess, IList<IImageContentDataAccess> imageContentDataAccessList, ILogger<ImageInteractor> log)
        {
            _imagesDataAccess = imageInfoDataAccess;
            _contentDataAccessList = imageContentDataAccessList;
            _log = log;
        }

        public async Task<ImageEntity?> GetImage(int id)
        {
            _log.LogInformation("[{Class}] Getting image: '{ID}'", nameof(ImageInteractor), id);
            return await _imagesDataAccess.GetImage(id);

        }

        public async Task<ImageEntity?> GetContent(int id, bool useThumb)
        {
            _log.LogInformation("[{Class}] Getting content for image: '{ID}', thumb: '{UseThumb}'", nameof(ImageInteractor), id, useThumb);
            if (id < 0)
            {
                _log.LogInformation("[{Class}] Invalid id: '{ID}'", nameof(ImageInteractor), id);
                throw new ArgumentNullException(nameof(id));
            }

            var image = await GetImage(id);
            if (image == null)
            {
                _log.LogInformation("[{Class}] Image not found: '{ID}'", nameof(ImageInteractor), id);
                return image;
            }

            string path = useThumb && !string.IsNullOrWhiteSpace(image.PhotoThumbUrl) ? image.PhotoThumbUrl : image.PhotoUrl;
            IImageContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => path.ToLowerInvariant().StartsWith(c.Prefix));

            if (contentDataAccess == null || string.IsNullOrWhiteSpace(contentDataAccess.Prefix))
            {
                _log.LogCritical("[{Class}] Invalid PhotoUrl: '{Path}'", nameof(ImageInteractor), path);
                throw new SystemException("Invalid photo URL");
            }

            image.Content = await contentDataAccess.GetContent(path);


            return image;
        }
    }
}
