using System.Text.RegularExpressions;
using Ez.Hress.Notifications.DataAccess;

namespace Ez.Hress.UnitTest.Notifications;

public class NotificationKeysTests
{
    [Fact]
    public void NewestFirst_LaterTimeSortsFirst()
    {
        var t = new DateTime(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc);

        var earlier = NotificationKeys.NewestFirst(t);
        var later = NotificationKeys.NewestFirst(t.AddSeconds(1));

        Assert.True(string.CompareOrdinal(later, earlier) < 0);
        Assert.Equal(19, earlier.Length);
        Assert.Equal(19, later.Length);
    }

    [Fact]
    public void EndpointKey_IsTableSafeAndStable()
    {
        const string endpoint = "https://fcm.googleapis.com/fcm/send/abc:def?x=1#y";

        var key = NotificationKeys.EndpointKey(endpoint);

        Assert.Equal(64, key.Length);
        Assert.Matches(new Regex("^[0-9a-f]+$"), key);
        Assert.Equal(key, NotificationKeys.EndpointKey(endpoint));
        Assert.NotEqual(key, NotificationKeys.EndpointKey(endpoint + "2"));
    }
}
