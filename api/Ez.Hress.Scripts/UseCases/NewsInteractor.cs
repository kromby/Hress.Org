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

        public async Task<IList<News>> GetNewsOnThisDay(int top)
        {
            _log.LogInformation($"[{nameof(NewsInteractor)}] Get historical news on this day");
            
            var date = DateTime.Today;
            var list = await _newsDataAccess.GetNews(date, true);

            if(list.Count() > 1)
            {
                int rnd = new Random().Next(list.Count());
                return list.Where(n => n.ID == list[rnd].ID).ToList();
            }

            return list;
        }
    }
}