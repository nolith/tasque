// IceBackend.cs created with MonoDevelop
// User: boyd at 8:32 AM 2/14/2008

using System;
using System.Collections.Generic;
using NDesk.DBus;
using org.freedesktop.DBus;
using Novell.IceDesktop;
using Tasque;
using Tasque.Backends;

namespace Tasque.Backends.IceCore
{
	public class IceBackend : IBackend
	{
		static ObjectPath DaemonPath =
			new ObjectPath ("/Novell/ICEDesktop/Daemon");
		static string DaemonNamespace = "Novell.ICEDesktop.Daemon";
		
		private org.freedesktop.DBus.IBus sessionBus;
		Novell.IceDesktop.IDaemon deskIceDaemon = null;
		
		private Gtk.ListStore tasks;
		private Gtk.TreeModelSort sortedTasks;
		private Dictionary<string, Gtk.TreeIter> taskIters;
		
		private Gtk.ListStore categories;
		private Gtk.TreeModelSort sortedCategories;
		private Dictionary<string, Gtk.TreeIter> categoryIters;
		
		private bool initialized;
		
		public IceBackend ()
		{
			initialized = false;
			
			//
			// Set up the Tasks ListStore
			//
			tasks = new Gtk.ListStore (typeof (ITask));
			sortedTasks = new Gtk.TreeModelSort (tasks);
			sortedTasks.SetSortFunc (0,
				new Gtk.TreeIterCompareFunc (CompareTasksSortFunc));
			sortedTasks.SetSortColumnId (0, Gtk.SortType.Ascending);
			taskIters = new Dictionary<string, Gtk.TreeIter> ();
			
			categories = new Gtk.ListStore (typeof (ICategory));
			sortedCategories = new Gtk.TreeModelSort (categories);
			sortedCategories.SetSortFunc (0,
				new Gtk.TreeIterCompareFunc (CompareCategoriesSortFunc));
			sortedCategories.SetSortColumnId (0, Gtk.SortType.Ascending);
			categoryIters = new Dictionary<string, Gtk.TreeIter> ();
			
			DaemonService.Initialize ();
		}
		
		#region Events
		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;
		#endregion // Events

		#region Properties
		public string Name
		{
			get { return "Novell Teaming and Conferencing"; }
		}
		
		/// <value>
		/// All the tasks provided by the backend.
		/// </value>
		public Gtk.TreeModel Tasks
		{
			get { return sortedTasks; }
		}
		
		/// <value>
		/// This returns all the ICategory items from the backend.
		/// </value>
		public Gtk.TreeModel Categories
		{
			get { return sortedCategories; }
		}
		
		/// <value>
		/// Indication that the backend has enough information
		/// (credentials/etc.) to run.  If false, the properties dialog will
		/// be shown so the user can configure the backend.
		/// </value>
		public bool Configured
		{
			// TODO: Implement IceBackend.Configured:bool
			get { return true; }
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized
		{
			get { return initialized; }
		}
		#endregion // Properties
		
		#region Methods
		/// <summary>
		/// Create a new task.
		/// </summary>
		public ITask CreateTask (string taskName, ICategory category)
		{
			IceTask task = null;
			IceCategory iceCategory = category as IceCategory;
			
			if (taskName == null || taskName.Trim () == string.Empty
					|| category == null) {
				Logger.Warn ("Cannot call IceBackend.CreateTask () with null/empty arguments.");
				return null;
			}
			
			string taskId = null;
			try {
				taskId =
				deskIceDaemon.CreateTask (iceCategory.Folder.ID,
										  taskName,
										  string.Empty,	// description
										  5,			// Lowest Priority/None
										  TaskStatus.NeedsAction,
										  string.Empty,	// start date
										  string.Empty);	// due date
			} catch (Exception e) {
				Logger.Error ("Exception calling deskIceDaemon.CreateTask (): {0}", e.Message);
				return null;
			}
			
			TaskEntry taskEntry = new TaskEntry (taskId, taskName);
			task = new IceTask (this, iceCategory, taskEntry);
			
			UpdateTask (task);
			
			return task;
		}
		
		/// <summary>
		/// Deletes the specified task.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		public void DeleteTask (ITask task)
		{
			Logger.Debug ("TODO: Implement IceBackend.DeleteTask()");
		}


		/// <summary>
		/// Refreshes the backend.
		/// </summary>
		public void Refresh()
		{
			// TODO: Eventually don't clear out existing entries, but match up
			// the updates
//			categories.Clear ();
			
			// TODO: Eventually don't clear out existing tasks, but match them
			// up with the updates
//			tasks.Clear ();
			
			Teamspace [] teams = null;
			try {
				teams = deskIceDaemon.GetTeamList ();
			} catch {
				Logger.Warn ("Exception thrown getting team list");
				return;
			}
			
			foreach (Teamspace team in teams) {
				Logger.Debug ("Team Found: {0} ({1})", team.Name, team.ID);
				
				// Check to see if the team has tasks enabled
				TaskFolder [] taskFolders = null;
				try {
					taskFolders = deskIceDaemon.GetTaskFolders (team.ID);
				} catch {
					Logger.Warn ("Exception thrown getting task folders");
					return;
				}
				
				foreach (TaskFolder taskFolder in taskFolders) {
					// Create an IceCategory for each folder
					IceCategory category = new IceCategory (team, taskFolder);
					Gtk.TreeIter iter;
					if (categoryIters.ContainsKey (category.Id))
						iter = categoryIters [category.Id];
					else {
						iter = categories.Append ();
						categoryIters [category.Id] = iter;
					}
					
					categories.SetValue (iter, 0, category);
					
					LoadTasksFromCategory (category);
				}
			}
			
			// TODO: Wrap this in a try/catch
			if (BackendSyncFinished != null) {
				try {
					BackendSyncFinished ();
				} catch (Exception e) {
					Logger.Warn ("Error calling BackendSyncFinished handler: {0}", e.Message);
				}
			}
		}

		/// <summary>
		/// Initializes the backend
		/// </summary>
		public void Initialize()
		{
			BusG.Init ();
			
			// Watch the session bus for when ICEcore Daemon comes or goes.
			// When it comes, attempt to connect to it.
			sessionBus =
				Bus.Session.GetObject<org.freedesktop.DBus.IBus> (
					"org.freedesktop.DBus",
					new ObjectPath ("/org/freedesktop/DBus"));
			sessionBus.NameOwnerChanged += OnDBusNameOwnerChanged;
			
			// Force the daemon to start up if it's not already running
			if (!Bus.Session.NameHasOwner (DaemonNamespace)) {
				Bus.Session.StartServiceByName (DaemonNamespace);
			}
			
			// Register for ICEcore Daemon's events
			ConnectToICEcoreDaemon ();
			
			//
			// Add in the AllCategory
			//
			AllCategory allCategory = new AllCategory ();
			Gtk.TreeIter iter = categories.Append ();
			categories.SetValue (iter, 0, allCategory);				
			
			// Populate the models
			Refresh ();
			
			initialized = true;
			
			if (BackendInitialized != null) {
				try {
					BackendInitialized ();
				} catch (Exception e) {
					Logger.Debug ("Exception in IceBackend.BackendInitialized handler: {0}", e.Message);
				}
			}
		}

		/// <summary>
		/// Cleanup the backend before quitting
		/// </summary>
		public void Cleanup()
		{
			Logger.Debug ("IceBackend.Cleanup ()");
			
			// TODO: Figure out whether we need to do anything in IceBackend.Cleanup ()");
		}
		
		public Gtk.Widget GetPreferencesWidget ()
		{
			return null;
		}
		
		public void UpdateTask (IceTask task)
		{
			// Set the task in the store so the model will update the UI.
			Gtk.TreeIter iter;
			
			if (!taskIters.ContainsKey (task.Id)) {
				// This must be a new task that should be added in.
				iter = tasks.Append ();
				taskIters [task.Id] = iter;
			} else {
				iter = taskIters [task.Id];
			}
			
			if (task.State == TaskState.Deleted) {
				taskIters.Remove (task.Id);
				if (!tasks.Remove (ref iter)) {
					Logger.Debug ("Successfully deleted from taskStore: {0}",
						task.Name);
				} else {
					Logger.Debug ("Problem removing from taskStore: {0}",
						task.Name);
				}
			} else {
				tasks.SetValue (iter, 0, task);
			}
		}
		
		public void SaveAndUpdateTask (IceTask task)
		{
			// Send new values to the server and then update the task in the
			// TreeModel.
			
			try {
				IceCategory iceCategory = task.Category as IceCategory;
				deskIceDaemon.UpdateTask (iceCategory.Folder.ID,
										  task.Entry.ID,
										  task.Name,
										  task.Entry.Description,
										  task.IceDesktopStatus,
										  int.Parse (task.Entry.Priority),
										  int.Parse (task.Entry.PercentComplete),
										  task.DueDateString);
			} catch (Exception e) {
				Logger.Warn ("Error calling deskIceDaemon.UpdateTask: {0}",
							 e.Message);
				return;
			}
			
			UpdateTask (task);
		}
		#endregion // Public Methods
		
		#region Private Methods
		/// <summary>
		/// Connect with the ICEcore Daemon and register event handlers.
		/// </summary>
		void ConnectToICEcoreDaemon ()
		{
			Console.WriteLine ("Connecting the to the ICEcore Daemon");
			deskIceDaemon = DaemonService.DaemonInstance;
			if (deskIceDaemon != null) {
				// Set up the daemon event handlers
				deskIceDaemon.Authenticated += OnAuthenticated;
				deskIceDaemon.Disconnected += OnDisconnected;
			}
			
			// TODO: Do we need to call a refresh here?
		}
		
		static int CompareTasksSortFunc (Gtk.TreeModel model,
										 Gtk.TreeIter a,
										 Gtk.TreeIter b)
		{
			ITask taskA = model.GetValue (a, 0) as ITask;
			ITask taskB = model.GetValue (b, 0) as ITask;
			
			if (taskA == null || taskB == null)
				return 0;
			
			return (taskA.CompareTo (taskB));
		}
		
		static int CompareCategoriesSortFunc (Gtk.TreeModel model,
											  Gtk.TreeIter a,
											  Gtk.TreeIter b)
		{
			ICategory categoryA = model.GetValue (a, 0) as ICategory;
			ICategory categoryB = model.GetValue (b, 0) as ICategory;
			
			if (categoryA == null || categoryB == null)
				return 0;
			
			if (categoryA is Tasque.AllCategory)
				return -1;
			else if (categoryB is Tasque.AllCategory)
				return 1;
			
			return (categoryA.Name.CompareTo (categoryB.Name));
		}
		
		private void LoadTasksFromCategory (IceCategory category)
		{
			TaskEntry [] taskEntries = null;
			try {
				taskEntries = deskIceDaemon.GetTaskEntries (category.Folder.ID);
			} catch (Exception e) {
				Logger.Warn ("Exception loading tasks from category: {0}", e.Message);
				return;
			}
			
			foreach (TaskEntry entry in taskEntries) {
				IceTask task = new IceTask (this, category, entry);
				Gtk.TreeIter iter;
				if (taskIters.ContainsKey (task.Id))
					iter = taskIters [task.Id];
				else {
					iter = tasks.Append ();
					taskIters [task.Id] = iter;
				}
				
				tasks.SetValue (iter, 0, task);
			}
		}
		#endregion // Private Methods
		
		#region Event Handlers
		void OnDBusNameOwnerChanged (string serviceName,
											string oldOwner,
											string newOwner)
		{
			if (serviceName == null)
				return;
			
			if (serviceName.CompareTo (DaemonNamespace) != 0)
				return;
			
			if (oldOwner != null && oldOwner.Length > 0) {
				// The daemon just went away
				Console.WriteLine ("The ICEcore Daemon just quit.");
				
				// TODO: Determine whether we should force the daemon to start up again
			} else {
				// This is a new daemon
				ConnectToICEcoreDaemon ();
				
				// Populate the models
				Refresh ();
			}
		}
		
		void OnAuthenticated (string server, string username)
		{
			Logger.Debug ("Received authenticated message from ICEcore Daemon");
			
			// Do a refresh
			Refresh ();
		}
		
		void OnDisconnected (string server, string username)
		{
			// TODO: Figure out what to do when the ICEcore Daemon disconnects
			Logger.Debug ("Received disconnect message from ICEcore Daemon");
			
			Refresh (); // ... this will clear out the lists (yuck!)
		}
		#endregion // Event Handlers
	}
}
