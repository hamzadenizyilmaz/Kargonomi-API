using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record ShipmentPage(
    [property: JsonPropertyName("data")] IReadOnlyList<Shipment> Items,
    [property: JsonPropertyName("links")] PaginationLinks Links,
    [property: JsonPropertyName("meta")] PaginationMeta Meta
    );
public sealed record PaginationLinks(
    [property: JsonPropertyName("first")] string? First,
    [property: JsonPropertyName("last")] string? Last,
    [property: JsonPropertyName("prev")] string? Previous,
    [property: JsonPropertyName("next")] string? Next
    );
public sealed record PaginationMeta(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage,
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("total")] int Total
    );
