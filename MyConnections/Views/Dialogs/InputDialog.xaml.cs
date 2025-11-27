using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Dialogs
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : ContentDialog
	{
		public InputDialog(ContentPresenter? contentPresenter, string title, string message, string defaultText = "") : base(contentPresenter)
		{
			InitializeComponent();

			var vm = new ViewModels.Dialogs.InputDialogViewModel
			{
				DialogTitle = title,
				Message = message,
				InputText = defaultText
			};

			DataContext = vm;
			PrimaryButtonText = "OK";

			// Focus the textbox after the dialog is shown
			Loaded += (_, _) => InputBox?.Focus();
		}

		// We keep the default behaviour of WPF‑Ui’s ContentDialog
		protected override void OnButtonClick(ContentDialogButton button)
		{
			// no custom behaviour – let the base implementation do its job.
			base.OnButtonClick(button);
		}
	}
}