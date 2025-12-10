# Hress.Org Architecture Rules

This document outlines the architectural patterns, conventions, and rules that should be followed when working with the Hress.Org codebase.

## 1. Entity Pattern

### Domain Entities

- **Location**: `/Entities` folder
- **Purpose**: Pure business objects with no knowledge of storage
- **Characteristics**:
  - No Azure Table Storage dependencies
  - No database-specific attributes
  - Focus on business logic and data representation

### Table Entities

- **Location**: `/DataAccess` folder
- **Purpose**: Azure Table Storage entities implementing `ITableEntity`
- **Naming Convention**: `{EntityName}TableEntity`
- **Required Properties**:
  - `PartitionKey` (string)
  - `RowKey` (string)
  - `Timestamp` (DateTimeOffset?)
  - `ETag` (ETag)
- **Constructor Pattern**: Should have constructors that take domain entities

**Example**:

```csharp
// Domain Entity: /Entities/MovieInfo.cs
public class MovieInfo : EntityBase<int>
{
    public string Name { get; set; }
    public IList<string> Language { get; set; }
    // ... other properties
}

// Table Entity: /DataAccess/MovieInfoTableEntity.cs
internal class MovieInfoTableEntity : ITableEntity
{
    public MovieInfoTableEntity(MovieInfo movieInfo)
    {
        PartitionKey = movieInfo.ID.ToString();
        RowKey = movieInfo.Name ?? "No Name";
        Language = string.Join(", ", movieInfo.Language ?? []);
        // ... other mappings
    }
    // ... ITableEntity implementation
}
```

## 2. Azure Table Storage Pattern

### Connection and Setup

- Use `BlobConnectionInfo` for connection strings
- Table names are typically plural (e.g., "Movies", "Crew", "Translations")
- RowKey encoding: URL encode special characters for RowKey values

### Table Client Usage

```csharp
public class SomeDataAccess
{
    private readonly TableClient _tableClient;

    public SomeDataAccess(BlobConnectionInfo connectionInfo, ILogger<SomeDataAccess> log)
    {
        _tableClient = new TableClient(connectionInfo.ConnectionString, "TableName");
    }
}
```

## 3. Dependency Injection Pattern

### Registration

- Use cases / interactors don't have interfaces
- All services registered in `Program.cs` using `services.AddSingleton<TInterface, TImplementation>()`
- Data access classes for Azure Blob and Table Storage take `BlobConnectionInfo` and `ILogger<T>` in constructors
- Follow existing naming patterns for consistency

### Example Registration

```csharp
// In Program.cs
services.AddSingleton<ITranslationService, CachedTranslationService>();
services.AddSingleton<ITranslationDataAccess, TranslationTableAccess>();
services.AddHttpClient<ITranslationProvider, FreeTranslateApiProvider>();
```

## 4. Error Handling Strategy

### Graceful Degradation

- Non-critical operations (like translation) should not fail the entire operation
- Log errors but continue with original/fallback data
- Use specific exception types for different failure scenarios

### Logging Pattern

- Use structured logging with `ILogger<T>`
- Include class name and method name in log messages
- Log at appropriate levels (Information, Warning, Error)
- Include relevant context data in log messages

**Example**:

```csharp
_log.LogInformation("[{Class}.{Method}] Processing translation for text: {Text}",
    nameof(MovieInteractor), nameof(SaveMovieInformationAsync), text);
```

## 5. Data Access Pattern

### Interface Design

- **Interface**: `I{EntityName}DataAccess` in `/UseCases` folder
- **Implementation**: `{EntityName}TableAccess` or `{EntityName}SqlAccess` in `/DataAccess` folder
- **Methods**: Async methods returning `Task<T>` or `Task<bool>`

### Example Interface

```csharp
public interface ITranslationDataAccess
{
    Task<string?> GetTranslationAsync(string sourceText, string sourceLanguage);
    Task SaveTranslationAsync(Translation translation);
    Task<IList<Translation>> GetTranslationsAsync(string sourceLanguage);
}
```

## 6. Configuration Pattern

### Settings Management

- Use `IConfiguration` for settings
- Environment-specific configuration through `host.json` and environment variables
- Feature flags for enabling/disabling functionality

### Example Configuration

```csharp
// In Program.cs
string translationApiUrl = config["Translation:ApiUrl"];
bool enableTranslation = bool.Parse(config["Translation:Enabled"] ?? "true");
```

## 7. Code Organization

### Namespace Structure

- Follow existing patterns (e.g., `Ez.Hress.Shared.UseCases`)
- Group related functionality in appropriate folders
- Use descriptive names that follow existing conventions

### File Structure

```
api/
├── Ez.Hress.Shared/
│   ├── Entities/
│   ├── UseCases/
│   ├── DataAccess/
│   └── Services/
├── Ez.Hress.Hardhead/
│   ├── Entities/
│   ├── UseCases/
│   └── DataAccess/
└── Ez.Hress.FunctionsApi/
    └── Program.cs
```

## 8. Testing Considerations

### Mockability

- Data access classes should be testable with dependency injection
- Error scenarios should be testable
- External API calls should be abstracted for testing

### Test Structure

- Unit tests for business logic
- Integration tests for data access
- Mock external dependencies

## 9. Performance Patterns

### Optimization Strategies

- Use async/await throughout the codebase
- Consider rate limiting for external API calls

## 10. Security Considerations

### Data Protection

- Never log sensitive information
- Use secure connection strings
- Validate input data
- Implement proper authentication and authorization

### API Security

- Use HTTPS for external API calls
- Implement proper error handling to avoid information leakage
- Validate API responses

---

**Note**: These rules should be followed when implementing new features or modifying existing ones. They ensure consistency across the codebase and make it easier for future development and maintenance.
