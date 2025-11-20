using System.Windows.Controls;
using ConnectionMgr.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ConnectionMgr.Views.Pages
{
    public partial class FirewallPage : INavigableView<FirewallViewModel>
    {
        public FirewallViewModel ViewModel { get; }

        public FirewallPage(FirewallViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
	}
}
