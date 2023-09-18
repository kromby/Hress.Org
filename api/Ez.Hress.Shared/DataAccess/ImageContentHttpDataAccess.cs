using Ez.Hress.Shared.UseCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
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
            throw new NotImplementedException();
        }
    }
}
