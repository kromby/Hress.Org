using Ez.Hress.Shared.Entities;

namespace Ez.Hress.Shared.UseCases;

public interface IImageInfoDataAccess
{
    Task<ImageEntity?> GetImage(int id);

    Task<int> Save(ImageEntity entity, int typeID, int height, int width);
}
