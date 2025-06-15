using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using System;
using Microsoft.Win32;
using MyGitClient.Models;

namespace MyGitClient;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Windows 컨텍스트 메뉴 클릭으로 실행 시
        // 예: args[0] = "C:\\Users\\me\\Documents\\MyRepo"
        if (args.Length > 0)
        {
            var selectedPath = args[0];
            AppState.SelectedPath = selectedPath;
        }

        RegisterContextMenu();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            //.WithInterFont() // TODO: 외부 폰트 사용 시, 주석 처리 필요
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "avares://MyGitClient/Assets/Fonts#Noto Sans KR",
                FontFallbacks =
                [
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://MyGitClient/Assets/Fonts#Noto Sans KR")
                    }
                ]
            });

    // Avalonia 애플리케이션이 시작될 때 레지스트리에 컨텍스트 메뉴를 등록한다.
    private static void RegisterContextMenu()
    {
        if (!OperatingSystem.IsWindows())
            return;

        try
        {
            string baseKey = @"Software\Classes\Directory\shell\MyGitClientHere";

            using (var key = Registry.CurrentUser.CreateSubKey(baseKey))
            {
                key?.SetValue(null, "Open with MyGitClient");
            }

            using (var key = Registry.CurrentUser.CreateSubKey(baseKey + @"\command"))
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                key?.SetValue(null, $"\"{exePath}\" \"%V\"");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"레지스트리 등록 실패: {ex.Message}");
        }
    }

}
