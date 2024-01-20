using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.IO;
using MyGitClient.ViewModels;

namespace MyGitClient.Views;

public partial class MainWindow : Window
{
    #region Properties
    private readonly MainWindowViewModel _viewModel;
    #endregion

    public MainWindow()
    {
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        InitializeComponent();
        //_viewModel.UpdateRepoInfo(@"C:\Users\dmitr\Desktop\p\repo\selosele-pub");
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
            _viewModel.UpdateRepoInfo(selectedFolderPath);
        }
    }

    /** <summary>버튼을 클릭, 애플리케이션을 종료한다.</summary> */
    private async void ExitMenuItem_Clicked(object sender, RoutedEventArgs e)
    {
        var confirm = await MainWindowViewModel.ConfirmBox("Are you sure want to quit the application?");
        if (confirm.ToString() == "Yes")
        {
            Close();
        }
    }
}