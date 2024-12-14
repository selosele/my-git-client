namespace MyGitClient.Models;

/// <summary>
/// Git Merge 상태 값 enum
/// </summary>
public enum MergeStatus
{
    /// <summary>
    /// 변경 사항 미존재
    /// </summary>
    UpToDate,
    
    /// <summary>
    /// 변경 사항 존재
    /// </summary>
    FastForward
}