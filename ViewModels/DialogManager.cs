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
	/// <param name="message">알림 메시지</param>
	/// <param name="title">알림 제목</param>
	public static async Task<ButtonResult> Alert(string message, string title = "Alert")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message).ShowAsync();

	/// <summary>
	/// 사용자에게 확인 메시지를 표출한다.
	/// </summary>
	/// <param name="message">알림 메시지</param>
	/// <param name="title">알림 제목</param>
	public static async Task<ButtonResult> Confirm(string message, string title = "Confirm")
		=> await MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel).ShowAsync();

	/// <summary>
	/// 사용자에게 정보 메시지를 표출한다.
	/// </summary>
	/// <param name="message">알림 메시지</param>
	/// <param name="title">알림 제목</param>
	public static async Task Info(string message, string title = "Infomation")
		=> await Alert(message, title);
	#endregion
	
}