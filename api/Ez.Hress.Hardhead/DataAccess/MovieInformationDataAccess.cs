using Azure.Data.Tables;
using Ez.Hress.Hardhead.Entities;
using Ez.Hress.Hardhead.UseCases;
using Ez.Hress.Shared.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Hress.Hardhead.DataAccess;

public class MovieInformationDataAccess : IMovieInformationDataAccess
{
    private readonly ILogger<MovieInformationDataAccess> _log;
    private readonly TableClient _movieClient;
    private readonly TableClient _crewClient;
    private readonly TableClient _relationClient;

    public MovieInformationDataAccess(BlobConnectionInfo connectionInfo, ILogger<MovieInformationDataAccess> log)
    {
        _movieClient = new TableClient(connectionInfo.ConnectionString, "Movies");
        _crewClient = new TableClient(connectionInfo.ConnectionString, "Crew");
        _relationClient = new TableClient(connectionInfo.ConnectionString, "MovieCrew");
        _log = log;
    }

    public async Task<bool> SaveMovieInformationAsync(MovieInfo movieInfo)
    {
        _log.LogInformation("Saving movie information: {MovieInfo}", movieInfo);

        MovieInfoTableEntity movieInfoTableEntity = new(movieInfo);
        var response = await _movieClient.UpsertEntityAsync<MovieInfoTableEntity>(movieInfoTableEntity);
        if (response.IsError)
        {
            _log.LogError("Error saving movie information: {MovieInfo}", movieInfo);
            throw new SystemException("Error saving Movie to Data Store");
        }

        foreach (var crewMember in movieInfo.Crew)
        {
            var innerCrewMember = await RecalculateCrewMemberAsync(crewMember, movieInfoTableEntity.RowKey);
            CrewEntity crewTableEntity = new(innerCrewMember);

            var crewResponse = await _crewClient.UpsertEntityAsync<CrewEntity>(crewTableEntity);
            if (crewResponse.IsError)
            {
                _log.LogError("Error saving crew information: {CrewMember}", crewMember);
                throw new SystemException("Error saving Crew Member to Data Store");
            }

            MovieCrewEntity relationTableEntity = new(movieInfoTableEntity.RowKey, crewTableEntity.RowKey, crewMember.Role.ToString());
            var relationResponse = await _relationClient.UpsertEntityAsync<MovieCrewEntity>(relationTableEntity);
            if (relationResponse.IsError)
            {
                _log.LogError("Error saving movie crew relation: {Relation}", relationTableEntity);
                throw new SystemException("Error saving Crew-Movie relation to Data Store");
            }
        }

        return true;
    }

    private async Task<CrewMember> RecalculateCrewMemberAsync(CrewMember crewMember, string movie)
    {
        var crewResponse = _relationClient.QueryAsync<MovieCrewEntity>(x => x.CrewMember == crewMember.Name);

        var movies = new HashSet<string>();
        await foreach (var role in crewResponse)
        {
            if (!movies.Contains(role.PartitionKey))
            {
                movies.Add(role.PartitionKey);
            }
        }

        crewMember.MovieCounter = movies.Count;

        if (!movies.Contains(movie))
        {
            crewMember.MovieCounter++;
        }

        return crewMember;
    }
}

