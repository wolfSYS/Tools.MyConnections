using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using MyConnections.Helpers;
using MyConnections.Models;
using MyConnections.Properties;
using WindowsFirewallHelper;
using Wpf.Ui;

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