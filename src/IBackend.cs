// ITaskBackend.cs created with MonoDevelop
// User: boyd at 7:02 AM 2/11/2008

using System;

namespace Tasque.Backends
{
	public delegate void BackendInitializedHandler ();
	public delegate void BackendSyncStartedHandler ();
	public delegate void BackendSyncFinishedHandler ();
	
	/// <summary>
	/// This is the main integration interface for different backends that
	/// Tasque can use.
	/// </summary>
	public interface IBackend
	{
		event BackendInitializedHandler BackendInitialized;
		event BackendSyncStartedHandler BackendSyncStarted;
		event BackendSyncFinishedHandler BackendSyncFinished;

		#region Properties
		/// <value>
		/// A human-readable name for the backend that will be displayed in the
		/// preferences dialog to allow the user to select which backend Tasque
		/// should use.
		/// </value>
		string Name
		{
			get;
		}
		
		/// <value>
		/// All the tasks provided by the backend.
		/// </value>
		Gtk.TreeModel Tasks
		{
			get;
		}
		
		/// <value>
		/// This returns all the ICategory items from the backend.
		/// </value>
		Gtk.TreeModel Categories
		{
			get;
		}
		
		/// <value>
		/// Indication that the backend has enough information
		/// (credentials/etc.) to run.  If false, the properties dialog will
		/// be shown so the user can configure the backend.
		/// </value>
		bool Configured
		{
			get;
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		bool Initialized
		{
			get;
		}
		#endregion // Properties
		
		#region Methods
		/// <summary>
		/// Create a new task.
		/// </summary>
		ITask CreateTask (string taskName, ICategory category);

		/// <summary>
		/// Deletes the specified task.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		void DeleteTask (ITask task);
		
		/// <summary>
		/// Refreshes the backend.
		/// </summary>
		void Refresh();
		
		/// <summary>
		/// Initializes the backend
		/// </summary>
		void Initialize();

		/// <summary>
		/// Cleanup the backend before quitting
		/// </summary>
		void Cleanup();
		
		/// <summary>
		/// A widget that will be placed into the Preferences Dialog when the
		/// backend is the one being used.
		/// </summary>
		/// <returns>
		/// A <see cref="Gtk.Widget"/>
		/// </returns>
		Gtk.Widget GetPreferencesWidget ();

#endregion // Methods
	}
}
