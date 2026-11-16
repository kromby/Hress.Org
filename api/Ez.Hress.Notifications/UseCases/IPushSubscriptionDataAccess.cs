using Ez.Hress.Notifications.Entities;

namespace Ez.Hress.Notifications.UseCases;

public interface IPushSubscriptionDataAccess
{
    /// <summary>Keyed by endpoint.</summary>
    Task UpsertAsync(PushSubscription subscription);
    Task<PushSubscription?> GetAsync(string endpoint);
    Task DeleteAsync(string endpoint);
    Task<IList<PushSubscription>> GetAllAsync();
}
