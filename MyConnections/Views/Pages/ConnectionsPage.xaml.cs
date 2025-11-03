using MyConnections.Services;
using MyConnections.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace MyConnections.Views.Pages
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
