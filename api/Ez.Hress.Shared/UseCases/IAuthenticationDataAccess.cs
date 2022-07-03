namespace Ez.Hress.Shared.UseCases
{
    public interface IAuthenticationDataAccess
    {
        Task<int> GetUserID(string username, string hashedPassword);

        Task SaveLoginInformation(int userID, string ipAddress);

        Task<int> SaveMagicCode(int userID, string code, DateTime expires);
    }
}
