using System.Threading.Tasks;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace MyGitClient.ViewModels
{
	public class DialogManager
	{
		/** <summary>사용자에게 알림 메시지를 표출한다.</summary> */
		public static async Task<ButtonResult> Alert(string message, string title = "Alert")
		{
			var box = MessageBoxManager.GetMessageBoxStandard(title, message);
			return await box.ShowAsync();
		}

		/** <summary>사용자에게 확인 메시지를 표출한다.</summary> */
		public static async Task<ButtonResult> Confirm(string message, string title = "Confirm")
		{
			var box = MessageBoxManager.GetMessageBoxStandard(title, message, ButtonEnum.YesNoCancel);
			return await box.ShowAsync();
		}
	}
}
