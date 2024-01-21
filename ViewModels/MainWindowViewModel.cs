using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace MyGitClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        //UpdateRepoInfo(@"C:\Users\dmitr\Desktop\p\repo\selosele-pub");
        LeftMenuItems = ["Status", "History"];
    }

    #region Properties
    /** <summary>최신 Repository 텍스트</summary> */
    private string _repositoryPath = "Load Git repository...";
    public string RepositoryPath
    {
        get => _repositoryPath;
        set => this.RaiseAndSetIfChanged(ref _repositoryPath, value);
    }

    /** <summary>최신 브랜치 텍스트</summary> */
    private string? _currentBranchText;
    public string? CurrentBranchText
    {
        get => _currentBranchText;
        set => this.RaiseAndSetIfChanged(ref _currentBranchText, value);
    }

    /** <summary>최신 커밋 텍스트</summary> */
    private string? _latestCommitText;
    public string? LatestCommitText
    {
        get => _latestCommitText;
        set => this.RaiseAndSetIfChanged(ref _latestCommitText, value);
    }

    /** <summary>최신 커밋 일시 텍스트</summary> */
    private string? _latestCommitDateText;
    public string? LatestCommitDateText
    {
        get => _latestCommitDateText;
        set => this.RaiseAndSetIfChanged(ref _latestCommitDateText, value);
    }

    /** <summary>액션버튼 Border 표시 여부</summary> */
    private bool? _actionButtonsBorderVisible = false;
    public bool? ActionButtonsBorderVisible
    {
        get => _actionButtonsBorderVisible;
        set => this.RaiseAndSetIfChanged(ref _actionButtonsBorderVisible, value);
    }

    /** <summary>커밋 정보 데이타그리드 표시 여부</summary> */
    private bool? _commitsDataGridVisible = false;
    public bool? CommitsDataGridVisible
    {
        get => _commitsDataGridVisible;
        set => this.RaiseAndSetIfChanged(ref _commitsDataGridVisible, value);
    }

    /** <summary>커밋 정보 목록</summary> */
    private ObservableCollection<CommitInfo>? _commits;
    public ObservableCollection<CommitInfo>? Commits
    {
        get => _commits;
        set => this.RaiseAndSetIfChanged(ref _commits, value);
    }

    /** <summary>레프트 메뉴 아이템</summary> */
    public ObservableCollection<string>? LeftMenuItems { get; set; }
    #endregion

    /** <summary>Git 저장소 정보를 가져와 화면에 표시한다.</summary> */
    [Obsolete]
    public async void UpdateRepoInfo()
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Select Git Repository Folder",
            Directory = Directory.GetCurrentDirectory() // 기본 경로 설정
        };

        var selectedFolderPath = await folderDialog.ShowAsync(Views.MainWindow.Instance);

        if (!string.IsNullOrEmpty(selectedFolderPath))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedFolderPath) || !Directory.Exists(selectedFolderPath))
                {
                    // 경로가 유효하지 않을 경우 사용자에게 메시지를 표출한다.
                    await AlertBox("Invalid repository path");
                    return;
                }

                using var repo = new Repository(selectedFolderPath);
                Branch currentBranch = repo.Head;
                Commit latestCommit = currentBranch.Tip;

                var commits = new List<CommitInfo>();
                foreach (Commit commit in repo.Commits)
                {
                    commits.Add(
                        new CommitInfo(
                            commit.Sha,
                            commit.Author.Name,
                            commit.Author.Email,
                            commit.Author.When.ToString("dd MMM yyyy HH:mm"),
                            commit.Message.Trim()
                        )
                    );
                }
                Commits = new ObservableCollection<CommitInfo>(commits);

                RepositoryPath = selectedFolderPath;
                CurrentBranchText = $"Current Branch: {currentBranch.FriendlyName}";
                LatestCommitText = $"Latest Commit: {latestCommit.Message.Trim()}";
                LatestCommitDateText = $"Latest Commit Date: {latestCommit.Author.When}";
                ActionButtonsBorderVisible = true;
                CommitsDataGridVisible = true;
            }
            catch (Exception ex)
            {
                await AlertBox(ex.Message);
            }
        }
    }

    /** <summary>사용자에게 알림 메시지를 표출한다.</summary> */
    public static async Task<ButtonResult> AlertBox(string message, string title = "Alert")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message);
        return await box.ShowAsync();
    }

    /** <summary>사용자에게 확인 메시지를 표출한다.</summary> */
    public static async Task<ButtonResult> ConfirmBox(string message, string title = "Confirm")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel);
        return await box.ShowAsync();
    }
}

/** <summary>커밋 정보</summary> */
public class CommitInfo(string sha, string authorName, string authorEmail, string authorWhen, string message)
{
    public string Sha { get; set; } = sha;
    public string AuthorName { get; set; } = authorName;
    public string AuthorEmail { get; set; } = authorEmail;
    public string AuthorWhen { get; set; } = authorWhen;
    public string Message { get; set; } = message;
}