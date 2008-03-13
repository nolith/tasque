/***************************************************************************
 *  Application.cs
 *
 *  Copyright (C) 2008 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net.Sockets;

using Gtk;
using Gdk;
using Gnome;
using Mono.Unix;
using Mono.Unix.Native;
#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif
using Tasque.Backends;

namespace Tasque
{
	class Application
	{
		private static Tasque.Application application = null;
		private static System.Object locker = new System.Object();

		private Gnome.Program program;
		private RemoteControl remoteControl;
		private Gdk.Pixbuf normalPixBuf;
		private Gtk.Image trayImage;
		private Egg.TrayIcon trayIcon;	
		private Preferences preferences;
		private EventBox eb;
		private IBackend backend;
		private PreferencesDialog preferencesDialog;
		
		/// <value>
		/// Keep track of the available backends.  The key is the Type name of
		/// the backend.
		/// </value>
		private Dictionary<string, IBackend> availableBackends;
		
		private IBackend customBackend;

		public static IBackend Backend
		{ 
			get { return Application.Instance.backend; }
			set {
				Application tasque = Application.Instance;
				if (tasque.backend != null) {
					// Cleanup the old backend
					try {
						Logger.Debug ("Cleaning up backend: {0}",
									  tasque.backend.Name);
						tasque.backend.Cleanup ();
					} catch (Exception e) {
						Logger.Warn ("Exception cleaning up '{0}': {1}",
									 tasque.backend.Name,
									 e.Message);
					}
				}
					
				// Initialize the new backend
				tasque.backend = value;
				if (tasque.backend == null) {
					return;
				}
					
				Logger.Info ("Using backend: {0} ({1})",
							 tasque.backend.Name,
							 tasque.backend.GetType ().ToString ());
				tasque.backend.Initialize();
				
				TaskWindow.Reinitialize ();
				
				Logger.Debug("Configuration status: {0}",
							 tasque.backend.Configured.ToString());
				if (tasque.backend.Configured == false) {
					Application.ShowPreferences();
				}
			}
		}
		
		public static List<IBackend> AvailableBackends
		{
			get {
				return new List<IBackend> (Application.Instance.availableBackends.Values);
			}
//			get { return Application.Instance.availableBackends; }
		}
		
		public static Application Instance
		{
			get {
				lock(locker) {
					if(application == null) {
						lock(locker) {
							application = new Application();
						}
					}
					return application;
				}
			}
		}

		public static Preferences Preferences
		{
			get { return Application.Instance.preferences; }
		}

		private Application ()
		{
			Init(null);
		}

		private Application (string[] args)
		{
			Init(args);
		}

		private void Init(string[] args)
		{
			program = new Gnome.Program (
							"Tasque",
							Defines.Version,
							Gnome.Modules.UI,
							args);

			preferences = new Preferences();
			
			// Register Tasque RemoteControl
			try {
				remoteControl = RemoteControlProxy.Register ();
				if (remoteControl != null) {
					Logger.Debug ("Tasque remote control active.");
				} else {
					// If Tasque is already running, open the tasks window
					// so the user gets some sort of feedback when they
					// attempt to run Tasque again.
					RemoteControl remote = null;
					try {
						remote = RemoteControlProxy.GetInstance ();
						remote.ShowTasks ();
					} catch {}

					Logger.Debug ("Tasque is already running.  Exiting...");
					System.Environment.Exit (-1);
				}
			} catch (Exception e) {
				Logger.Debug ("Tasque remote control disabled (DBus exception): {0}",
				            e.Message);
			}
			
			// Read the args and check to see if a specific backend is specified
			if (args.Length > 0) {
Logger.Debug ("args [0]: {0}", args [0]);
				// We're only looking at the first argument
				string potentialBackendClassName = args [0];
				
				customBackend = null;
				Assembly asm = Assembly.GetCallingAssembly ();
				try {
					customBackend = (IBackend)
						asm.CreateInstance (potentialBackendClassName);
				} catch (Exception e) {
					Logger.Warn ("Backend specified on args not found: {0}\n\t{1}",
						potentialBackendClassName, e.Message);
				}
			}
			
			// Discover all available backends
			LoadAvailableBackends ();

			GLib.Idle.Add(InitializeIdle);
		}
		
		/// <summary>
		/// Load all the available backends that Tasque can find.  First look in
		/// Tasque.exe and then for other DLLs in the same directory Tasque.ex
		/// resides.
		/// </summary>
		private void LoadAvailableBackends ()
		{
			availableBackends = new Dictionary<string,IBackend> ();
			
			List<IBackend> backends = new List<IBackend> ();
			
			Assembly tasqueAssembly = Assembly.GetCallingAssembly ();
			
			// Look for other backends in Tasque.exe
			backends.AddRange (GetBackendsFromAssembly (tasqueAssembly));
			
			// Look through the assemblies located in the same directory as
			// Tasque.exe.
			Logger.Debug ("Tasque.exe location:  {0}", tasqueAssembly.Location);
			
			DirectoryInfo loadPathInfo =
				Directory.GetParent (tasqueAssembly.Location);
			Logger.Info ("Searching for Backend DLLs in: {0}", loadPathInfo.FullName);
			
			foreach (FileInfo fileInfo in loadPathInfo.GetFiles ("*.dll")) {
				Logger.Info ("\tReading {0}", fileInfo.FullName);
				Assembly asm = null;
				try {
					asm = Assembly.LoadFile (fileInfo.FullName);
				} catch (Exception e) {
					Logger.Debug ("Exception loading {0}: {1}",
								  fileInfo.FullName,
								  e.Message);
					continue;
				}
				
				backends.AddRange (GetBackendsFromAssembly (asm));
			}
			
			foreach (IBackend backend in backends) {
				string typeId = backend.GetType ().ToString ();
				if (availableBackends.ContainsKey (typeId) == true)
					continue;
				
				Logger.Debug ("Storing '{0}' = '{1}'", typeId, backend.Name);
				availableBackends [typeId] = backend;
			}
		}
		
		private List<IBackend> GetBackendsFromAssembly (Assembly asm)
		{
			List<IBackend> backends = new List<IBackend> ();
			
			foreach (Type type in asm.GetTypes ()) {
				if (type.IsClass == false)
					continue; // Skip non-class types
				if (type.GetInterface ("Tasque.Backends.IBackend") == null)
					continue;
				Logger.Debug ("Found Available Backend: {0}", type.ToString ());
				
				IBackend availableBackend = null;
				try {
					availableBackend = (IBackend)
						asm.CreateInstance (type.ToString ());
				} catch (Exception e) {
					Logger.Warn ("Could not instantiate {0}: {1}",
								 type.ToString (),
								 e.Message);
					continue;
				}
				
				if (availableBackend != null)
					backends.Add (availableBackend);
			}
			
			return backends;
		}

		private bool InitializeIdle()
		{
			if (customBackend != null) {
				Application.Backend = customBackend;
			} else {
				// Check to see if the user has a preference of which backend
				// to use.  If so, use it, otherwise, pop open the preferences
				// dialog so they can choose one.
				string backendTypeString = Preferences.Get (Preferences.CurrentBackend);
				Logger.Debug ("CurrentBackend specified in Preferences: {0}", backendTypeString);
				if (backendTypeString != null
						&& availableBackends.ContainsKey (backendTypeString)) {
					Application.Backend = availableBackends [backendTypeString];
				}
			}
			
			SetupTrayIcon ();
			
			if (backend == null) {
				// Pop open the preferences dialog so the user can choose a
				// backend service to use.
				Application.ShowPreferences ();
			} else {
				TaskWindow.ShowWindow();
			}
			
			return false;
		}

		private void UpdateTrayIcon()
		{
		}

		private void SetupTrayIcon ()
		{
			eb = new EventBox();
			normalPixBuf = Utilities.GetIcon ("tasque-24", 24);
			trayImage = new Gtk.Image(normalPixBuf);
			eb.Add(trayImage);

			// hooking event
			eb.ButtonPressEvent += OnTrayIconClick;
			trayIcon = new Egg.TrayIcon("Tasque");
			trayIcon.Add(eb); 

//			trayIcon.EnterNotifyEvent += OnTrayIconEnterNotifyEvent;
//			trayIcon.LeaveNotifyEvent += OnTrayIconLeaveNotifyEvent;
			// showing the trayicon
			trayIcon.ShowAll();			
		}


		private void OnPreferences (object sender, EventArgs args)
		{
			Logger.Info ("OnPreferences called");
			if (preferencesDialog == null) {
				preferencesDialog = new PreferencesDialog ();
				preferencesDialog.Hidden += OnPreferencesDialogHidden;
			}
			
			preferencesDialog.Present ();
		}
		
		private void OnPreferencesDialogHidden (object sender, EventArgs args)
		{
			preferencesDialog.Destroy ();
			preferencesDialog = null;
		}
		
		public static void ShowPreferences()
		{
			application.OnPreferences(null, EventArgs.Empty);
		}

		private void OnAbout (object sender, EventArgs args)
		{
			string [] authors = new string [] {
				"Boyd Timothy <btimothy@gmail.com>",
				"Calvin Gaisford <calvinrg@gmail.com>"
			};

			/* string [] documenters = new string [] {
			   "Calvin Gaisford <calvinrg@gmail.com>"
			   };

			   string translators = Catalog.GetString ("translator-credits");
			   if (translators == "translator-credits")
			   translators = null;
			 */


			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.Name = "Tasque";
			about.Version = Defines.Version;
			about.Logo = Utilities.GetIcon("tasque-48", 48);
			about.Copyright =
				Catalog.GetString ("Copyright \xa9 2008 Novell, Inc.");
			about.Comments = Catalog.GetString ("A Useful Task List");
			about.Website = "http://live.gnome.org/Tasque";
			about.WebsiteLabel = Catalog.GetString("Tasque Project Homepage");
			about.Authors = authors;
			//about.Documenters = documenters;
			//about.TranslatorCredits = translators;
			about.IconName = "tasque";
			about.SetSizeRequest(300, 300);
			about.Run ();
			about.Destroy ();

		}


		private void OnShowTaskWindow (object sender, EventArgs args)
		{
			TaskWindow.ShowWindow();
		}
		
		private void OnNewTask (object sender, EventArgs args)
		{
			// Show the TaskWindow and then cause a new task to be created
			TaskWindow.ShowWindow ();
			TaskWindow.AddTask ();
		}

		private void OnQuit (object sender, EventArgs args)
		{
			Logger.Info ("OnQuitAction called - terminating application");
			if (backend != null) {
				backend.Cleanup();
			}
			TaskWindow.SavePosition();			
			program.Quit (); // Should this be called instead?
		}
		
		private void OnRefreshAction (object sender, EventArgs args)
		{
			Application.Backend.Refresh();
		}
		
		

		private void OnTrayIconClick (object o, ButtonPressEventArgs args) // handler for mouse click
		{
			if (args.Event.Button == 1) {
				TaskWindow.ShowWindow();
			} else if (args.Event.Button == 3) {
				// FIXME: Eventually get all these into UIManagerLayout.xml file
				Menu popupMenu = new Menu();

				ImageMenuItem showTasksItem = new ImageMenuItem
					(Catalog.GetString ("Show Tasks ..."));

				showTasksItem.Image = new Gtk.Image(Utilities.GetIcon ("tasque-16", 16));
				showTasksItem.Sensitive = backend != null && backend.Initialized;
				showTasksItem.Activated += OnShowTaskWindow;
				popupMenu.Add (showTasksItem);
				
				ImageMenuItem newTaskItem = new ImageMenuItem
					(Catalog.GetString ("New Task ..."));
				newTaskItem.Image = new Gtk.Image (Gtk.Stock.New, IconSize.Menu);
				newTaskItem.Sensitive = backend != null && backend.Initialized;
				newTaskItem.Activated += OnNewTask;
				popupMenu.Add (newTaskItem);

				SeparatorMenuItem separator = new SeparatorMenuItem ();
				popupMenu.Add (separator);

				ImageMenuItem preferences = new ImageMenuItem (Gtk.Stock.Preferences, null);
				preferences.Activated += OnPreferences;
				popupMenu.Add (preferences);

				ImageMenuItem about = new ImageMenuItem (Gtk.Stock.About, null);
				about.Activated += OnAbout;
				popupMenu.Add (about);

				separator = new SeparatorMenuItem ();
				popupMenu.Add (separator);

				ImageMenuItem refreshAction = new ImageMenuItem
					(Catalog.GetString ("Refresh Tasks"));

				refreshAction.Image = new Gtk.Image(Utilities.GetIcon (Gtk.Stock.Execute, 16));
				refreshAction.Sensitive = backend != null && backend.Initialized;
				refreshAction.Activated += OnRefreshAction;
				popupMenu.Add (refreshAction);
				
				separator = new SeparatorMenuItem ();
				popupMenu.Add (separator);
				
				ImageMenuItem quit = new ImageMenuItem ( Gtk.Stock.Quit, null);
				quit.Activated += OnQuit;
				popupMenu.Add (quit);

				popupMenu.ShowAll(); // shows everything
				//popupMenu.Popup(null, null, null, IntPtr.Zero, args.Event.Button, args.Event.Time);
				popupMenu.Popup(null, null, null, args.Event.Button, args.Event.Time);
			}
		}		



		public static void Main(string[] args)
		{
			try 
			{
				Utilities.SetProcessName ("Tasque");
				application = GetApplicationWithArgs(args);
				application.StartMainLoop ();
			} 
			catch (Exception e)
			{
				Tasque.Logger.Debug("Exception is: {0}", e);
				Exit (-1);
			}
		}

		public static Application GetApplicationWithArgs(string[] args)
		{
			lock(locker)
			{
				if(application == null)
				{
					lock(locker)
					{
						application = new Application(args);
					}
				}
				return application;
			}
		}

		public static void OnExitSignal (int signal)
		{
			if (ExitingEvent != null) ExitingEvent (null, EventArgs.Empty);
			if (signal >= 0) System.Environment.Exit (0);
		}

		public static event EventHandler ExitingEvent = null;

		public static void Exit (int exitcode)
		{
			OnExitSignal (-1);
			System.Environment.Exit (exitcode);
		}

#if ENABLE_NOTIFY_SHARP
		public static void ShowAppNotification(Notification notification)
		{
			notification.AttachToWidget(
					Tasque.Application.Instance.trayIcon);
			notification.Show();
		}
#endif


		public void StartMainLoop ()
		{
			program.Run ();
		}

		public void QuitMainLoop ()
		{
			//	actionManager ["QuitAction"].Activate ();
		}

	}
}
