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

			// Hook up the OK request → close logic
			vm.OkRequested += (_, _) => { DialogResult = true; };

			DataContext = vm;
			PrimaryButtonText = "OK";

			// Focus the textbox after the dialog is shown
			Loaded += (_, _) => InputBox?.Focus();
		}

		public bool DialogResult { get; set; }
		public string InputText { get; set; }
		public string Message { get; set; }

		// We keep the default behaviour of WPF‑Ui’s ContentDialog
		protected override void OnButtonClick(ContentDialogButton button)
		{
			// No custom behaviour – let the base implementation do its job.
			base.OnButtonClick(button);
		}
	}
}