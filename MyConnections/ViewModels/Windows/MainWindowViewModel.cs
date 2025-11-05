using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace MyConnections.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "wolfSYS.Tools.MyConnections";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Connections",
                Icon = new SymbolIcon { Symbol = SymbolRegular.ServerSurfaceMultiple16 },
                TargetPageType = typeof(Views.Pages.ConnectionsPage)
            },
			new NavigationViewItem()
			{
				Content = "Firewall",
				Icon = new SymbolIcon { Symbol = SymbolRegular.Fire24 },
				TargetPageType = typeof(Views.Pages.FirewallPage)
			}
		};

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
		};

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Connections", Tag = "tray_home" }
        };
    }
}
