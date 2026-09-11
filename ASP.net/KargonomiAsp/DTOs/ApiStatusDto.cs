namespace Kargonomi.AspNet.DTOs;

public sealed record ApiStatusDto(
    string Service,
    string Environment,
    bool KargonomiConfigured,
    DateTimeOffset TimestampUtc);
