namespace MyGitClient.Models;

/// <summary>
/// Git 사용자 정보
/// </summary>
/// <param name="userName">Git 사용자 이름</param>
/// <param name="userEmail">Git 사용자 이메일</param>
public class AuthorInfo(string userName, string userEmail)
{

    #region [필드]
    /// <summary>
    /// Git 사용자 이름
    /// </summary>
    public string UserName { get; set; } = userName;

    /// <summary>
    /// Git 사용자 이메일
    /// </summary>
    public string UserEmail { get; set; } = userEmail;
    #endregion
    
}