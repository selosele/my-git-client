using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGitClient.ViewModels;

/// <summary>
/// System.Diagnostics.Process의 커스텀 클래스.
/// 외부 프로세스를 시작할 때 창을 숨기고 실행해야 하는 경우 CustomProcess.Start() 메서드를 사용한다.
/// </summary>
public class CustomProcess
{

    #region Methods
    /// <summary>
    /// 외부 프로세스를 실행한다.
    /// </summary>
    /// <param name="fileName">실행할 파일의 경로</param>
    /// <param name="arguments">실행 파일에 전달할 인수</param>
    public static async Task Start(string fileName, string arguments)
    {
        try
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = fileName,
                    Arguments = arguments,
                    // 프로세스 시작 시 창을 숨기도록 설정한다.
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            process.Start();
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }
    #endregion

}