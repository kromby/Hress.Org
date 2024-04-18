namespace Ez.Hress.Shared.UseCases;

public interface IImageContentDataAccess
{
    Task<byte[]?> GetContent(string path);

    string Prefix { get; }

    Task Save(string container, byte[] content, int id);
}
