using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using ConnectionMgr.Properties;
using Microsoft.Extensions.Logging;

namespace ConnectionMgr.Helpers
{
	/// <summary>
	/// WPF creates a new settings file for every new app version, and the old settings file is left untouched.
	/// Because we never read from it again, it looks as if the settings "reset".
	/// The runtime has no built‑in "auto‑upgrade" that migrates the old settings for us, so we must do it explicitly.
	/// </summary>
	/// <remarks>
	/// Little helper class for better managing the standard default settings upgrade mechanism like described in
	/// <see cref="https://stackoverflow.com/questions/23924183/keep-users-settings-after-altering-assembly-file-version"/>
	/// </remarks>
	public static class SettingsUpgradeManager
	{
		public static void EnsureUpgraded()
		{
			try
			{
				// 1. the last known app version in the settings
				if (Settings.Default.LastKnownAppVersion != Assembly.GetExecutingAssembly().GetName().Version.ToString())
				{
					// 2. perform the upgrade – copies old values into the new settings file
					Settings.Default.Upgrade();                     // <-- merges old values

					// 3. update the stored version so we don't upgrade again
					Settings.Default.LastKnownAppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

					// 4. Persist the changes
					Settings.Default.Save();
				}
			}
			catch
			{
				// wayne
			}
		}
	}
}
