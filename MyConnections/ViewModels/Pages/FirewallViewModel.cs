using System.Diagnostics;
using System.IO;
using MyConnections.Properties;
using Windows.Storage;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace MyConnections.ViewModels.Pages
{
    public partial class FirewallViewModel : PagesBaseViewModel
	{
        private bool _isInitialized = false;

		public FirewallViewModel(
					Interfaces.ILoggerService logger,
					IContentDialogService dialogService,
					ISnackbarService snackbarService)
					: base(logger, dialogService, snackbarService)
		{
			// BaseVM will care about DY
		}

		public override Task OnNavigatedFromAsync()
		{
			return Task.CompletedTask;
		}

		public override Task OnNavigatedToAsync()
		{
			if (!_isInitialized)
				InitializeViewModel();

			return Task.CompletedTask;
		}

		private void InitializeViewModel()
        {
            _isInitialized = true;
		}

    }
}
