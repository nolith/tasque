// SqliteBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008

using System;
using System.Collections.Generic;
using Mono.Unix;
using Tasque.Backends;
using Mono.Data.Sqlite;

namespace Tasque.Backends.Sqlite
{
	public class SqliteBackend : IBackend
	{
		private Dictionary<int, Gtk.TreeIter> taskIters;
		private Gtk.TreeStore taskStore;
		private Gtk.TreeModelSort sortedTasksModel;
		private bool initialized;
		private bool configured = true;
		
		private Database db;
		
		private Gtk.ListStore categoryListStore;
		private Gtk.TreeModelSort sortedCategoriesModel;

		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;
		
		SqliteCategory defaultCategory;
		//SqliteCategory workCategory;
		//SqliteCategory projectsCategory;
		
		public SqliteBackend ()
		{
			initialized = false;
			taskIters = new Dictionary<int, Gtk.TreeIter> (); 
			taskStore = new Gtk.TreeStore (typeof (ITask));
			
			sortedTasksModel = new Gtk.TreeModelSort (taskStore);
			sortedTasksModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareTasksSortFunc));
			sortedTasksModel.SetSortColumnId (0, Gtk.SortType.Ascending);
			
			categoryListStore = new Gtk.ListStore (typeof (ICategory));
			
			sortedCategoriesModel = new Gtk.TreeModelSort (categoryListStore);
			sortedCategoriesModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareCategorySortFunc));
			sortedCategoriesModel.SetSortColumnId (0, Gtk.SortType.Ascending);
		}
		
		#region Public Properties
		public string Name
		{
			get { return "Local File"; } // TODO: Return something more usable to the user like, "Built-in" or whatever
		}
		
		/// <value>
		/// All the tasks including ITaskDivider items.
		/// </value>
		public Gtk.TreeModel Tasks
		{
			get { return sortedTasksModel; }
		}
		
		/// <value>
		/// This returns all the task lists (categories) that exist.
		/// </value>
		public Gtk.TreeModel Categories
		{
			get { return sortedCategoriesModel; }
		}
		
		/// <value>
		/// Indication that the Sqlite backend is configured
		/// </value>
		public bool Configured 
		{
			get { return configured; }
		}
		
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized
		{
			get { return initialized; }
		}		
		
		public Database Database
		{
			get { return db; }
		}
		#endregion // Public Properties
		
		#region Public Methods
		public ITask CreateTask (string taskName, ICategory category)		
		{
			// not sure what to do here with the category
			SqliteTask task = new SqliteTask (this, taskName);
			
			// Determine and set the task category
			if (category == null || category is Tasque.AllCategory)
				task.Category = defaultCategory; // Default to work
			else
				task.Category = category;
			
			Gtk.TreeIter iter = taskStore.AppendNode ();
			taskStore.SetValue (iter, 0, task);
			taskIters [task.SqliteId] = iter;
			
			return task;
		}
		
		public void DeleteTask(ITask task)
		{
			//string id = task.Id;
			task.Delete ();
			//string command = "delete from Tasks where id=" + id;
			//db.ExecuteNonQuery (command);
		}
		
		public void Refresh()
		{}
		
		public void Initialize()
		{
			if(db == null)
				db = new Database();
				
			db.Open();
			
			//
			// Add in the "All" Category
			//
			AllCategory allCategory = new Tasque.AllCategory ();
			Gtk.TreeIter iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, allCategory);
			
			
			RefreshCategories();
			RefreshTasks();		

		
			initialized = true;
			if(BackendInitialized != null) {
				BackendInitialized();
			}		
		}

		public void Cleanup()
		{
			this.categoryListStore.Clear();
			this.taskStore.Clear();
			this.taskIters.Clear();

			if (db != null)
				db.Close();
			db = null;
			initialized = false;		
		}

		public Gtk.Widget GetPreferencesWidget ()
		{
			// TODO: Replace this with returning null once things are going
			// so that the Preferences Dialog doesn't waste space.
			return new Gtk.Label ("Local file requires no configuration.");
		}

		/// <summary>
		/// Given some text to be input into the database, do whatever
		/// processing is required to make sure special characters are
		/// escaped, etc.
		/// </summary>
		public string SanitizeText (string text)
		{
			return text.Replace ("'", "''");
		}
		
		#endregion // Public Methods
		
		#region Private Methods
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
		
		static int CompareCategorySortFunc (Gtk.TreeModel model,
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
		
		public void UpdateTask (SqliteTask task)
		{
			// Set the task in the store so the model will update the UI.
			Gtk.TreeIter iter;
			
			if (!taskIters.ContainsKey (task.SqliteId))
				return;
				
			iter = taskIters [task.SqliteId];
			
			if (task.State == TaskState.Deleted) {
				taskIters.Remove (task.SqliteId);
				if (!taskStore.Remove (ref iter)) {
					Logger.Debug ("Successfully deleted from taskStore: {0}",
						task.Name);
				} else {
					Logger.Debug ("Problem removing from taskStore: {0}",
						task.Name);
				}
			} else {
				taskStore.SetValue (iter, 0, task);
			}
		}
		
		
		
		public void RefreshCategories()
		{
			Gtk.TreeIter iter;
			SqliteCategory newCategory;
			bool hasValues = false;
			
			string command = "SELECT id FROM Categories";
			SqliteCommand cmd = db.Connection.CreateCommand();
			cmd.CommandText = command;
			SqliteDataReader dataReader = cmd.ExecuteReader();
			while(dataReader.Read()) {
			    int id = dataReader.GetInt32(0);
				hasValues = true;
				
				newCategory = new SqliteCategory (this, id);
				if( (defaultCategory == null) || (newCategory.Name.CompareTo("Work") == 0) )
					defaultCategory = newCategory;
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, newCategory);				
			}
			
			dataReader.Close();
			cmd.Dispose();

			if(!hasValues)
			{
				defaultCategory = newCategory = new SqliteCategory (this, "Work");
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, newCategory);

				newCategory = new SqliteCategory (this, "Personal");
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, newCategory);
				
				newCategory = new SqliteCategory (this, "Family");
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, newCategory);		

				newCategory = new SqliteCategory (this, "Project");
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, newCategory);		
			}
		}
		

		public void RefreshTasks()
		{
			Gtk.TreeIter iter;
			SqliteTask newTask;
			bool hasValues = false;

			string command = "SELECT id,Category,Name,DueDate,CompletionDate,Priority, State FROM Tasks";
			SqliteCommand cmd = db.Connection.CreateCommand();
			cmd.CommandText = command;
			SqliteDataReader dataReader = cmd.ExecuteReader();
			while(dataReader.Read()) {
				int id = dataReader.GetInt32(0);
				int category = dataReader.GetInt32(1);
				string name = dataReader.GetString(2);
				long dueDate = dataReader.GetInt64(3);
				long completionDate = dataReader.GetInt64(4);
				int priority = dataReader.GetInt32(5);
				int state = dataReader.GetInt32(6);

				hasValues = true;

				newTask = new SqliteTask(this, id, category,
				                         name, dueDate, completionDate,
				                         priority, state);
				iter = taskStore.AppendNode();
				taskStore.SetValue (iter, 0, newTask);
				taskIters [newTask.SqliteId] = iter;
			}

			dataReader.Close();
			cmd.Dispose();

			if(!hasValues)
			{
				newTask = new SqliteTask (this, "Create some tasks");
				newTask.Category = defaultCategory;
				newTask.DueDate = DateTime.Now;
				newTask.Priority = TaskPriority.Medium;
				iter = taskStore.AppendNode ();
				taskStore.SetValue (iter, 0, newTask);	
				taskIters [newTask.SqliteId] = iter;
			}
		}

		#endregion // Private Methods
		
		#region Event Handlers
		#endregion // Event Handlers
	}
}
