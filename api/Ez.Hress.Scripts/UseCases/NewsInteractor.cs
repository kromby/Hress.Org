using Ez.Hress.Scripts.Entities;
using Microsoft.Extensions.Logging;
using Ganss.Xss;
using System.Text.RegularExpressions;

namespace Ez.Hress.Scripts.UseCases;

public class NewsInteractor
{
    private readonly INewsDataAccess _newsDataAccess;
    private readonly ILogger<NewsInteractor> _log;

    public NewsInteractor(INewsDataAccess newsDataAccess, ILogger<NewsInteractor> log)
    {
        _newsDataAccess = newsDataAccess;
        _log = log;
    }

    public async Task<IList<News>> GetLatestNewsAsync()
    {
        return await _newsDataAccess.GetLatestNews(10);
    }

    public async Task<News> GetNewsAsync(int id)
    {
        return await _newsDataAccess.GetNews(id);
    }

    public async Task<IList<News>> GetNewsOnThisDayAsync(int top)
    {
        _log.LogInformation($"[{nameof(NewsInteractor)}] Get historical news on this day");
        
        var date = DateTime.Today;
        var list = await _newsDataAccess.GetNews(date, true, true, false);

        if(list.Count > 1 && top == 1)
        {
            // skipcq: CS-A1008
            int rnd = new Random().Next(list.Count);
            return list.Where(n => n.ID == list[rnd].ID).ToList();
        }

        return list;
    }

    public async Task<IList<News>> GetNewsByYearAsync(int year)
    {
        if (year < 2000 && year <= DateTime.Today.Year)
        {
            throw new ArgumentException("Year must be after 2000 and not in the future", nameof(year));
        }

        return await _newsDataAccess.GetNews(new DateTime(year, 1, 1), false, false, true);
    }

    public async Task<IList<News>> GetNewsByYearAndMonthAsync(int year, int month)
    {
        if (year < 2000 && year <= DateTime.Today.Year)
        {
            throw new ArgumentException("Year must be after 2000 and not in the future", nameof(year));
        }

        if (month < 1 && month > 12)
        {
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));
        }

        return await _newsDataAccess.GetNews(new DateTime(year, month, 1), false, true, true);
    }

    public async Task<IList<StatisticNewsByDate>> GetNewsYearStatisticsAsync()
    {
        return await _newsDataAccess.GetNewsStatisticsByDate(null);
    }

    public async Task<IList<StatisticNewsByDate>> GetNewsMonthStatisticsAsync(int year)
    {
        return await _newsDataAccess.GetNewsStatisticsByDate(year);
    }

    public async Task<IList<News>> GetNewsByAuthorAsync(int userId)
    {
        if (userId < 1)
        {
            throw new ArgumentException("User ID must be a positive number", nameof(userId));
        }

        _log.LogInformation("[{Class}.{Method}] Get news by author '{UserId}'", nameof(NewsInteractor), nameof(GetNewsByAuthorAsync), userId);
        return await _newsDataAccess.GetNewsByAuthor(userId);
    }

    public async Task<News> CreateNewsAsync(News news, int userId)
    {
        ValidateAndPrepare(news, userId);
        news.InsertedBy = userId;
        news.Inserted = DateTime.UtcNow;

        _log.LogInformation("[{Class}.{Method}] Creating news '{Name}' for user '{UserId}'", nameof(NewsInteractor), nameof(CreateNewsAsync), news.Name, userId);
        news.ID = await _newsDataAccess.CreateNews(news);
        return news;
    }

    public async Task<News> UpdateNewsAsync(int id, News news, int userId)
    {
        if (id < 1)
        {
            throw new ArgumentException("News ID must be a positive number", nameof(id));
        }

        ValidateAndPrepare(news, userId);

        var existing = await _newsDataAccess.GetNews(id);
        if (existing == null || existing.ID < 1)
        {
            throw new KeyNotFoundException($"News '{id}' not found");
        }

        if (existing.InsertedBy != userId)
        {
            _log.LogWarning("[{Class}.{Method}] User '{UserId}' is not the author of news '{Id}'", nameof(NewsInteractor), nameof(UpdateNewsAsync), userId, id);
            throw new UnauthorizedAccessException("Only the author can edit this news");
        }

        news.ID = id;
        news.InsertedBy = existing.InsertedBy;
        news.Inserted = existing.Inserted;
        news.UpdatedBy = userId;
        news.Updated = DateTime.UtcNow;

        _log.LogInformation("[{Class}.{Method}] Updating news '{Id}' for user '{UserId}'", nameof(NewsInteractor), nameof(UpdateNewsAsync), id, userId);
        await _newsDataAccess.UpdateNews(news);
        return news;
    }

    private static void ValidateAndPrepare(News news, int userId)
    {
        if (userId < 1)
        {
            throw new ArgumentException("User ID must be a positive number", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(news.Name))
        {
            throw new ArgumentException("Headline is required", nameof(news));
        }

        if (string.IsNullOrWhiteSpace(news.Content))
        {
            throw new ArgumentException("Body is required", nameof(news));
        }

        if (news.Image != null)
        {
            if (news.Image.ID < 1)
            {
                throw new ArgumentException("Image ID must be a positive number", nameof(news));
            }

            if (news.ImageAlign == default)
            {
                news.ImageAlign = Align.Left;
            }
            else if (!Enum.IsDefined(news.ImageAlign))
            {
                throw new ArgumentException($"Invalid image alignment '{(int)news.ImageAlign}'", nameof(news));
            }
        }

        var content = NewsHtmlSanitizer.Sanitize(news.Content);
        if (!HasVisibleContent(content))
        {
            throw new ArgumentException("Body is required", nameof(news));
        }

        news.Name = news.Name.Trim();
        news.Content = OpenExternalLinksInNewTab(content);
    }

    // News bodies are user-authored HTML; only the formatting the editor (and legacy news) uses is kept.
    private static readonly HtmlSanitizer NewsHtmlSanitizer = CreateSanitizer();

    private static HtmlSanitizer CreateSanitizer()
    {
        var options = new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "p", "br", "strong", "b", "em", "i", "u", "s", "span", "div",
                "ul", "ol", "li", "h2", "h3", "blockquote", "code", "pre", "a", "img",
            },
            AllowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "href", "target", "src", "alt", "title", "width", "height",
            },
            AllowedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "http", "https", "mailto" },
            UriAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "href", "src" },
        };
        return new HtmlSanitizer(options);
    }

    private static readonly Regex HtmlTag = new(@"<[^>]*>", RegexOptions.Compiled);
    // Runs on sanitized output, where an unsafe src has already been removed.
    private static readonly Regex ImageWithSource = new(@"<img\b[^>]*\bsrc\s*=\s*[""'][^""'\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static bool HasVisibleContent(string html)
    {
        return ImageWithSource.IsMatch(html)
            || !string.IsNullOrWhiteSpace(System.Net.WebUtility.HtmlDecode(HtmlTag.Replace(html, string.Empty)));
    }

    // Ported from legacy ContentManager.HandleHyperlinks: external links open in a new tab.
    private static readonly Regex AnchorTag = new(@"<a\s[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TargetAttribute = new(@"(?<![\w-])target\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ExternalHref = new(@"(?<![\w-])href\s*=\s*[""']?https?:", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string OpenExternalLinksInNewTab(string html)
    {
        return AnchorTag.Replace(html, m =>
            ExternalHref.IsMatch(m.Value) && !TargetAttribute.IsMatch(m.Value)
                ? m.Value.Insert(2, " target=\"_blank\"")
                : m.Value);
    }
}
