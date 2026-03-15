using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IUserInteractor
{
    Task<UserBasicEntity> GetUser(int id);
    Task<IList<UserBasicEntity>> GetUsers(string? role = null);
}