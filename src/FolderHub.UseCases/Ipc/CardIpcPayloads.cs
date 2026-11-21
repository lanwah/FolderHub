using System.Text.Json;
using System.Text.Json.Serialization;

namespace FolderHub.UseCases.Ipc;

/// <summary>
/// WP7 业务载荷（信封 PayloadJson）。字段保持可向前扩展。
/// </summary>
public static class CardIpcJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;
        return JsonSerializer.Deserialize<T>(json, Options);
    }
}

public sealed record CardDefinitionUpsertPayload(
    Guid CardId,
    Guid FolderId,
    bool Enabled,
    string DisplayName,
    string? Alias,
    string? Description,
    string Path,
    bool IsPathUnavailable);

public sealed record CardIdPayload(Guid CardId);

public sealed record CardCloseRequestedPayload(
    Guid CardId,
    string Reason);

public sealed record CardStateChangedNoticePayload(
    Guid CardId,
    string Action,
    double? Left = null,
    double? Top = null,
    bool? Locked = null,
    long? StateVersion = null);
