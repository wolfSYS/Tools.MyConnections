using ConnectionMgr.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ConnectionMgr.Views.Pages
{
    public partial class HostsFileEditPage : INavigableView<HostsFileEditViewModel>
    {
        public HostsFileEditViewModel ViewModel { get; }

        public HostsFileEditPage(HostsFileEditViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

    }
}
