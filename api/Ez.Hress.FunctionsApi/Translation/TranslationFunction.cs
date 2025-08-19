using Ez.Hress.Shared.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace Ez.Hress.FunctionsApi.Translation;

public class TranslationFunction
{
    private readonly string _function = nameof(TranslationFunction);
    private readonly ITranslationService _translationService;
    private readonly ILogger<TranslationFunction> _log;

    public TranslationFunction(ITranslationService translationService, ILogger<TranslationFunction> log)
    {
        _translationService = translationService;
        _log = log;
    }

    [Function("translate")]
    public async Task<IActionResult> Translate(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "translate")] HttpRequest req)
    {
        var methodName = nameof(Translate);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is required");
            }

            // Parse request
            var request = JsonSerializer.Deserialize<TranslationRequest>(requestBody);
            if (request == null || string.IsNullOrEmpty(request.Text))
            {
                return new BadRequestObjectResult("Text is required");
            }

            // Set default source language if not provided
            var sourceLanguage = string.IsNullOrEmpty(request.SourceLanguage) ? "en" : request.SourceLanguage;

            // Translate the text
            var translatedText = await _translationService.TranslateAsync(request.Text, sourceLanguage);

            var response = new TranslationResponse
            {
                OriginalText = request.Text,
                TranslatedText = translatedText,
                SourceLanguage = sourceLanguage,
                TargetLanguage = "is" // Always Icelandic
            };

            return new OkObjectResult(response);
        }
        catch (JsonException jex)
        {
            _log.LogError(jex, "[{Class}.{Method}] Invalid JSON in request body", _function, methodName);
            return new BadRequestObjectResult("Invalid JSON in request body");
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
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[{_function}] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("translateBatch")]
    public async Task<IActionResult> TranslateBatch(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "translate/batch")] HttpRequest req)
    {
        var methodName = nameof(TranslateBatch);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            // Read request body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Request body is required");
            }

            // Parse request
            var request = JsonSerializer.Deserialize<BatchTranslationRequest>(requestBody);
            if (request == null || request.Texts == null || request.Texts.Count == 0)
            {
                return new BadRequestObjectResult("Texts array is required and cannot be empty");
            }

            // Set default source language if not provided
            var sourceLanguage = string.IsNullOrEmpty(request.SourceLanguage) ? "en" : request.SourceLanguage;

            // Translate all texts
            var translatedTexts = await _translationService.TranslateListAsync(request.Texts, sourceLanguage);

            var response = new BatchTranslationResponse
            {
                OriginalTexts = request.Texts,
                TranslatedTexts = translatedTexts.ToList(),
                SourceLanguage = sourceLanguage,
                TargetLanguage = "is" // Always Icelandic
            };

            return new OkObjectResult(response);
        }
        catch (JsonException jex)
        {
            _log.LogError(jex, "[{Class}.{Method}] Invalid JSON in request body", _function, methodName);
            return new BadRequestObjectResult("Invalid JSON in request body");
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
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[{_function}] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }

    [Function("translateCache")]
    public async Task<IActionResult> GetCachedTranslation(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "translate/cache")] HttpRequest req)
    {
        var methodName = nameof(GetCachedTranslation);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", _function, methodName);

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            // Get query parameters
            if (!req.Query.ContainsKey("text"))
            {
                return new BadRequestObjectResult("Text parameter is required");
            }

            var text = req.Query["text"].ToString();
            var sourceLanguage = req.Query.ContainsKey("sourceLanguage") ? req.Query["sourceLanguage"].ToString() : "en";

            // Get cached translation
            var cachedTranslation = await _translationService.GetCachedTranslationAsync(text, sourceLanguage);

            if (cachedTranslation == null)
            {
                return new NotFoundResult();
            }

            var response = new TranslationResponse
            {
                OriginalText = text,
                TranslatedText = cachedTranslation,
                SourceLanguage = sourceLanguage,
                TargetLanguage = "is", // Always Icelandic
                IsCached = true
            };

            return new OkObjectResult(response);
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
        finally
        {
            stopwatch.Stop();
            _log.LogInformation($"[{_function}] Elapsed: {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}

// Request/Response models
public class TranslationRequest
{
    public string Text { get; set; } = string.Empty;
    public string? SourceLanguage { get; set; }
}

public class TranslationResponse
{
    public string OriginalText { get; set; } = string.Empty;
    public string TranslatedText { get; set; } = string.Empty;
    public string SourceLanguage { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
    public bool IsCached { get; set; } = false;
}

public class BatchTranslationRequest
{
    public List<string> Texts { get; set; } = new();
    public string? SourceLanguage { get; set; }
}

public class BatchTranslationResponse
{
    public List<string> OriginalTexts { get; set; } = new();
    public List<string> TranslatedTexts { get; set; } = new();
    public string SourceLanguage { get; set; } = string.Empty;
    public string TargetLanguage { get; set; } = string.Empty;
}
