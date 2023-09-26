using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Hardhead.UseCases;

namespace Ez.Hress.FunctionsApi.Hardhead
{
    public class MovieFunctions
    {
        private readonly string _class = nameof(HardheadFunctions);
        //private readonly AuthenticationInteractor _authenticationInteractor;
        private readonly MovieInteractor _movieInteractor;

        public MovieFunctions(MovieInteractor movieInteractor)
        {
            _movieInteractor = movieInteractor;
        }

        [FunctionName("MovieFunctions")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/{id:int?}")] HttpRequest req, int? id,
            ILogger log)
        {
            log.LogInformation("[{Class}]C# HTTP trigger function processed a request.", _class);

            if (HttpMethods.IsGet(req.Method))
            {
                if (id.HasValue)
                {

                }
                else
                {
                    if (req.Query.ContainsKey("filter"))
                    {
                        var list = await _movieInteractor.GetMovies(req.Query["filter"]);
                        return new OkObjectResult(list);
                    }
                }
            }

            return new NotFoundResult();
        }
    }
}
