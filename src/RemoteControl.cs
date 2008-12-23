// RemoteControl.cs created with MonoDevelop
// User: sandy at 9:49 AM 2/14/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using Mono.Unix; // for Catalog.GetString ()

#if ENABLE_NOTIFY_SHARP
using Notifications;
#endif

using org.freedesktop.DBus;
using NDesk.DBus;

namespace Tasque
{
	[Interface ("org.gnome.Tasque.RemoteControl")]
	public class RemoteControl : MarshalByRefObject
	{
		static Gdk.Pixbuf tasqueIcon;
		static RemoteControl ()
		{
			tasqueIcon = Utilities.GetIcon ("tasque-48", 48);
		}
		
		public RemoteControl()
		{
		}
				
		/// <summary>
		/// Create a new task in Tasque using the given categoryName and name.
		/// Will not attempt to parse due date information.
		/// </summary>
		/// <param name="categoryName">
		/// A <see cref="System.String"/>.  The name of an existing category.
		/// Matches are not case-sensitive.
		/// </param>
		/// <param name="taskName">
		/// A <see cref="System.String"/>.  The name of the task to be created.
		/// </param>
		/// <param name="enterEditMode">
		/// A <see cref="System.Boolean"/>.  Specify true if the TaskWindow
		/// should be shown, the new task scrolled to, and have it be put into
		/// edit mode immediately.
		/// </param>
		/// <returns>
		/// A unique <see cref="System.String"/> which can be used to reference
		/// the task later.
		/// </returns>
		public string CreateTask (string categoryName, string taskName,
						bool enterEditMode)
		{
			return CreateTask (categoryName, taskName, enterEditMode, false);
		}

		/// <summary>
		/// Create a new task in Tasque using the given categoryName and name.
		/// </summary>
		/// <param name="categoryName">
		/// A <see cref="System.String"/>.  The name of an existing category.
		/// Matches are not case-sensitive.
		/// </param>
		/// <param name="taskName">
		/// A <see cref="System.String"/>.  The name of the task to be created.
		/// </param>
		/// <param name="enterEditMode">
		/// A <see cref="System.Boolean"/>.  Specify true if the TaskWindow
		/// should be shown, the new task scrolled to, and have it be put into
		/// edit mode immediately.
		/// </param>
		/// <param name="parseDate">
		/// A <see cref="System.Boolean"/>.  Specify true if the 
		/// date should be parsed out of the taskName (in case 
		/// Preferences.ParseDateEnabledKey is true as well).
		/// </param>
		/// <returns>
		/// A unique <see cref="System.String"/> which can be used to reference
		/// the task later.
		/// </returns>
		public string CreateTask (string categoryName, string taskName,
						bool enterEditMode, bool parseDate)
		{
			Gtk.TreeIter iter;
			Gtk.TreeModel model = Application.Backend.Categories;
			
			//
			// Validate the input parameters.  Don't allow null or empty strings
			// be passed-in.
			//
			if (categoryName == null || categoryName.Trim () == string.Empty
					|| taskName == null || taskName.Trim () == string.Empty) {
				return string.Empty;
			}
			
			//
			// Look for the specified category
			//
			if (!model.GetIterFirst (out iter)) {
				return string.Empty;
			}
			
			ICategory category = null;
			do {
				ICategory tempCategory = model.GetValue (iter, 0) as ICategory;
				if (tempCategory.Name.ToLower ().CompareTo (categoryName.ToLower ()) == 0) {
					// Found a match
					category = tempCategory;
				}
			} while (model.IterNext (ref iter));
			
			if (category == null) {
				return string.Empty;
			}
			
			// If enabled, attempt to parse due date information
			// out of the taskName.
			DateTime taskDueDate = DateTime.MinValue;
			if (parseDate && Application.Preferences.GetBool (Preferences.ParseDateEnabledKey))
				Utilities.ParseTaskText (
				                         taskName,
				                         out taskName,
				                         out taskDueDate);
			ITask task = null;
			try {
				task = Application.Backend.CreateTask (taskName, category);
				if (taskDueDate != DateTime.MinValue)
					task.DueDate = taskDueDate;
			} catch (Exception e) {
				Logger.Error ("Exception calling Application.Backend.CreateTask from RemoteControl: {0}", e.Message);
				return string.Empty;
			}
			
			if (task == null) {
				return string.Empty;
			}
			
			if (enterEditMode) {
				TaskWindow.SelectAndEdit (task);
			}
			
			#if ENABLE_NOTIFY_SHARP
			// Use notify-sharp to alert the user that a new task has been
			// created successfully.
			Notification notify =
				new Notification (
					Catalog.GetString ("New task created."), // summary
					Catalog.GetString (taskName), // body
					tasqueIcon);
			Application.ShowAppNotification (notify);
			#endif
			
			
			return task.Id;
		}
		
		/// <summary>
		/// Return an array of Category names.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string[] GetCategoryNames ()
		{
			List<string> categories = new List<string> ();
			string[] emptyArray = categories.ToArray ();
			
			Gtk.TreeIter iter;
			Gtk.TreeModel model = Application.Backend.Categories;
			
			if (!model.GetIterFirst (out iter))
				return emptyArray;
			
			do {
				ICategory category = model.GetValue (iter, 0) as ICategory;
				if (category is AllCategory)
					continue; // Prevent the AllCategory from being returned
				categories.Add (category.Name);
			} while (model.IterNext (ref iter));
			
			return categories.ToArray ();
		}
		
		public void ShowTasks ()
		{
			TaskWindow.ShowWindow ();
		}
		
		/// <summary>
		/// Retreives the IDs of all tasks for the current backend.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> array containing the ID of all tasks
		/// in the current backend.
		/// </returns>
		public string[] GetTaskIds ()
		{
			Gtk.TreeIter iter;
			Gtk.TreeModel model;
			
			ITask task;
			List<string> ids;
			
			ids = new List<string> ();
			model = Application.Backend.Tasks;
			
			if (!model.GetIterFirst (out iter))
				return new string[0];
				
			do {
				task = model.GetValue (iter, 0) as ITask;
				ids.Add (task.Id);
			} while (model.IterNext (ref iter));
			
			return ids.ToArray ();
		}
		
		/// <summary>
		/// Gets the name of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> the name of the task
		/// </returns>
		public string GetNameForTaskById (string id)
		{
			ITask task = GetTaskById (id);
			return task != null ? task.Name : string.Empty;
		}
		
		/// <summary>
		/// Gets the category of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> the category of the task
		/// </returns>
		public string GetCategoryForTaskById (string id)
		{
			ITask task = GetTaskById (id);
			return task != null ? task.Category.Name : string.Empty;
		}
		
		/// <summary>
		/// Gets the state of a task for a given ID
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/> the state of the task
		/// </returns>
		public int GetStateForTaskById (string id)
		{
			ITask task = GetTaskById (id);
			return task != null ? (int) task.State : -1;
		}

		/// <summary>
		/// Marks a task complete
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		public void MarkTaskAsCompleteById (string id)
		{
			ITask task = GetTaskById (id);
			if (task == null)
				return;
				
			if (task.State == TaskState.Active) {
				// Complete immediately; no timeout or fancy
				// GUI stuff.
				task.Complete ();
			}
		}
		
		/// <summary>
		/// Looks up a task by ID in the backend
		/// </summary>
		/// <param name="id">
		/// A <see cref="System.String"/> for the ID of the task
		/// </param>
		/// <returns>
		/// A <see cref="ITask"/> having the given ID
		/// </returns>
		private ITask GetTaskById (string id)
		{
			Gtk.TreeIter  iter;
			Gtk.TreeModel model;
			
			ITask task = null;
			model = Application.Backend.Tasks;
			
			if (model.GetIterFirst (out iter)) {
				do {
					task = model.GetValue (iter, 0) as ITask;
					if (task.Id.Equals (id)) {
						return task;
					}
				} while (model.IterNext (ref iter));
			}			
			
			return task;
		}
	}
}
