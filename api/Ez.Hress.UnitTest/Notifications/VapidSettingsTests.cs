using Ez.Hress.Notifications.DataAccess;

namespace Ez.Hress.UnitTest.Notifications;

public class VapidSettingsTests
{
    [Theory]
    [InlineData("not-a-key", "not-a-key", "mailto:admin@hress.org")]
    [InlineData("", "", "mailto:admin@hress.org")]
    public void Validate_MalformedKeys_ThrowsInvalidOperation(string publicKey, string privateKey, string subject)
    {
        var settings = new VapidSettings(publicKey, privateKey, subject);
        Assert.Throws<InvalidOperationException>(settings.Validate);
    }
}
