using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGitClient.ViewModels;

/// <summary>
/// System.Diagnostics.Process의 커스텀 클래스.
/// 프로세스 시작 시 사용할 창을 숨겨야 할 때, CustomProcess.Start() 메서드를 사용한다.
/// </summary>
public class CustomProcess
{

    #region Methods
    /// <summary>
    /// 외부 프로세스를 실행한다.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static async Task Start(string fileName, string arguments)
    {
        try
        {
            var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            // 프로세스 시작 시 사용할 창을 숨긴다.
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }
    #endregion

}