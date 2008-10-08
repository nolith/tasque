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
			
			ITask task = null;
			try {
				task = Application.Backend.CreateTask (taskName, category);
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
			
			// TODO: Add ITask.Id and return the new Id of the task.
			//return task.Id;
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
	}
}
