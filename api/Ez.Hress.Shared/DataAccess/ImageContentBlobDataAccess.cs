using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ez.Hress.Shared.Entities;
using Ez.Hress.Shared.UseCases;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public class ImageContentBlobDataAccess : IImageContentDataAccess
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<ImageContentBlobDataAccess> _log;

        public ImageContentBlobDataAccess(BlobConnectionInfo blobConnectionInfo, ILogger<ImageContentBlobDataAccess> log)
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

            _log.LogDebug("[{Class}] constructor executed", this.GetType().Name);

        }

        public string Prefix { get => "blob:/"; }

        public async Task<byte[]> GetContent(string path)
        {
            try
            {
                var stringSplit = path.Replace(Prefix.ToUpper(), "").Split('/');
                var container = stringSplit[0];
                var id = stringSplit[1];

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
                _log.LogError(rfex, "[{Class}] Image not found '{Path}'", this.GetType().Name, path);
                return null;
            }
        }
    }
}
