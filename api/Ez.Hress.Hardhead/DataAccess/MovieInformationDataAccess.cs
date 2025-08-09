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

        // Clear existing crew relationships for this movie to prevent duplicates
        await ClearExistingCrewRelationshipsAsync(movieInfoTableEntity.RowKey);

        // First, save all crew relationships without calculating movie counts
        foreach (var crewMember in movieInfo.Crew)
        {
            // Save the crew member first (without accurate MovieCounter yet)
            CrewEntity crewTableEntity = new(crewMember);
            var crewResponse = await _crewClient.UpsertEntityAsync<CrewEntity>(crewTableEntity);
            if (crewResponse.IsError)
            {
                _log.LogError("Error saving crew information: {CrewMember}", crewMember);
                throw new SystemException("Error saving Crew Member to Data Store");
            }

            // Save the relationship
            MovieCrewEntity relationTableEntity = new(movieInfoTableEntity.RowKey, crewTableEntity.RowKey, crewMember.Role.ToString());
            var relationResponse = await _relationClient.UpsertEntityAsync<MovieCrewEntity>(relationTableEntity);
            if (relationResponse.IsError)
            {
                _log.LogError("Error saving movie crew relation: {Relation}", relationTableEntity);
                throw new SystemException("Error saving Crew-Movie relation to Data Store");
            }
        }

        // Now recalculate and update movie counts for all crew members
        foreach (var crewMember in movieInfo.Crew)
        {
            var updatedCrewMember = await RecalculateCrewMemberAsync(crewMember, movieInfoTableEntity.RowKey);
            CrewEntity updatedCrewTableEntity = new(updatedCrewMember);
            
            var updateResponse = await _crewClient.UpsertEntityAsync<CrewEntity>(updatedCrewTableEntity);
            if (updateResponse.IsError)
            {
                _log.LogWarning("Error updating crew member movie count: {CrewMember}", crewMember);
                // Don't throw - this is just a counter update
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
            movies.Add(role.PartitionKey);
        }

        crewMember.MovieCounter = movies.Count;

        if (!movies.Contains(movie))
        {
            crewMember.MovieCounter++;
        }

        return crewMember;
    }

    private async Task ClearExistingCrewRelationshipsAsync(string movieKey)
    {
        _log.LogInformation("Clearing existing crew relationships for movie: {MovieKey}", movieKey);
        
        try
        {
            var existingRelations = _relationClient.QueryAsync<MovieCrewEntity>(
                x => x.PartitionKey == movieKey);

            await foreach (var relation in existingRelations)
            {
                var deleteResponse = await _relationClient.DeleteEntityAsync(relation.PartitionKey, relation.RowKey);
                if (deleteResponse.IsError)
                {
                    _log.LogWarning("Failed to delete existing crew relation: {PartitionKey}/{RowKey}", 
                        relation.PartitionKey, relation.RowKey);
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Error clearing existing crew relationships for movie {MovieKey}", movieKey);
            // Don't throw - we want to continue with saving new relationships
        }
    }

    public async Task<MovieInfo?> GetMovieInformationAsync(int movieId)
    {
        _log.LogInformation("Getting movie information for ID: {MovieId}", movieId);

        try
        {
            // Get movie information from the Movies table
            var movieResponse = _movieClient.QueryAsync<MovieInfoTableEntity>(
                x => x.PartitionKey == movieId.ToString());

            MovieInfoTableEntity? movieEntity = null;
            await foreach (var entity in movieResponse)
            {
                movieEntity = entity;
                break; // Take the first (should be only) result
            }

            if (movieEntity == null)
            {
                _log.LogInformation("No movie information found for ID: {MovieId}", movieId);
                return null;
            }

            // Convert table entity to MovieInfo
            var movieInfo = new MovieInfo(movieEntity.RowKey, movieEntity.Description ?? string.Empty, movieEntity.Rated, movieEntity.Country)
            {
                ID = movieId,
                Year = movieEntity.Year,
                Released = movieEntity.Released,
                DVDReleased = movieEntity.DVDReleased,
                Age = movieEntity.Age,
                Runtime = movieEntity.Runtime,
                Awards = movieEntity.Awards,
                Metascore = movieEntity.Metascore,
                ImdbRating = movieEntity.ImdbRating,
                ImdbVotes = movieEntity.ImdbVotes,
                ImdbID = movieEntity.ImdbID,
                BoxOffice = movieEntity.BoxOffice,
                Production = movieEntity.Production,
                Website = movieEntity.Website
            };

            // Deserialize collections from stored strings
            if (!string.IsNullOrEmpty(movieEntity.Genre))
            {
                movieInfo.Genre = movieEntity.Genre.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrEmpty(movieEntity.Language))
            {
                movieInfo.Language = movieEntity.Language.Split(", ", StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (!string.IsNullOrEmpty(movieEntity.Ratings))
            {
                try
                {
                    movieInfo.Ratings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(movieEntity.Ratings) 
                                       ?? new Dictionary<string, string>();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Failed to deserialize ratings for movie {MovieId}, using empty dictionary", movieId);
                    movieInfo.Ratings = new Dictionary<string, string>();
                }
            }

            // Get crew members for this movie
            var crewRelationResponse = _relationClient.QueryAsync<MovieCrewEntity>(
                x => x.PartitionKey == movieEntity.RowKey);

            var crewMembers = new List<CrewMember>();
            var seenCrewRoles = new HashSet<string>(); // To prevent duplicate name+role combinations

            await foreach (var relation in crewRelationResponse)
            {
                // Get crew member details
                var crewResponse = _crewClient.QueryAsync<CrewEntity>(
                    x => x.RowKey == relation.CrewMember);

                await foreach (var crewEntity in crewResponse)
                {
                    var crewName = crewEntity.RowKey;
                    
                    // Try to parse the role enum, skip if invalid
                    if (!Enum.TryParse<Role>(relation.Role, out var role))
                    {
                        continue; // Skip this crew member if role is invalid
                    }
                    
                    var crewRoleKey = $"{crewName}|{role}"; // Unique key for name+role combination
                    
                    // Only add if we haven't seen this exact name+role combination
                    if (!seenCrewRoles.Contains(crewRoleKey))
                    {
                        var crewMember = new CrewMember
                        {
                            Name = crewName,
                            Role = role,
                            MovieCounter = crewEntity.MovieCounter
                        };
                        crewMembers.Add(crewMember);
                        seenCrewRoles.Add(crewRoleKey);
                    }
                    else
                    {
                        _log.LogWarning("Duplicate crew member entry found for {CrewName} as {Role} in movie {MovieId}, skipping duplicate", 
                            crewName, role, movieId);
                    }
                    break; // Take first result
                }
            }

            movieInfo.Crew = crewMembers;

            return movieInfo;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error retrieving movie information for ID: {MovieId}", movieId);
            throw;
        }
    }
}

