using Ez.Hress.Scripts.Entities;
using Microsoft.Extensions.Logging;

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
}