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
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Dialogs
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class AiOverview : ContentDialog
	{
		public string InputText { get; set; }
		public string Message { get; set; }
		public bool DialogResult {get; set; }

		public AiOverview(ContentPresenter? contentPresenter, string title, string message) : base(contentPresenter)
		{
			InitializeComponent();
			DataContext = this;
			Title = title;
			Message = message;
		}

		protected override void OnButtonClick(ContentDialogButton button)
		{
			base.OnButtonClick(button);
			return;
		}
	}
}

