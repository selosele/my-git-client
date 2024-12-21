namespace MyGitClient.Models;

/// <summary>
/// Git 커밋 정보
/// </summary>
/// <param name="sha">커밋의 고유 식별자인 SHA</param>
/// <param name="authorName">커밋 작성자의 이름</param>
/// <param name="authorEmail">커밋 작성자의 이메일 주소</param>
/// <param name="authorWhen">커밋이 작성된 시간</param>
/// <param name="message">커밋 메시지</param>
public class CommitInfo(string sha, string authorName, string authorEmail, string authorWhen, string message)
{

    #region [필드]
    /// <summary>
    /// 커밋의 고유 식별자인 SHA
    /// </summary>
    public string Sha { get; set; } = sha;

    /// <summary>
    /// 커밋 작성자의 이름
    /// </summary>
    public string AuthorName { get; set; } = authorName;

    /// <summary>
    /// 커밋 작성자의 이메일 주소
    /// </summary>
    public string AuthorEmail { get; set; } = authorEmail;

    /// <summary>
    /// 커밋이 작성된 시간
    /// </summary>
    public string AuthorWhen { get; set; } = authorWhen;

    /// <summary>
    /// 커밋 메시지
    /// </summary>
    public string Message { get; set; } = message;
    #endregion
    
}