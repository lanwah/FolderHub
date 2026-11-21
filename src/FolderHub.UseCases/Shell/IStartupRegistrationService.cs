namespace FolderHub.UseCases.Shell;

/// <summary>
/// 用户级开机启动注册（FR-013 / ADR-003）。不要求管理员权限。
/// </summary>
public interface IStartupRegistrationService
{
    /// <summary>当前用户启动项是否已注册 FolderHub。</summary>
    bool IsRegistered();

    /// <summary>按偏好启用或移除用户级启动项。</summary>
    void Apply(bool enabled);
}
