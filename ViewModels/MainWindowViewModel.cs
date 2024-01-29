using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using LibGit2Sharp;
using Avalonia.Controls;
using ReactiveUI;
using System.Linq;
using System.Reactive;
using MyGitClient.Models;

namespace MyGitClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        //UpdateRepoInfo(@"D:\workspace\MyGitClient");
        LeftMenuItems = ["파일 상태", "History"];
        
        StageFileCommand = ReactiveCommand.Create<string>(StageFile);
        UnstageFileCommand = ReactiveCommand.Create<string>(UnstageFile);
    }

    #region Fields
    /** <summary>스테이지에 파일을 올리는 Command 인스턴스</summary> */
    public ReactiveCommand<string, Unit> StageFileCommand { get; }
    /** <summary>스테이지에서 파일을 제거하는 Command 인스턴스</summary> */
    public ReactiveCommand<string, Unit> UnstageFileCommand { get; }

    /** <summary>FileSystemWatcher 인스턴스</summary> */
    private readonly FileSystemWatcher _watcher = new();

    /** <summary>FileSystemWatcher의 파일 변경 감지에 제외할 디렉터리들</summary> */
    private readonly List<string> _excludedWatchPath = [];

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

    /** <summary>스테이지에 올라간 파일 목록</summary> */
    private ObservableCollection<string>? _staged;
    public ObservableCollection<string>? Staged
    {
        get => _staged;
        set => this.RaiseAndSetIfChanged(ref _staged, value);
    }

    /** <summary>스테이지에 올라가지 않은 파일 목록</summary> */
    private ObservableCollection<string>? _unstaged;
    public ObservableCollection<string>? Unstaged
    {
        get => _unstaged;
        set => this.RaiseAndSetIfChanged(ref _unstaged, value);
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
    public async Task SelectRepoInfo()
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
                    await DialogManager.Alert("유효하지 않은 저장소 경로입니다.");
                    return;
                }

                // Git 저장소 정보 출력
                UpdateRepoInfo(selectedFolderPath);

                // Git 로컬 저장소 파일의 변경을 실시간으로 감지
                FireWatchFileChanges();
            }
            catch (Exception ex)
            {
                await DialogManager.Alert(ex.Message);
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
        var stagedList = new List<string>();
        var unstagedList = new List<string>();

        // 수정된 파일
        foreach (var entry in status.Modified) unstagedList.Add(entry.FilePath);

        // 추가된 파일
        foreach (var entry in status.Added) unstagedList.Add(entry.FilePath);

        // 삭제된 파일
        foreach (var entry in status.Removed) unstagedList.Add(entry.FilePath);

        // 추적되지 않은 파일
        foreach (var entry in status.Untracked) unstagedList.Add(entry.FilePath);

        // 스테이지에 올라간 파일
        foreach (var entry in status.Staged) stagedList.Add(entry.FilePath);
            
        // 스테이지에 올라간 파일 목록
        Staged = new ObservableCollection<string>(stagedList);

        // 스테이지에 올라가지 않은 파일 목록
        Unstaged = new ObservableCollection<string>(unstagedList);
    }

    /** <summary>스테이지에 파일을 올린다.</summary> */
    private void StageFile(string filePath)
    {
        // 저장소 열기
        using var repo = new Repository(RepositoryPath);

        // git add 수행
        Commands.Stage(repo, filePath);

        Unstaged?.Remove(filePath);
        Staged?.Add(filePath);

        // 변경된 파일 목록 출력
        //UpdateStatusInfo(repo.RetrieveStatus());
    }

    /** <summary>스테이지에서 파일을 제거한다.</summary> */
    private void UnstageFile(string filePath)
    {
        // 저장소 열기
        using var repo = new Repository(RepositoryPath);

        // git reset HEAD [파일경로] 수행
        Commands.Unstage(repo, filePath);

        Staged?.Remove(filePath);
        Unstaged?.Add(filePath);

        // 변경된 파일 목록 출력
        //UpdateStatusInfo(repo.RetrieveStatus());
    }

    /** <summary>commit을 수행한다.</summary> */
    public async Task Commit()
    {
        // 스테이지에 올라간 파일이 없으면 커밋을 수행하지 않는다.
        if (Staged?.Count == 0)
        {
            await DialogManager.Alert("커밋할 파일이 존재하지 않습니다.");
            return;
        }
    }

    /** <summary>pull을 수행한다.</summary> */
    public async Task Pull()
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
                await DialogManager.Alert("원격 저장소의 변경 사항이 없습니다.");
                return;
            }

            // Git 저장소 정보 출력
            UpdateRepoInfo(RepositoryPath);
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }

    /** <summary>push를 수행한다.</summary> */
    public async Task Push()
    {
        await DialogManager.Alert("준비 중입니다.");
    }

    /** <summary>Git 저장소 정보를 새로고침하여 출력한다.</summary> */
    public void Refresh()
    {
        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /** <summary>Git 로컬 저장소 파일의 변경을 실시간으로 감지한다.</summary> */
    private void FireWatchFileChanges()
    {
        // .gitignore 파일 읽기
        ReadGitIgnoreFile();

        // 파일 변경을 실시간으로 감지
        WatchFileChanges(RepositoryPath);
    }

    /** <summary>파일의 변경을 실시간으로 감지한다.</summary> */
    private void WatchFileChanges(string repositoryPath)
    {
        // FileSystemWatcher 생성
        _watcher.Path = repositoryPath;
        _watcher.EnableRaisingEvents = true;
        _watcher.IncludeSubdirectories = true;
        _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

        // 파일 변경 이벤트 핸들러 등록
        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileRenamed;
    }

    /** <summary>파일의 변경을 감지한다.</summary> */
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (IsExcludedPath(e.FullPath))
            return;
            
        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /** <summary>파일명의 변경을 감지한다.</summary> */
    private void OnFileRenamed(object source, RenamedEventArgs e)
    {
        if (IsExcludedPath(e.FullPath))
            return;

        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /** <summary>파일 변경 감지 시, 감지에 제외한 디렉터리 발견 여부를 반환한다.</summary> */
    private bool IsExcludedPath(string fullPath)
    {
        foreach (var path in _excludedWatchPath)
        {
            if (fullPath.Contains(path))
                return true;
        }
        return false;
    }

    /** <summary>.gitignore 파일을 읽어 배열에 담는다.</summary> */
    private void ReadGitIgnoreFile()
    {
        _excludedWatchPath.Clear();
        
        string filePath = Path.Combine(RepositoryPath, ".gitignore");

        // .gitignore 파일이 없으면 .git 폴더만 제외한다.
        if (!File.Exists(filePath))
        {
            _excludedWatchPath.Add(@"\.git");
            return;
        }
            
        // 파일의 모든 줄을 읽어온다.
        string[] lines = File.ReadAllLines(filePath);

        // 주석 또는 빈 줄을 제외하고 유효한 경로를 리스트에 추가한다.
        foreach (string line in lines.Where(l => !string.IsNullOrWhiteSpace(l) && !l.Trim().StartsWith("#")))
        {
            string sanitizedPath = line.Trim().Replace("/", "\\");
            _excludedWatchPath.Add(sanitizedPath);
        }
    }
}