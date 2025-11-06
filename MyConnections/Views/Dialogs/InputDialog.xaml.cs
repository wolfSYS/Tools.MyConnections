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

namespace MyConnections.Views.Dialogs
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
			DataContext = this;
			Title = title;
			Message = message;
			InputText = defaultText;
			InputBox.Text = defaultText;
			InputBox.Focus();
			PrimaryButtonText = "OK";
		}

		protected override void OnButtonClick(ContentDialogButton button)
		{
			base.OnButtonClick(button);
			return;
		}
	}
}

