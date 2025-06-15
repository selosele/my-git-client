namespace MyGitClient.Models;

/// <summary>
/// 애플리케이션 상태를 저장하는 클래스
/// </summary>
public static class AppState
{
    #region [필드]
    /// <summary>
    /// 선택된 경로
    /// </summary>
    public static string? SelectedPath { get; set; }
    #endregion
}