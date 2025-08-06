using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Shared.DataAccess;

public class MediaContentBlobDataAccess : IImageContentDataAccess, IVideoContentDataAccess
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<MediaContentBlobDataAccess> _log;

    public MediaContentBlobDataAccess(BlobConnectionInfo blobConnectionInfo, ILogger<MediaContentBlobDataAccess> log)
    {
        _log = log;

        try
        {
            // Create a BlobServiceClient object which will be used to create a container client
            _blobServiceClient = new BlobServiceClient(blobConnectionInfo.ConnectionString);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}] Exception '{Exception}'", this.GetType().Name, ex.Message);
            throw;
        }
    }

    public string Prefix { get => "blob:/"; }

    public async Task<byte[]?> GetContent(string path)
    {
        try
        {
            var stringSplit = path.Replace(Prefix.ToUpper(), "").Split('/');
            var container = stringSplit[0];
            var id = path.Replace(Prefix.ToUpper(), "").Replace($"{container}/", "");
            //stringSplit[1];

            _log.LogInformation("[{Class}] Get blob {id} from container {container}", this.GetType().Name, id, container);

            // Get the container client object
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container);


            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(id);
            BlobDownloadInfo response = await blobClient.DownloadAsync();

            MemoryStream ms = new();
            response.Content.CopyTo(ms);
            return ms.ToArray();

        }
        catch (RequestFailedException rfex)
        {
            _log.LogError(rfex, "[{Class}] Media not found '{Path}'", this.GetType().Name, path);
            return null;
        }
    }

    public async Task Save(string container, byte[] content, int id)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(container);
        await containerClient.UploadBlobAsync(id.ToString(), new BinaryData(content));
    }
}
