using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Dialogs
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : ContentDialog
	{
		public string InputText { get; set; }
		public string Message { get; set; }
		public bool DialogResult {get; set; }

		public InputDialog(ContentPresenter? contentPresenter, string title, string message, string defaultText = "") : base(contentPresenter)
		{
			InitializeComponent();
			//DataContext = this;
			//Title = title;
			//Message = message;
			//InputText = defaultText;
			//InputBox.Text = defaultText;
			//InputBox.Focus();
			//PrimaryButtonText = "OK";
			var vm = new ViewModels.Dialogs.InputDialogViewModel
			{
				DialogTitle = title,
				Message = message,
				InputText = defaultText
			};

			// Hook up the OK request → close logic
			vm.OkRequested += (_, _) =>
			{
				DialogResult = true;
				//this.Close();
			};

			DataContext = vm;
			PrimaryButtonText = "OK";

			// Focus the textbox after the dialog is shown
			Loaded += (_, _) => InputBox?.Focus();
		}

		// We keep the default behaviour of WPF‑Ui’s ContentDialog
		protected override void OnButtonClick(ContentDialogButton button)
		{
			// No custom behaviour – let the base implementation do its job.
			base.OnButtonClick(button);
		}
	}
}

