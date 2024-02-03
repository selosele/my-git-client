using Avalonia;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using System;

namespace MyGitClient;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI()
            //.WithInterFont() // TODO: 외부 폰트 사용 시, 주석 처리 필요
            .With(new FontManagerOptions{
                DefaultFamilyName = "avares://MyGitClient/Assets/Fonts#Noto Sans KR",
                FontFallbacks =
                [
                    new FontFallback
                    {
                        FontFamily = new FontFamily("avares://MyGitClient/Assets/Fonts#Noto Sans KR")
                    }
                ]
            });
}
