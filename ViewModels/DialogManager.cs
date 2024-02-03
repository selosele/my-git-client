using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace MyGitClient.ViewModels;

public class DialogManager
{
	
	#region Methods
	/// <summary>
	/// 사용자에게 알림 메시지를 표출한다.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="title"></param>
	/// <returns></returns>
	public static async Task<ButtonResult> Alert(string message, string title = "Alert")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();

	/// <summary>
	/// 사용자에게 확인 메시지를 표출한다.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="title"></param>
	/// <returns></returns>
	public static async Task<ButtonResult> Confirm(string message, string title = "Confirm")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel).ShowAsync();
	#endregion
	
}