using System.Windows.Input;
using ConnectionMgr.ViewModels.Dialogs;
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

			// wire the CloseRequested event to actually close the window
			vm.CloseRequested += (_, __) => this.Close();

			DataContext = vm;
		}

		/// <summary>
		/// FluentWindow hides the standard window title bar => make it dragable via mouse
		/// </summary>
		private void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
				this.DragMove();
		}
	}
}