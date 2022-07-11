using Ez.Hress.Scripts.Entities;
using Microsoft.Extensions.Logging;

namespace Ez.Hress.Scripts.UseCases
{
    public class NewsInteractor
    {
        private readonly INewsDataAccess _newsDataAccess;
        private readonly ILogger<NewsInteractor> _log;

        public NewsInteractor(INewsDataAccess newsDataAccess, ILogger<NewsInteractor> log)
        {
            _newsDataAccess = newsDataAccess;
            _log = log;
        }

        public async Task<IList<News>> GetLatestNews()
        {
            return await _newsDataAccess.GetLatestNews(10);
        }

        public async Task<News> GetNews(int id)
        {
            return await _newsDataAccess.GetNews(id);
        }

        public async Task<IList<News>> GetHistoricalNews(int year)
        {
            return null;
        }

        public async Task<IList<News>> GetHistoricalNews(int year, int month)
        {
            return null;
        }

        public async Task<IList<News>> GetHistoricalNews(int year, int month, int day)
        {
            return null;
        }
    }
}