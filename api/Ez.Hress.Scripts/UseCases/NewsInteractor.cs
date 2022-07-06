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
            return await _newsDataAccess.GetNews(10);
        }
    }
}