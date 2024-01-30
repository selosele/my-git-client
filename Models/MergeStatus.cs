namespace MyGitClient.Models;

// <summary>Git Merge 상태 값 enum</summary>
public enum MergeStatus
{
    // 변경 사항 미존재
    UpToDate,
    // 변경 사항 존재
    FastForward
}