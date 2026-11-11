using Ez.Hress.NotificationFunctions;

namespace Ez.Hress.UnitTest.Notifications;

public class NotificationPushMessageTests
{
    [Theory]
    [InlineData("{\"notificationID\":\"all|abc\"}")]
    [InlineData("{\"NotificationID\":\"all|abc\"}")]
    [InlineData("{\"notificationId\":\"all|abc\"}")]
    public void Parse_ShouldReadNotificationID_RegardlessOfCasing(string body)
    {
        var message = NotificationPushMessage.Parse(body);

        Assert.Equal("all|abc", message.NotificationID);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"notificationID\":\"\"}")]
    public void Parse_ShouldThrow_WhenNotificationIDIsMissing(string body)
    {
        Assert.Throws<InvalidOperationException>(() => NotificationPushMessage.Parse(body));
    }
}
