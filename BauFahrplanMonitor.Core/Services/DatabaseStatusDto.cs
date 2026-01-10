namespace BauFahrplanMonitor.Core.Services;

// Core/Dto/DatabaseStatusDto.cs

public sealed record DatabaseStatusDto(
    DatabaseService.DatabaseHealthStatus Status,
    int?                                 CurrentSchemaVersion,
    int                                  ExpectedSchemaVersion,
    string                               Message,
    DatabaseConnectionDto                Connection
);

public sealed record DatabaseConnectionDto(
    string Host,
    int    Port,
    string Database,
    string User
);