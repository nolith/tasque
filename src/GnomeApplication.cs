using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Xml;

using Mono.Unix;
using Mono.Unix.Native;

namespace Tasque
{
	public class GnomeApplication : INativeApplication
	{
		private Gnome.Program program;
		private string confDir;

		public GnomeApplication ()
		{
			confDir = Path.Combine (
				Environment.GetFolderPath (
				Environment.SpecialFolder.ApplicationData),
				"tasque");
			if (!Directory.Exists (confDir))
				Directory.CreateDirectory (confDir);
		}

		public void Initialize (string locale_dir,
		                        string display_name,
		                        string process_name,
		                        string [] args)
		{
			Mono.Unix.Catalog.Init ("tasque", Defines.LocaleDir);
			try {
				SetProcessName (process_name);
			} catch {} // Ignore exception if fail (not needed to run)

			Gtk.Application.Init ();
			program = new Gnome.Program (display_name,
			                             Defines.Version,
			                             Gnome.Modules.UI,
			                             args);
		}

		public void InitializeIdle ()
		{
		}

		public event EventHandler ExitingEvent;

		public void Exit (int exitcode)
		{
			OnExitSignal (-1);
			System.Environment.Exit (exitcode);
		}

		public void StartMainLoop ()
		{
			program.Run ();
		}

		public void QuitMainLoop ()
		{
			Gtk.Main.Quit ();
		}

		[DllImport("libc")]
		private static extern int prctl (int option,
			                                 byte [] arg2,
			                                 IntPtr arg3,
			                                 IntPtr arg4,
			                                 IntPtr arg5);

		// From Banshee: Banshee.Base/Utilities.cs
		private void SetProcessName (string name)
		{
			if (prctl (15 /* PR_SET_NAME */,
			                Encoding.ASCII.GetBytes (name + "\0"),
			                IntPtr.Zero,
			                IntPtr.Zero,
			                IntPtr.Zero) != 0)
				throw new ApplicationException (
				        "Error setting process name: " +
				        Mono.Unix.Native.Stdlib.GetLastError ());
		}

		private void OnExitSignal (int signal)
		{
			if (ExitingEvent != null)
				ExitingEvent (null, new EventArgs ());

			if (signal >= 0)
				System.Environment.Exit (0);
		}
		
		public void OpenUrl (string url)
		{
			Gnome.Url.Show (url);
		}
		
		public string ConfDir {
			get {
				return confDir;
			}
		}
	}
}
