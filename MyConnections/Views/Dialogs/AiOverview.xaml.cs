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
	/// Display AI summary
	/// </summary>
	public partial class AiOverview : FluentWindow
	{
		public string InputText { get; set; }
		public string Message { get; set; }
		public bool DialogResult {get; set; }

		public AiOverview(string title, string message)
		{
			InitializeComponent();
			DataContext = this;
			Title = title;
			txtTitle.Text = title;
			Message = message;
		}

		private void OnButtonCloseClick(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void OnButtonCopyClick(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(Message);
		}
	}
}

