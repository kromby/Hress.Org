using Ez.Hress.Shared.UseCases;

namespace Ez.Hress.Shared.DataAccess;

public class ImageContentHttpDataAccess : IImageContentDataAccess
{
    public string Prefix { get => "http"; }

    public async Task<byte[]?> GetContent(string path)
    {
        //path = path.Replace("http://", "https://");
        var client = new HttpClient();
        return await client.GetByteArrayAsync(path);
    }

    public Task Save(string container, byte[] content, int id)
    {
        // skipcq: CS-A1003
        throw new NotImplementedException();
    }
}
