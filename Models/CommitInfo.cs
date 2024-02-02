namespace MyGitClient.Models
{
    // <summary>Git 커밋 정보</summary>
    public class CommitInfo(string sha, string authorName, string authorEmail, string authorWhen, string message)
    {
        public string Sha { get; set; } = sha;
        public string AuthorName { get; set; } = authorName;
        public string AuthorEmail { get; set; } = authorEmail;
        public string AuthorWhen { get; set; } = authorWhen;
        public string Message { get; set; } = message;
    }
}