namespace Ez.Hress.Shared.UseCases;

public interface IVideoContentDataAccess
{
    Task<byte[]?> GetContent(string path);
    
    //Task<Stream?> GetContentStream(string path);
    
    //Task<long> GetContentSize(string path);
    
    //Task<string?> GetContentType(string path);

    //Task Save(string container, byte[] content, int id);
    
    //Task SaveStream(string container, Stream content, int id, string contentType);
} 