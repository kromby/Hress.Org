using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Ez.Hress.Shared.UseCases;

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

    public async Task<ImageEntity?> GetImageAsync(int id)
    {
        _log.LogInformation("[{Class}] Getting image: '{ID}'", nameof(ImageInteractor), id);
        return await _imagesDataAccess.GetImage(id);

    }

    public async Task<ImageEntity?> GetContentAsync(int id, int? width, int height)
    {
        int useWidth = width ?? 0;

        _log.LogInformation("[{Class}] Getting content for image: '{ID}', thumb: '{useWidth}'", nameof(ImageInteractor), id, useWidth);
        if (id < 0)
        {
            _log.LogInformation("[{Class}] Invalid id: '{ID}'", nameof(ImageInteractor), id);
            throw new ArgumentNullException(nameof(id));
        }

        var image = await GetImageAsync(id);
        if (image == null)
        {
            _log.LogInformation("[{Class}] Image not found: '{ID}'", nameof(ImageInteractor), id);
            return image;
        }

        //string path = (useWidth < 5) && !string.IsNullOrWhiteSpace(image.PhotoThumbUrl) ? image.PhotoThumbUrl : image.PhotoUrl;
        string path = image.PhotoUrl;
        IImageContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => path.ToLowerInvariant().StartsWith(c.Prefix));

        if (contentDataAccess == null || string.IsNullOrWhiteSpace(contentDataAccess.Prefix))
        {
            _log.LogCritical("[{Class}] Invalid PhotoUrl: '{Path}'", nameof(ImageInteractor), path);
            throw new SystemException("Invalid photo URL");
        }

        var content = await contentDataAccess.GetContent(path);

        try
        {
            if (useWidth > 0 && content != null)
            {
                var imageObject = Image.Load(content);

                useWidth = useWidth < (imageObject.Width * 2) ? useWidth : imageObject.Width * 2;

                imageObject.Mutate(i => i.AutoOrient().Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Crop,
                    Position = AnchorPositionMode.Center,
                    Size = new Size(useWidth, height)
                }));

                using var ms = new MemoryStream();
                imageObject.SaveAsJpeg(ms);
                content = ms.ToArray();
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}], {Message}", nameof(ImageInteractor), ex.Message);
        }

        image.Content = content;


        return image;
    }

    public async Task<ImageHrefEntity> SaveAsync(ImageEntity entity, int userID)
    {
        if (entity == null)
            throw new ArgumentException("Can not be null", nameof(entity));

        entity.Validate();

        _log.LogInformation("[{Class}] ImageEntity: '{entity}'", nameof(ImageInteractor), entity);

        string? containerName = Enum.GetName(typeof(ImageContainer), entity.Container);
        if (containerName == null)
        {
            var ex = new ArgumentException($"Container '{entity.Container}' not found.");
            _log.LogError(ex, "Container not found");
            throw ex;
        }

        int typeID = 0;
        typeID = entity.Container switch
        {
            ImageContainer.Album => 20,
            ImageContainer.ATVR => 25,
            ImageContainer.News => 19,
            ImageContainer.Profile => 24,
            // Hardhead, Other
            _ => 26,
        };
        ;

        if (entity.ID == 0)
        {
            entity.Inserted = DateTime.UtcNow;
            entity.InsertedBy = userID;
            entity.PhotoUrl = $"BLOB:/{containerName.ToLowerInvariant()}/@ID";
        }
        else
        {
            entity.Updated = DateTime.UtcNow;
            entity.UpdatedBy = userID;
        }

        IImageContentDataAccess? contentDataAccess = _contentDataAccessList.FirstOrDefault(c => c.Prefix.Equals("blob:/"));
        if (contentDataAccess == null)
        {
            var ex = new SystemException(@"Data access with prefix 'blob:/' not found.");
            _log.LogError(ex, "Could not find correct content data access.");
            throw ex;
        }

        int id = 0;
        try
        {
            //var imageInfo = Image.Identify(entity.Content);
            var imageInfo = Image.Load(entity.Content);
            imageInfo.Mutate(i => i.AutoOrient());
            bool dateTakenFound = false;
            // Try to get date taken from EXIF data
            if (imageInfo.Metadata.ExifProfile?.TryGetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal, out var dateTaken) == true)
            {
                if (dateTaken != null && DateTime.TryParseExact(dateTaken.Value, "yyyy:MM:dd HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime photoTaken))
                {
                    entity.Inserted = photoTaken;
                    dateTakenFound = true;
                }
            }

            if (!dateTakenFound)
            {
                entity.Inserted = DateTime.UtcNow;
            }

            id = await _imagesDataAccess.Save(entity, typeID, imageInfo.Height, imageInfo.Width);
            _log.LogInformation("[{Class}] New image saved to metadata database: '{id}'", nameof(ImageInteractor), id);

            if (id == -1)
                throw new SystemException("Saving failed - unknown reason - ID -1");

#pragma warning disable CS8604 // Possible null reference argument. This is checked in entity.Validate().
            await contentDataAccess.Save(containerName.ToLowerInvariant(), entity.Content, id);
            _log.LogInformation("[{Class}] New image saved to blob store: '{id}'", nameof(ImageInteractor), id);

            return new ImageHrefEntity(id, entity.Name);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        catch (InvalidImageContentException iicex)
        {
            _log.LogError(iicex, "Invalid image content");
            throw;
        }
    }
}
