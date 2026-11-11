namespace Ez.Hress.Notifications.DataAccess;

public class VapidSettings(string publicKey, string privateKey, string subject)
{
    public string PublicKey { get; } = publicKey;
    public string PrivateKey { get; } = privateKey;
    public string Subject { get; } = subject;
}
