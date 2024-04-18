using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Ez.Hress.MajorEvents.Entities;
using Ez.Hress.Shared.Entities;

namespace Ez.Hress.FunctionsApi.Images
{
    public class ImagesContentFunction
    {
        private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly ImageInteractor _imageInteractor;
        public ImagesContentFunction(AuthenticationInteractor authenticationInteractor, ImageInteractor imageInteractor)
        {
            _authenticationInteractor = authenticationInteractor;
            _imageInteractor = imageInteractor;
        }

        [FunctionName("images")]
        public async Task<IActionResult> RunImages(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "images")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("[{Class}] C# HTTP trigger function processed a request.", nameof(RunImages));

            var isJWTValid = AuthenticationUtil.GetAuthenticatedUserID(_authenticationInteractor, req.Headers, log, out int userID);
            if (!isJWTValid)
            {
                log.LogInformation("[{Function}]  JWT is not valid!", nameof(RunImages));
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var image = JsonConvert.DeserializeObject<ImageEntity>(requestBody);

            try
            {
                var entity = await _imageInteractor.SaveAsync(image, userID);
                return new OkObjectResult(entity);
            }
            catch(ArgumentException aex)
            {
                log.LogError(aex, requestBody);
                return new BadRequestResult();
            }
        }

            [FunctionName("imagesContent")]
        public async Task<IActionResult> RunImagesContent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "images/{id:int}/content")] HttpRequest req,
            int id, ILogger log)
        {
            log.LogInformation("[{Class}] C# HTTP trigger function processed a request.", nameof(RunImagesContent));

            int width = 0;
            if (!string.IsNullOrWhiteSpace(req.Query["thumb"]))
            {
                _ = bool.TryParse(req.Query["thumb"], out bool isThumb);
                width = isThumb ? 250 : 0;
            }

            if (!string.IsNullOrWhiteSpace(req.Query["width"]))
            {
                _ = int.TryParse(req.Query["width"], out width);
            }

            int height = 0;
            if (!string.IsNullOrWhiteSpace(req.Query["height"]))
            {
                _ = int.TryParse(req.Query["height"], out height);
            }

            var entity = await _imageInteractor.GetContentAsync(id, width, height);

            if (entity?.Content == null)
            {
                return new NotFoundResult();
            }

            req.HttpContext.Response.Headers.ContentDisposition  = $"inline; filename={entity.ID}.jpg";
            return new FileContentResult(entity.Content, new MediaTypeHeaderValue("image/jpeg"));
        }
    }
}
