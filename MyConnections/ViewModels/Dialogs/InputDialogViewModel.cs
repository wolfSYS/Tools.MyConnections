using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConnectionMgr.ViewModels.Dialogs
{
	public partial class InputDialogViewModel : ObservableObject
	{
		[ObservableProperty] private string _dialogTitle = "Your input is required";

		[ObservableProperty] private string _message = string.Empty;

		[ObservableProperty] private string _inputText = string.Empty;

		public event EventHandler? OkRequested;

		public ICommand OkCommand => new RelayCommand(OnOk);

		private void OnOk()
		{
			// Notify the view that it should close the dialog.
			OkRequested?.Invoke(this, EventArgs.Empty);
		}
	}
}
