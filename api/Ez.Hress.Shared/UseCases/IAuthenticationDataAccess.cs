namespace Ez.Hress.Shared.UseCases
{
    public interface IAuthenticationDataAccess
    {
        Task<int> GetUserID(string username, string hashedPassword);

        Task SaveLoginInformation(int userId, string ipAddress);
    }
}
