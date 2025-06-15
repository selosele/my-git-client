using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI;
using LibGit2Sharp;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using MyGitClient.Models;

namespace MyGitClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    
    #region [생성자]
    public MainWindowViewModel()
    {
        LeftMenuItems = ["파일 상태", "History"];

        StagedSelection = new SelectionModel<string>
        {
            SingleSelect = false
        };
        StagedSelection.SelectionChanged += OnStagedSelectionChanged;

        StageFileCommand = ReactiveCommand.Create<string>(StageFile);
        UnstageFileCommand = ReactiveCommand.Create<string>(UnstageFile);

        // Windows 컨텍스트 메뉴 클릭으로 실행 시
        if (!string.IsNullOrEmpty(AppState.SelectedPath))
        {
            InitRepoInfo(AppState.SelectedPath);
        }
    }
    #endregion

    #region [필드]
    /// <summary>
    /// 스테이지에 올라간 파일의 Selection 인스턴스
    /// </summary>
    public ISelectionModel StagedSelection { get; }

    /// <summary>
    /// 스테이지에 파일을 올리는 Command 인스턴스
    /// </summary>
    public ReactiveCommand<string, Unit> StageFileCommand { get; }

    /// <summary>
    /// 스테이지에서 파일을 제거하는 Command 인스턴스
    /// </summary>
    public ReactiveCommand<string, Unit> UnstageFileCommand { get; }

    /// <summary>
    /// FileSystemWatcher 인스턴스
    /// </summary>
    private readonly FileSystemWatcher _watcher = new();

    /// <summary>
    /// FileSystemWatcher의 파일 변경 감지에 제외할 디렉터리들
    /// </summary>
    private readonly List<string> _excludedWatchPath = [];

    /// <summary>
    /// 최신 Git 저장소 경로
    /// </summary>
    private string _repositoryPath = "Git 저장소 불러오기...";
    public string RepositoryPath
    {
        get => _repositoryPath;
        set => this.RaiseAndSetIfChanged(ref _repositoryPath, value);
    }

    /// <summary>
    /// 최신 브랜치 텍스트
    /// </summary>
    private string? _currentBranchText;
    public string? CurrentBranchText
    {
        get => _currentBranchText;
        set => this.RaiseAndSetIfChanged(ref _currentBranchText, value);
    }

    /// <summary>
    /// 최신 커밋 텍스트
    /// </summary>
    private string? _latestCommitText;
    public string? LatestCommitText
    {
        get => _latestCommitText;
        set => this.RaiseAndSetIfChanged(ref _latestCommitText, value);
    }

    /// <summary>
    /// 최신 커밋 일시 텍스트
    /// </summary>
    private string? _latestCommitDateText;
    public string? LatestCommitDateText
    {
        get => _latestCommitDateText;
        set => this.RaiseAndSetIfChanged(ref _latestCommitDateText, value);
    }

    /// <summary>
    /// 커밋 메시지 텍스트
    /// </summary>
    private string? _commitMessageText;
    public string? CommitMessageText
    {
        get => _commitMessageText;
        set => this.RaiseAndSetIfChanged(ref _commitMessageText, value);
    }

    /// <summary>
    /// 액션버튼 Border 표시 여부
    /// </summary>
    private bool? _actionButtonsBorderVisible = false;
    public bool? ActionButtonsBorderVisible
    {
        get => _actionButtonsBorderVisible;
        set => this.RaiseAndSetIfChanged(ref _actionButtonsBorderVisible, value);
    }

    /// <summary>
    /// 커밋 정보 데이타그리드 표시 여부
    /// </summary>
    private bool? _commitsDataGridVisible = false;
    public bool? CommitsDataGridVisible
    {
        get => _commitsDataGridVisible;
        set => this.RaiseAndSetIfChanged(ref _commitsDataGridVisible, value);
    }

    /// <summary>
    /// 스테이지에 올라간 파일 목록
    /// </summary>
    private ObservableCollection<string>? _staged;
    public ObservableCollection<string>? Staged
    {
        get => _staged;
        set => this.RaiseAndSetIfChanged(ref _staged, value);
    }

    /// <summary>
    /// 스테이지에 올라간 선택된 파일
    /// </summary>
    private ObservableCollection<string>? _selectedStaged;
    public ObservableCollection<string>? SelectionStaged
    {
        get => _selectedStaged;
        set => this.RaiseAndSetIfChanged(ref _selectedStaged, value);
    }

    /// <summary>
    /// 스테이지에 올라가지 않은 파일 목록
    /// </summary>
    private ObservableCollection<string>? _unstaged;
    public ObservableCollection<string>? Unstaged
    {
        get => _unstaged;
        set => this.RaiseAndSetIfChanged(ref _unstaged, value);
    }

    /// <summary>
    /// 커밋 정보 목록
    /// </summary>
    private ObservableCollection<CommitInfo>? _commits;
    public ObservableCollection<CommitInfo>? Commits
    {
        get => _commits;
        set => this.RaiseAndSetIfChanged(ref _commits, value);
    }

    /// <summary>
    /// Git 사용자 정보
    /// </summary>
    private string? _authorInfoText;
    public string? AuthorInfoText
    {
        get => _authorInfoText;
        set => this.RaiseAndSetIfChanged(ref _authorInfoText, value);
    }

    /// <summary>
    /// 레프트 메뉴 아이템
    /// </summary>
    public ObservableCollection<string>? LeftMenuItems { get; set; }
    #endregion

    #region [메서드]
    /// <summary>
    /// 프로그램 실행 직후 Git 저장소 정보를 불러온다.
    /// </summary>
    /// <param name="repositoryPath">Git 저장소 경로</param>
    public async void InitRepoInfo(string repositoryPath)
    {
        try
        {
            // Git 저장소 정보 출력
            UpdateRepoInfo(repositoryPath);

            // Git 로컬 저장소 파일의 변경을 실시간으로 감지
            FireWatchFileChanges();
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }

    /// <summary>
    /// Git 저장소 경로를 선택한다.
    /// </summary>
    [Obsolete]
    public async Task SelectRepoInfo()
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Git 저장소 폴더 선택",
            Directory = Directory.GetCurrentDirectory() // 기본 경로 설정
        };

        var selectedFolderPath = await folderDialog.ShowAsync(Views.MainWindow.Instance!);

        if (string.IsNullOrWhiteSpace(selectedFolderPath))
            return;

        if (!Directory.Exists(selectedFolderPath))
        {
            // 경로가 유효하지 않을 경우 사용자에게 메시지를 표출한다.
            await DialogManager.Alert("유효하지 않은 저장소 경로입니다.");
            return;
        }

        try
        {
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

    /// <summary>
    /// Git 저장소 정보를 가져와 화면에 표시한다.
    /// </summary>
    /// <param name="repositoryPath">Git 저장소 경로</param>
    public void UpdateRepoInfo(string repositoryPath)
    {
        using var repo = new Repository(repositoryPath);
        var currentBranch = repo.Head;
        var latestCommit = currentBranch.Tip;

        // Git 사용자 정보 가져오기
        var authorInfo = GetAuthorInfo(repo.Config);

        AuthorInfoText = $"{authorInfo.UserName} <{authorInfo.UserEmail}>";
        RepositoryPath = repositoryPath;
        CurrentBranchText = $"최신 브랜치: {currentBranch.FriendlyName}";
        LatestCommitText = $"최신 커밋: {latestCommit.Message.Trim()}";
        LatestCommitDateText = $"최신 커밋 일자: {latestCommit.Author.When}";
        ActionButtonsBorderVisible = true;
        CommitsDataGridVisible = true;
        
        CommitMessageText = "";
        SelectionStaged?.Clear();

        // 변경된 파일 목록 출력
        UpdateStatusInfo(repo.RetrieveStatus());

        // 커밋 정보 출력
        UpdateCommitsInfo(repo.Commits);
    }

    /// <summary>
    /// 커밋 정보 목록을 가져와 화면에 표시한다.
    /// </summary>
    /// <param name="commits">커밋 정보 목록</param>
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
                    commit.Author.When.ToString("yyyy-MM-dd HH:mm:ss"),
                    commit.Message.Trim()
                )
            );
        }
        Commits = [..commitList];
    }

    /// <summary>
    /// 변경된 파일 목록을 가져와 화면에 표시한다.
    /// </summary>
    /// <param name="status">파일 상태</param>
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

    /// <summary>
    /// commit을 수행한다.
    /// </summary>
    public async Task Commit()
    {
        // 스테이지에 올라간 파일이 없으면 커밋을 수행하지 않는다.
        if (Staged?.Count == 0)
        {
            await DialogManager.Alert("커밋할 파일이 존재하지 않습니다.");
            return;
        }

        // 입력된 커밋 메시지가 없으면 커밋을 수행하지 않는다.
        if (string.IsNullOrWhiteSpace(CommitMessageText))
        {
            await DialogManager.Alert("커밋 메시지를 입력하세요.");
            return;
        }

        using var repo = new Repository(RepositoryPath);

        // Git 사용자 정보 가져오기
        var authorInfo = GetAuthorInfo(repo.Config);
        var signature = new Signature(authorInfo.UserName, authorInfo.UserEmail, DateTimeOffset.Now);

        // git commit 수행
        var commit = repo.Commit(CommitMessageText, signature, signature);
    }

    /// <summary>
    /// pull을 수행한다.
    /// </summary>
    public async Task Pull()
    {
        try
        {
            // 원격 저장소 이름
            var remoteName = "origin";

            // 저장소 열기
            using var repo = new Repository(RepositoryPath);

            // 현재 브랜치 가져오기
            var currentBranch = repo.Head;

            // 원격 브랜치 가져오기
            var remoteBranch = repo.Branches[$"{remoteName}/{currentBranch.FriendlyName}"];

            // 원격 저장소 정보 가져오기
            var remote = repo.Network.Remotes[remoteName];

            // Git 사용자 정보 가져오기
            var authorInfo = GetAuthorInfo(repo.Config);

            // git pull 수행
            var mergeResult = Commands.Pull(repo, new Signature(authorInfo.UserName, authorInfo.UserEmail, DateTimeOffset.Now),
                new PullOptions
                {
                    FetchOptions = new FetchOptions()
                    // TODO: GitHub의 private 원격 저장소에서 pull을 받으려면 username, password 입력 필요
                    // FetchOptions = new FetchOptions
                    // {
                    //     CredentialsProvider = (url, usernameFromUrl, types) =>
                    //         new UsernamePasswordCredentials
                    //         {
                    //             Username = "personal access token",
                    //             Password = string.Empty
                    //         }
                    // }
                });

            // 원격 저장소의 변경 사항이 없을 경우 사용자에게 메시지를 표출한다.
            // 변경 사항 없을 시 "UpToDate"
            // 변경 사항 존재 시 "FastForward"
            if (mergeResult.Status.ToString() == Models.MergeStatus.UpToDate.ToString())
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

    /// <summary>
    /// push를 수행한다.
    /// </summary>
    public async Task Push()
    {
        // 원격 저장소 이름
        var remoteName = "origin";

        // 저장소 열기
        using var repo = new Repository(RepositoryPath);

        // 원격 저장소 가져오기
        var remote = repo.Network.Remotes[remoteName];

        // 현재 브랜치 가져오기
        var currentBranch = repo.Head;
        var branchName = currentBranch.FriendlyName;

        // git push 수행
        // TODO: LibGit2Sharp을 활용한 push는 자격 증명을 필요로 해서 임시방편으로 cmd 실행 로직을 사용
        // repo.Network.Push(remote, $"refs/heads/{branchName}",
        //     new PushOptions
        //     {
        //         CredentialsProvider = (url, usernameFromUrl, types) =>
        //             new UsernamePasswordCredentials
        //             {
        //                 Username = "personal access token",
        //                 Password = string.Empty
        //             }
        //     });
        try
        {
            await CustomProcess.Start("git", $"push {remoteName} {branchName}:{branchName}");
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }

    /// <summary>
    /// commit을 취소한다.
    /// </summary>
    public void Reset()
    {
        using var repo = new Repository(RepositoryPath);
        
        var trackedBranch = repo.Head.TrackedBranch;
        var originHeadCommit = repo.ObjectDatabase.FindMergeBase(repo.Branches[trackedBranch.FriendlyName].Tip, repo.Head.Tip);
        
        // git reset 수행
        repo.Reset(ResetMode.Soft, originHeadCommit);
    }

    /// <summary>
    /// Git 저장소 정보를 새로고침하여 출력한다.
    /// </summary>
    public void Refresh()
    {
        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /// <summary>
    /// 최신 Git 저장소 경로를 탐색기로 연다.
    /// </summary>
    public async Task OpenFileExplorer()
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = RepositoryPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }

    /// <summary>
    /// 스테이지에 파일을 올린다.
    /// </summary>
    /// <param name="filePath">파일 경로를 포함한 파일명</param>
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

    /// <summary>
    /// 스테이지에서 파일을 제거한다.
    /// </summary>
    /// <param name="filePath">파일 경로를 포함한 파일명</param>
    private void UnstageFile(string filePath)
    {
        // 저장소 열기
        using var repo = new Repository(RepositoryPath);

        // git reset HEAD [파일경로] 수행
        Commands.Unstage(repo, filePath);

        Staged?.Remove(filePath);
        Unstaged?.Add(filePath);
        SelectionStaged?.Remove(filePath);

        // 변경된 파일 목록 출력
        //UpdateStatusInfo(repo.RetrieveStatus());
    }

    /// <summary>
    /// Git 로컬 저장소 파일의 변경을 실시간으로 감지한다.
    /// </summary>
    private void FireWatchFileChanges()
    {
        // .gitignore 파일 읽기
        ReadGitIgnoreFile();

        // 파일 변경을 실시간으로 감지
        WatchFileChanges(RepositoryPath);
    }

    /// <summary>
    /// 파일의 변경을 실시간으로 감지한다.
    /// </summary>
    /// <param name="repositoryPath">Git 저장소 경로</param>
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

    /// <summary>
    /// 파일의 변경을 감지한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (IsExcludedPath(e.FullPath)) return;

        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /// <summary>
    /// 파일명의 변경을 감지한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (IsExcludedPath(e.FullPath)) return;

        // Git 저장소 정보 출력
        UpdateRepoInfo(RepositoryPath);
    }

    /// <summary>
    /// 파일 변경 감지 시, 감지에 제외한 디렉터리 발견 여부를 반환한다.
    /// </summary>
    /// <param name="fullPath">파일 경로를 포함한 파일명</param>
    private bool IsExcludedPath(string fullPath)
    {
        foreach (var path in _excludedWatchPath)
        {
            if (fullPath.Contains(path)) return true;
        }
        return false;
    }

    /// <summary>
    /// .gitignore 파일을 읽어 배열에 담는다.
    /// </summary>
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

    /// <summary>
    /// Git 사용자 정보를 반환한다.
    /// </summary>
    /// <param name="repoConfig">Git 저장소 설정</param>
    private static AuthorInfo GetAuthorInfo(Configuration repoConfig)
    {
        // Git 사용자 정보 가져오기
        var userName = repoConfig.Get<string>("user.name").Value;
        var userEmail = repoConfig.Get<string>("user.email").Value;

        return new AuthorInfo(userName, userEmail);
    }

    /// <summary>
    /// 스테이지에 올라간 파일을 선택한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private void OnStagedSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        if (sender == null) return;

        foreach (var item in e.SelectedItems)
        {
            SelectionStaged?.Add(item?.ToString()!);
        }
    }
    #endregion

}