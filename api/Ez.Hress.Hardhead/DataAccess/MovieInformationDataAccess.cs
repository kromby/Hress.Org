using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess;

public class MovieInformationDataAccess : IMovieInformationDataAccess
{
    private readonly ILogger<MovieInformationDataAccess> _log;
    private readonly TableClient _tableClient;

    public MovieInformationDataAccess(BlobConnectionInfo connectionInfo, ILogger<MovieInformationDataAccess> log)
    {
        _tableClient = new TableClient(connectionInfo.ConnectionString, "Movies");
        _log = log;
    }

    public async Task<bool> SaveMovieInformationAsync(MovieInfo movieInfo)
    {
        _log.LogInformation("Saving movie information: {MovieInfo}", movieInfo);

        MovieInfoTableEntity movieInfoTableEntity = new(movieInfo);
        var response = await _tableClient.UpsertEntityAsync<MovieInfoTableEntity>(movieInfoTableEntity);
        if(response.IsError)
        {
            _log.LogError("Error saving movie information: {MovieInfo}", movieInfo);
            throw new SystemException("Error saving to Data Store");
        }

        return true;
    }
}

