using System.Windows.Controls;
using ConnectionMgr.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ConnectionMgr.Views.Pages
{
	public partial class FirewallPage : INavigableView<FirewallViewModel>
	{
		public FirewallPage(FirewallViewModel viewModel)
		{
			ViewModel = viewModel;
			DataContext = this;

			InitializeComponent();

			viewModel.RulesRefreshed += () =>
			{
				// Clear UI selection AFTER the ItemsSource has repopulated
				Dispatcher.InvokeAsync(() =>
				{
					ClearFirewallSelection();
				});
			};
		}

		public FirewallViewModel ViewModel { get; }

		public void ClearFirewallSelection()
		{
			FirewallGrid.UnselectAll();
			FirewallGrid.SelectedItem = null;
			FirewallGrid.SelectedItems.Clear();
		}
	}
}