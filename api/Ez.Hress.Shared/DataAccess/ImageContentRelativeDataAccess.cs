using Ez.Hress.Shared.UseCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.DataAccess
{
    public class ImageContentRelativeDataAccess : IImageContentDataAccess
    {
        public string Prefix { get => "~/"; }

        public async Task<byte[]> GetContent(string path)
        {
            path = path.Replace("~/", "https://hress.azurewebsites.net/");
            var client = new HttpClient();
            return await client.GetByteArrayAsync(path);
        }
    }
}
