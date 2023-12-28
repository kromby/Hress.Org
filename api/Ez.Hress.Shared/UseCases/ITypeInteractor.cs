using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases
{
    public interface ITypeInteractor
    {
        Task<IList<TypeEntity>> GetEzTypes();

        Task<TypeEntity> GetEzType(string code);

        Task<TypeEntity> GetEzType(int id);
    }
}