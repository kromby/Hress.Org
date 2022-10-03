using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ez.Hress.Scripts.UseCases;
using System.Text;

namespace Ez.Hress.FunctionsApi.Rss
{
    public class RssFunction
    {
        private readonly NewsInteractor _newsInteractor;

        public RssFunction(NewsInteractor newsInteractor)
        {
            _newsInteractor = newsInteractor;
        }

        [FunctionName("rss")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("[{Function}] C# HTTP trigger function processed a request.", nameof(Run));

            log.LogInformation("[{Function}] Getting latest news", nameof(Run));
            var list = await _newsInteractor.GetLatestNews();

            StringBuilder sb = new StringBuilder();

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

        protected string RemoveIllegalCharacters(object input)
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
}
