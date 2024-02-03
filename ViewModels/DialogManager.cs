using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace MyGitClient.ViewModels;

public class DialogManager
{
	/// <summary>
  	/// 사용자에게 알림 메시지를 표출한다.
	/// </summary>
	public static async Task<ButtonResult> Alert(string message, string title = "Alert")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();

	/// <summary>
  	/// 사용자에게 확인 메시지를 표출한다.
	/// </summary>
	public static async Task<ButtonResult> Confirm(string message, string title = "Confirm")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel).ShowAsync();
}