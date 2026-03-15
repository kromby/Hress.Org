using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IUserDataAccess
{
    Task<UserBasicEntity> GetUser(int id);
    Task<IList<UserBasicEntity>> GetUsers();
    Task<IList<UserBasicEntity>> GetUsersByRole(string roleCode);
}
