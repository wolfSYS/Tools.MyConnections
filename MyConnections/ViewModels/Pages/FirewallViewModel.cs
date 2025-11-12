using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using ConnectionMgr.Helpers;
using ConnectionMgr.Models;
using ConnectionMgr.Properties;
using WindowsFirewallHelper;
using Wpf.Ui;

namespace ConnectionMgr.ViewModels.Pages
{
	public partial class FirewallViewModel : PagesBaseViewModel
	{
		private bool _isInitialized = false;

		[ObservableProperty]
		private string _hostsFileContent = string.Empty;

		[ObservableProperty]
		private bool _hasChanges = false;



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
			_isInitialized = false;

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
			SetProgressAsync(true);
			ReadHostsFile();
			_isInitialized = true;
			SetProgressAsync(false);
		}

		private void ReadHostsFile()
		{
			try
			{
				HostsFileContent = File.ReadAllText(@"C:\Windows\System32\drivers\etc\hosts");
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "HostFileEditPage::ReadHostsFile");
				ShowError(ex);
			}
		}


		public void Editor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (e.Changes.Any())
				HasChanges = true;
		}

		[RelayCommand]
		private async Task RefreshConnection()
		{
			//await RefreshRulesAsync();
		}
	}
}