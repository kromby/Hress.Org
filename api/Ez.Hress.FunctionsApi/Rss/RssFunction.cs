using Ez.Hress.Scripts.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace Ez.Hress.FunctionsApi.Rss;

public class RssFunction
{
    private readonly NewsInteractor _newsInteractor;
    private readonly ILogger<RssFunction> _log;

    public RssFunction(NewsInteractor newsInteractor, ILogger<RssFunction> log)
    {
        _newsInteractor = newsInteractor;
        _log = log;
    }

    [Function("rss")]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
    {
        var methodName = nameof(RunAsync);
        _log.LogInformation("[{Class}.{Method}] C# HTTP trigger function processed a request.", nameof(RssFunction), methodName);

        _log.LogInformation("[{Function}] Getting latest news", nameof(RunAsync));
        var list = await _newsInteractor.GetLatestNewsAsync();

        StringBuilder sb = new();

        sb.AppendLine("<rss version=\"2.0\">");
        sb.AppendLine("<channel>");
        sb.AppendLine("<title>Hress.Org</title>");
        sb.AppendLine("<link>http://www.hress.org</link>");
        sb.AppendLine("<description>Þar sem hressleikinn býr!</description>");
        foreach (var item in list)
        {
            sb.AppendLine("<item>");
            sb.AppendLine($"<title>{RemoveIllegalCharacters(item.Name)}</title>");
            sb.AppendLine($"<link>https://www.hress.org/news/{item.ID}</link>");
            sb.AppendLine($"<author>{RemoveIllegalCharacters(item.Author.Username)}</author>");
            sb.AppendLine($"<pubDate>{item.Inserted.ToString("R")}</pubDate>");
            sb.AppendLine($"<description>{RemoveIllegalCharacters(item.Content)}</description>");
            sb.AppendLine("</item>");
        }
        sb.AppendLine("</channel>");
        sb.AppendLine("</rss>");
        var rssString = sb.ToString();
        
        return new ContentResult { Content = rssString, ContentType = "application/xml", StatusCode = 200 };
    }

    protected static string RemoveIllegalCharacters(object input)
    {
        // cast the input to a string
        string data = input.ToString();

        // replace illegal characters in XML documents with their entity references
        data = data.Replace("&", "&amp;");
        data = data.Replace("\"", "&quot;");
        data = data.Replace("'", "&apos;");
        data = data.Replace("<", "&lt;");
        data = data.Replace(">", "&gt;");

        return data;
    }
}
