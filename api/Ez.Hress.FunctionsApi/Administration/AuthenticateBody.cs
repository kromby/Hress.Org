namespace Ez.Hress.FunctionsApi.Administration;

internal class AuthenticateBody
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Code { get; set; }
}
