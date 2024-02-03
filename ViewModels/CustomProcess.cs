using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyGitClient.ViewModels;

/// <summary>
/// System.Diagnostics.Process의 커스터마이징 클래스.
/// 프로세스 시작 시 사용할 창을 숨겨야 할 때, CustomProcess.Start() 메서드를 사용한다.
/// </summary>
public class CustomProcess
{

    #region Methods
    /// <summary>
    /// 외부 프로세스를 실행한다.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="Arguments"></param>
    /// <returns></returns>
    public static async Task Start(string fileName, string Arguments)
    {
        try
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = fileName;
            cmd.StartInfo.Arguments = Arguments;
            // 프로세스 시작 시 사용할 창을 숨긴다.
            cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            cmd.Start();
        }
        catch (Exception ex)
        {
            await DialogManager.Alert(ex.Message);
        }
    }
    #endregion

}