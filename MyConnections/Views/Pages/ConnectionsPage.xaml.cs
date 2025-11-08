using ConnectionMgr.Services;
using ConnectionMgr.ViewModels.Pages;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;

namespace ConnectionMgr.Views.Pages
{
    public partial class ConnectionsPage : INavigableView<ConnectionsViewModel>
    {
        
        public ConnectionsViewModel ViewModel { get; }

        public ConnectionsPage(ConnectionsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
		}
    }
}
