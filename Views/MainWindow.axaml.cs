using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LibGit2Sharp;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MyGitClient.Views;

public partial class MainWindow : Window
{
    /** <summary>Git Action 버튼 박스</summary> */
    private Border? actionButtonsBorder;
    /** <summary>Git 저장소 선택 버튼</summary> */
    private Button? updateRepoInfoButton;
    /** <summary>최신 브랜치 텍스트 블록</summary> */
    private TextBlock? branchTextBlock;
    /** <summary>최신 커밋 내용 텍스트 블록</summary> */
    private TextBlock? commitTextBlock;
    /** <summary>최신 커밋 날짜 텍스트 블록</summary> */
    private TextBlock? commitDateTextBlock;
    /** <summary>커밋 정보 목록</summary> */
    public ObservableCollection<CommitInfo>? Commits { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        //UpdateRepoInfo(@"C:\Users\dmitr\Desktop\p\repo\selosele-pub");
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        actionButtonsBorder = this.FindControl<Border>("ActionButtonsBorder");
        updateRepoInfoButton = this.FindControl<Button>("UpdateRepoInfoButton");
        branchTextBlock = this.FindControl<TextBlock>("BranchTextBlock");
        commitTextBlock = this.FindControl<TextBlock>("CommitTextBlock");
        commitDateTextBlock = this.FindControl<TextBlock>("CommitDateTextBlock");
    }

    /** <summary>Git 저장소 정보를 가져와 화면에 표시한다.</summary> */
    private async void UpdateRepoInfo(string repositoryPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(repositoryPath) || !Directory.Exists(repositoryPath))
            {
                // 경로가 유효하지 않을 경우 사용자에게 메시지를 표출한다.
                await AlertBox("Invalid repository path");
                return;
            }

            using var repo = new Repository(repositoryPath);
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
                        commit.Author.When.ToString(),
                        commit.Message
                    )
                );
            }
            Commits = new ObservableCollection<CommitInfo>(commits);

            if (updateRepoInfoButton != null) updateRepoInfoButton.Content = repositoryPath;
            if (branchTextBlock != null) branchTextBlock.Text = $"Current Branch: {currentBranch.FriendlyName}";
            if (commitTextBlock != null) commitTextBlock.Text = $"Latest Commit: {latestCommit.Message}";
            if (commitDateTextBlock != null) commitDateTextBlock.Text = $"Latest Commit Date: {latestCommit.Author.When}";
            if (actionButtonsBorder != null) actionButtonsBorder.IsVisible = true;
        }
        catch (Exception ex)
        {
            await AlertBox(ex.Message);
        }
    }

    /** <summary>버튼을 클릭, Git 저장소 정보를 가져와 화면에 표시한다.</summary> */
    private async void UpdateRepoInfoButton_Clicked(object source, RoutedEventArgs e)
    {
        var folderDialog = new OpenFolderDialog
        {
            Title = "Select Git Repository Folder",
            Directory = Directory.GetCurrentDirectory() // 기본 경로 설정
        };

        var selectedFolderPath = await folderDialog.ShowAsync(this);

        if (!string.IsNullOrEmpty(selectedFolderPath))
        {
            UpdateRepoInfo(selectedFolderPath);
        }
    }

    /** <summary>버튼을 클릭, 애플리케이션을 종료한다.</summary> */
    private async void ExitMenuItem_Clicked(object sender, RoutedEventArgs e)
    {
        var confirm = await ConfirmBox("Are you sure want to quit the application?");
        if (confirm.ToString() == "Yes")
        {
            Close();
        }
    }

    /** <summary>사용자에게 알림 메시지를 표출한다.</summary> */
    private static async Task<ButtonResult> AlertBox(string message, string title = "Alert")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message);
        return await box.ShowAsync();
    }

    /** <summary>사용자에게 확인 메시지를 표출한다.</summary> */
    private static async Task<ButtonResult> ConfirmBox(string message, string title = "Confirm")
    {
        var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel);
        return await box.ShowAsync();
    }
}