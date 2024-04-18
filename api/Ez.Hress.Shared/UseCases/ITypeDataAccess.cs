using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface ITypeDataAccess
{
    Task<IList<TypeEntity>> GetTypes();
}
