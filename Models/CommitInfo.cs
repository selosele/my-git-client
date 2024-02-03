namespace MyGitClient.Models;

/// <summary>
/// Git 커밋 정보
/// </summary>
/// <param name="sha"></param>
/// <param name="authorName"></param>
/// <param name="authorEmail"></param>
/// <param name="authorWhen"></param>
/// <param name="message"></param>
public class CommitInfo(string sha, string authorName, string authorEmail, string authorWhen, string message)
{

    #region Fields
    public string Sha { get; set; } = sha;
    public string AuthorName { get; set; } = authorName;
    public string AuthorEmail { get; set; } = authorEmail;
    public string AuthorWhen { get; set; } = authorWhen;
    public string Message { get; set; } = message;
    #endregion
    
}