using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace ConnectionMgr.ViewModels.Dialogs
{
	/// <summary>
	/// ViewModel for the AI Overview dialog.
	/// </summary>
	public partial class AiOverviewViewModel : ObservableObject
	{
		// --------------------------------------------------------------------
		//  Properties
		// --------------------------------------------------------------------
		/// <summary>Dialog’s window title.</summary>
		[ObservableProperty]
		private string _dialogTitle;

		/// <summary>Message shown in the read‑only editor.</summary>
		[ObservableProperty]
		private string _message;

		// --------------------------------------------------------------------
		//  Commands
		// --------------------------------------------------------------------
		/// <summary>
		/// Copies the Message to the clipboard.
		/// </summary>
		[RelayCommand]
		private void Copy2Clipboard()
		{
			Clipboard.SetText(Message ?? string.Empty);
		}

		/// <summary>
		/// Signals that the dialog should close.
		/// </summary>
		[RelayCommand]
		private void CloseDialog()
		{
			// Raise an event that the view will subscribe to.
			CloseRequested?.Invoke(this, EventArgs.Empty);
		}

		// --------------------------------------------------------------------
		//  Public event – used by the view to actually close the window.
		// --------------------------------------------------------------------
		public event EventHandler CloseRequested;
	}
}