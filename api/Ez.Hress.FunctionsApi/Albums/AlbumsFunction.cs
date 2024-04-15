using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Albums.UseCases;
using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.FunctionsApi.Albums
{
    public class AlbumsFunction
    {
        private readonly AlbumInteractor _albumInteractor;
        private readonly AuthenticationInteractor _authenticationInteractor;
        public AlbumsFunction(AuthenticationInteractor authenticationInteractor, AlbumInteractor albumInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _albumInteractor = albumInteractor;
        }

        [FunctionName("albums")]
        public async Task<IActionResult> RunAlbums(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{id:int?}")] HttpRequest req,
            int? id, ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbums));

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
            if (!isJWTValid)
            {
                log.LogInformation("[RunMagic] JWT is not valid!");
                return new UnauthorizedResult();
            }

            if (id.HasValue)
            {
                var entity = await _albumInteractor.GetAlbum(id.Value);
                if (entity == null)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(entity);
            }
            else
            {
                var list = await _albumInteractor.GetAlbums();
                return new OkObjectResult(list);
            }
        }

        [FunctionName("albumImages")]
        public async Task<IActionResult> RunAlbumImages(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "albums/{albumID:int}/images")] HttpRequest req,
            int albumID, ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(RunAlbumImages));

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
            if (!isJWTValid)
            {
                log.LogInformation("[RunMagic] JWT is not valid!");
                return new UnauthorizedResult();
            }

            var list = await _albumInteractor.GetImages(albumID);
            return new OkObjectResult(list);
        }
    }
}
