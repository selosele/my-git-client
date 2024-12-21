using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MyGitClient.ViewModels;

namespace MyGitClient.Views;

public partial class MainWindow : Window
{

    #region [생성자]
    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;
        Instance = this;
    }
    #endregion
    
    #region [필드]
    /// <summary>
    /// 메인 윈도우 인스턴스
    /// </summary>
    public static MainWindow? Instance;

    /// <summary>
    /// 뷰모델 인스턴스
    /// </summary>
    private readonly MainWindowViewModel _viewModel;
    
    /// <summary>
    /// Status 영역 StackPanel
    /// </summary>
    private StackPanel? _statusStackPanel;
    
    /// <summary>
    /// History 영역 StackPanel
    /// </summary>
    private StackPanel? _historyStackPanel;
    #endregion

    #region [메서드]
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        _statusStackPanel = this.Find<StackPanel>("StatusStackPanel");
        _historyStackPanel = this.Find<StackPanel>("HistoryStackPanel");
    }

    /// <summary>
    /// 버튼을 클릭, 애플리케이션을 종료한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private async void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var confirm = await DialogManager.Confirm("프로그램을 종료하시겠습니까?");
        if (confirm == ButtonResult.Yes)
        {
            Close();
        }
    }

    /// <summary>
    /// 레프트 메뉴 아이템을 클릭한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private void LeftListBox_SelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            var selectedItem = (string?)e.AddedItems[0];
            switch (selectedItem)
            {
                case "파일 상태":
                    _statusStackPanel!.IsVisible = true;
                    _historyStackPanel!.IsVisible = false;
                    break;
                case "History":
                    _statusStackPanel!.IsVisible = false;
                    _historyStackPanel!.IsVisible = true;
                    break;
            }
        }
    }

    /// <summary>
    /// "이 애플리케이션에 대해서" 메뉴 아이템을 클릭한다.
    /// </summary>
    /// <param name="sender">이벤트를 발생시킨 객체</param>
    /// <param name="e">이벤트 인수</param>
    private async void AboutAppMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var message = string.Join(Environment.NewLine,
            "이 애플리케이션은 .NET(Avalonia)로 개발되었습니다.",
            "Copyright selosele 2024 All Rights Reserved.",
            "버전 0.0.1"
        );
        await DialogManager.Info(message);
    }
    #endregion

}