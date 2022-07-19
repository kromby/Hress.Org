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

namespace Ez.Hress.FunctionsApi.Images
{
    public class ImagesContentFunction
    {
        private readonly ImageInteractor _imageInteractor;
        public ImagesContentFunction(ImageInteractor imageInteractor)
        {
            _imageInteractor = imageInteractor;
        }

        [FunctionName("imagesContent")]
        public async Task<IActionResult> RunImagesContent(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "images/{id:int}/content")] HttpRequest req,
            int id, ILogger log)
        {
            log.LogInformation("[{Class}] C# HTTP trigger function processed a request.", nameof(RunImagesContent));

            bool isThumb = false;
            if (!string.IsNullOrWhiteSpace(req.Query["thumb"]))
            {
                _ = bool.TryParse(req.Query["thumb"], out isThumb);
            }

            var entity = await _imageInteractor.GetContent(id, isThumb);

            if (entity == null || entity.Content == null)
            {
                return new NotFoundResult();
            }

            req.HttpContext.Response.Headers.ContentDisposition  = $"inline; filename={entity.ID}.jpg";
            return new FileContentResult(entity.Content, new MediaTypeHeaderValue("image/jpeg"));
        }
    }
}
