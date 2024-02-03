namespace MyGitClient.Models;

/// <summary>
/// Git 사용자 정보
/// </summary>
/// <param name="userName"></param>
/// <param name="userEmail"></param>
public class AuthorInfo(string userName, string userEmail)
{

    #region Fields
    public string UserName { get; set; } = userName;
    public string UserEmail { get; set; } = userEmail;
    #endregion
    
}