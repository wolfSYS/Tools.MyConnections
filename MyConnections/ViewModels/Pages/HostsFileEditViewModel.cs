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
	public partial class HostsFileEditViewModel : PagesBaseViewModel
	{
		private bool _isInitialized = false;

		[ObservableProperty]
		private string _hostsFileContent = string.Empty;



		public HostsFileEditViewModel(
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
			HostsFileContent = File.ReadAllText(@"C:\Windows\System32\drivers\etc\hosts");
		}


		[RelayCommand]
		private async Task RefreshConnection()
		{
			//await RefreshRulesAsync();
		}
	}
}