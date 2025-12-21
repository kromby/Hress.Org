using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Types;

public class TypesFunction
{
    private readonly string _function = nameof(TypesFunction);
    private readonly ITypeInteractor _typeInteractor;
    private readonly ILogger<TypesFunction> _log;

    public TypesFunction(ITypeInteractor typeInteractor, ILogger<TypesFunction> log)
    {
        _typeInteractor = typeInteractor;
        _log = log;
    }

    [Function("types")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "types/{id:int?}")] HttpRequest req,
        int? id)
    {
        var methodName = nameof(Run);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        try
        {
            if (id.HasValue)
            {
                // Get single type by ID
                var type = await _typeInteractor.GetEzType(id.Value);
                return new OkObjectResult(type);
            }
            
            if (req.Query.ContainsKey("parentId") && int.TryParse(req.Query["parentId"], out int parentId))
            {
                // Get types by parent ID
                var types = await _typeInteractor.GetEzTypesByParentId(parentId);
                return new OkObjectResult(types);
            }
        
            // Get all types
            var types = await _typeInteractor.GetEzTypes();
            return new OkObjectResult(types);
        
        }
        catch (ArgumentException aex)
        {
            _log.LogError(aex, "[{Class}.{Method}] Invalid input", _function, methodName);
            return new BadRequestObjectResult(aex.Message);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "[{Class}.{Method}] Unhandled error", _function, methodName);
            throw;
        }
    }
}

