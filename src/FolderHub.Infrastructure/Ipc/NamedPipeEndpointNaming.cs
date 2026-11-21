namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// 管道端点命名：应用命名空间 + SessionId + 不可预测实例标识（P3-07 §5.1）。
/// 名称本身不是秘密，也不是身份认证。
/// </summary>
public static class NamedPipeEndpointNaming
{
    public const string PipePrefix = "FolderHub.Ipc";

    public static string BuildPipeName(int sessionId, Guid hostInstanceId)
        => $"{PipePrefix}.s{sessionId}.{hostInstanceId:N}";

    public static string ToPipePath(string pipeName)
        => $@"\\.\pipe\{pipeName}";
}
