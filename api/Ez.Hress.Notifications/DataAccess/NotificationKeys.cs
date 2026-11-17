using System.Security.Cryptography;
using System.Text;

namespace Ez.Hress.Notifications.DataAccess;

internal static class NotificationKeys
{
    /// <summary>Partition for notifications addressed to everyone, and for all push subscriptions.</summary>
    public const string AllPartition = "all";

    /// <summary>Row key prefix that sorts ascending as newest first.</summary>
    public static string NewestFirst(DateTime createdUtc)
    {
        return (DateTime.MaxValue.Ticks - createdUtc.Ticks).ToString("D19");
    }

    /// <summary>Endpoint URLs contain characters that are illegal in row keys, so use a hash.</summary>
    public static string EndpointKey(string endpoint)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(endpoint))).ToLowerInvariant();
    }
}
