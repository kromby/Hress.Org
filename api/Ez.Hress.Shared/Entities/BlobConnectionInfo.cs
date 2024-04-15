namespace Ez.Hress.Shared.Entities;

public class BlobConnectionInfo
{
    public BlobConnectionInfo(string connectionString)
    {
        ConnectionString = connectionString;
    }
    
    public string ConnectionString { get; set; }
}
