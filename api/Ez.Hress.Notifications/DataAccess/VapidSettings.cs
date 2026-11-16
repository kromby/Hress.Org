using Lib.Net.Http.WebPush.Authentication;

namespace Ez.Hress.Notifications.DataAccess;

public class VapidSettings(string publicKey, string privateKey, string subject)
{
    public string PublicKey { get; } = publicKey;
    public string PrivateKey { get; } = privateKey;
    public string Subject { get; } = subject;

    /// <summary>
    /// Checks the key and subject formats the same way sending does, so a bad setting fails at startup
    /// instead of on the first queue message.
    /// </summary>
    public void Validate()
    {
        try
        {
            _ = new VapidAuthentication(PublicKey, PrivateKey) { Subject = Subject };
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            throw new InvalidOperationException($"Invalid VAPID settings: {ex.Message} (see README.md).", ex);
        }
    }
}
