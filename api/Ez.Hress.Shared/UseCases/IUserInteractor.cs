using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases
{
    public interface IUserInteractor
    {
        Task<UserBasicEntity> GetUser(int id);
    }
}