using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using ConnectionMgr.Helpers;
using ConnectionMgr.Models;
using ConnectionMgr.Properties;
using WindowsFirewallHelper;
using Wpf.Ui;

namespace ConnectionMgr.ViewModels.Pages
{
	public partial class HostsFileEditViewModel : PagesBaseViewModel
	{
		private const string PATH_TO_HOSTSFILE = @"C:\Windows\System32\drivers\etc\hosts";

		[ObservableProperty]
		private bool _hasChanges = false;

		[ObservableProperty]
		private string _hostsFileContent = string.Empty;

		private bool _isInitialized = false;

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

		[RelayCommand(CanExecute = nameof(CanExecuteFile))]
		private void CancelChanges(bool hasChanges)
		{
			if (!hasChanges)
				return;

			ReadHostsFile();
			ShowInfo("Pending changes canceled", "The content of the Hosts File is now in its orriginal state again.");
		}

		private bool CanExecuteFile(bool hasChanges)
		{
			return HasChanges;
		}

		[RelayCommand]
		private async Task EditorTextChanged(TextBox box)
		{
			HasChanges = true;
		}

		private async Task InitializeViewModel()
		{
			await ReadHostsFile();
			_isInitialized = true;
		}

		private async Task ReadHostsFile()
		{
			try
			{
				await SetProgressAsync(true);
				HostsFileContent = File.ReadAllText(PATH_TO_HOSTSFILE);
				await SetProgressAsync(false);
				HasChanges = false;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "HostFileEditPage::ReadHostsFile");
				await SetProgressAsync(false);
				ShowError(ex);
			}
		}

		[RelayCommand(CanExecute = nameof(CanExecuteFile))]
		private async Task SaveFile(bool hasChanges)
		{
			if (!hasChanges)
				return;

			try
			{
				await SetProgressAsync(true);
				File.WriteAllText(PATH_TO_HOSTSFILE, HostsFileContent);
				await SetProgressAsync(false);
				_logger.Debug("Changes in Windows Hosts File saved to disc.");
				ShowInfo("File saved", "The Hosts File has been successfully written to disc.");
				HasChanges = false;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "HostFileEditPage::SaveFile");
				await SetProgressAsync(false);
				ShowError(ex);
			}
		}
	}
}