using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Shared.UseCases
{
    public class TypeInteractor : ITypeInteractor
    {
        private readonly ITypeDataAccess _typeDataAccess;
        private readonly ILogger<TypeInteractor> _log;
        private readonly string _class = nameof(TypeInteractor);

        public TypeInteractor(ITypeDataAccess typeDataAccess, ILogger<TypeInteractor> log)
        {
            _typeDataAccess = typeDataAccess;
            _log = log;
        }

        public async Task<IList<TypeEntity>> GetEzTypes()
        {
            _log.LogInformation("[{Class}] GetTypes", _class);
            return await _typeDataAccess.GetTypes();
        }

        public async Task<TypeEntity> GetEzType(string code)
        {
            var list = await GetEzTypes();
            var type = list.First(t => t.Code == code);
            return type;
        }

        public async Task<TypeEntity> GetEzType(int id)
        {
            var list = await GetEzTypes();
            var type = list.First(t => t.ID == id);
            return type;
        }
    }
}
