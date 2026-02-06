using System.Text.Json.Serialization;

namespace EirMed.API.Models;

public sealed class ErrorResponse
{
    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, string[]>? Errors { get; init; }

    [JsonPropertyName("traceId")]
    public string? TraceId { get; init; }
}
