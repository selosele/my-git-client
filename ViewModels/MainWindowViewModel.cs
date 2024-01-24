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
        LeftMenuItems = ["파일 상태", "History"];
    }

    #region Fields
    /** <summary>최신 Repository 텍스트</summary> */
    private string _repositoryPath = "Git 저장소 불러오기...";
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

    /** <summary>파일 변경 정보 목록</summary> */
    private ObservableCollection<string>? _changes;
    public ObservableCollection<string>? Changes
    {
        get => _changes;
        set => this.RaiseAndSetIfChanged(ref _changes, value);
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

    /** <summary>Git 저장소 경로를 선택한다.</summary> */
    [Obsolete]
    public async void SelectRepoInfo()
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Git 저장소 폴더 선택",
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
                    await AlertBox("유효하지 않은 저장소 경로입니다.");
                    return;
                }

                // Git 저장소 정보 출력
                UpdateRepoInfo(selectedFolderPath);
            }
            catch (Exception ex)
            {
                await AlertBox(ex.Message);
            }
        }
    }

    /** <summary>Git 저장소 정보를 가져와 화면에 표시한다.</summary> */
    public void UpdateRepoInfo(string repositoryPath)
    {
        using var repo = new Repository(repositoryPath);
        Branch currentBranch = repo.Head;
        Commit latestCommit = currentBranch.Tip;

        RepositoryPath = repositoryPath;
        CurrentBranchText = $"최신 브랜치: {currentBranch.FriendlyName}";
        LatestCommitText = $"최신 커밋: {latestCommit.Message.Trim()}";
        LatestCommitDateText = $"최신 커밋 일자: {latestCommit.Author.When}";
        ActionButtonsBorderVisible = true;
        CommitsDataGridVisible = true;

        // 변경된 파일 목록 출력
        UpdateStatusInfo(repo.RetrieveStatus());

        // 커밋 정보 출력
        UpdateCommitsInfo(repo.Commits);
    }

    /** <summary>커밋 정보 목록을 가져와 화면에 표시한다.</summary> */
    public void UpdateCommitsInfo(IQueryableCommitLog commits)
    {
        var commitList = new List<CommitInfo>();
        foreach (Commit commit in commits)
        {
            commitList.Add(
                new CommitInfo(
                    commit.Sha,
                    commit.Author.Name,
                    commit.Author.Email,
                    commit.Author.When.ToString("dd MMM yyyy HH:mm"),
                    commit.Message.Trim()
                )
            );
        }
        Commits = new ObservableCollection<CommitInfo>(commitList);
    }

    /** <summary>변경된 파일 목록을 가져와 화면에 표시한다.</summary> */
    public void UpdateStatusInfo(RepositoryStatus status)
    {
        var changedList = new List<string>();

        // 수정된 파일
        foreach (var entry in status.Modified) changedList.Add(entry.FilePath);

        // 추가된 파일
        foreach (var entry in status.Added) changedList.Add(entry.FilePath);

        // 삭제된 파일
        foreach (var entry in status.Removed) changedList.Add(entry.FilePath);

        // 추적되지 않은 파일
        foreach (var entry in status.Untracked) changedList.Add(entry.FilePath);
            
        Changes = new ObservableCollection<string>(changedList);
    }

    /** <summary>commit을 수행한다.</summary> */
    public async void Commit()
    {
        await AlertBox("준비 중입니다.");
    }

    /** <summary>pull을 수행한다.</summary> */
    public async void Pull()
    {
        try
        {
            // 원격 저장소 이름
            string remoteName = "origin";

            // 저장소 열기
            using var repo = new Repository(RepositoryPath);

            // Git 사용자 정보 가져오기
            string userName = repo.Config.Get<string>("user.name").Value;
            string userEmail = repo.Config.Get<string>("user.email").Value;

            // 현재 브랜치 가져오기
            Branch currentBranch = repo.Head;

            // 원격 브랜치 가져오기
            Branch remoteBranch = repo.Branches[$"{remoteName}/{currentBranch.FriendlyName}"];

            // 원격 저장소 정보 가져오기
            Remote remote = repo.Network.Remotes[remoteName];

            // pull 수행
            var mergeResult = Commands.Pull(repo, new Signature(userName, userEmail, DateTimeOffset.Now),
                new PullOptions
                {
                    FetchOptions = new FetchOptions()
                    // TODO: GitHub의 private 원격 저장소에서 pull을 받으려면 username, password 입력 필요
                    // FetchOptions = new FetchOptions
                    // {
                    //     CredentialsProvider = (url, usernameFromUrl, types) =>
                    //         new UsernamePasswordCredentials
                    //         {
                    //             Username = "",
                    //             Password = ""
                    //         }
                    // }
                });

            // 원격 저장소의 변경 사항이 없을 경우 사용자에게 메시지를 표출한다.
            if (mergeResult.Status.ToString() == "UpToDate") // 변경 사항 존재 시 "FastForward"
            {
                await AlertBox("원격 저장소의 변경 사항이 없습니다.");
                return;
            }

            UpdateRepoInfo(RepositoryPath);
        }
        catch (Exception ex)
        {
            await AlertBox(ex.Message);
        }
    }

    /** <summary>push를 수행한다.</summary> */
    public async void Push()
    {
        await AlertBox("준비 중입니다.");
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