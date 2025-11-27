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
using ConnectionMgr.ViewModels.Dialogs;
using ConnectionMgr.ViewModels.Pages;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Dialogs
{
	/// <summary>
	/// Display AI summary
	/// </summary>
	public partial class AiOverview : FluentWindow
	{
		public AiOverview(string title, string message)
		{
			InitializeComponent();

			var vm = new AiOverviewViewModel
			{
				DialogTitle = title,
				Message = message
			};

			// 2️ Wire the CloseRequested event to actually close the window
			vm.CloseRequested += (_, __) => this.Close();

			DataContext = vm;
		}

		private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}