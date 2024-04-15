namespace Ez.Hress.Shared.Entities;

public class AuthenticationInfo
{
    public AuthenticationInfo(string key, string issuer, string audience, string salt)
    {
        Key = key;
        Issuer = issuer;
        Audience = audience;
        Salt = salt;
    }

    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Salt { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            throw new ArgumentNullException(nameof(Key));

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new ArgumentNullException(nameof(Issuer));

        if (string.IsNullOrWhiteSpace(Audience))
            throw new ArgumentNullException(nameof(Audience));

        if (string.IsNullOrEmpty(Salt))
            throw new ArgumentNullException(nameof(Salt));
    }
}
