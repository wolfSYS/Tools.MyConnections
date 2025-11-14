using ConnectionMgr.ViewModels.Pages;
using Wpf.Ui.Abstractions.Controls;

namespace ConnectionMgr.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

		private void CardExpander_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{

        }
    }
}
