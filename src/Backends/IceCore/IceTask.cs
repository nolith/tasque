// IceTask.cs created with MonoDevelop
// User: boyd at 8:33 AM 2/14/2008

using System;
using System.Collections.Generic;
using Novell.IceDesktop;

namespace Tasque.Backends.IceCore
{
	public class IceTask : AbstractTask
	{
		private IceBackend backend;
		private IceCategory category;
		private TaskEntry entry;
		private TaskState state;
		private string id;
		
		private static string UtcDateFormat = "yyyy-MM-ddTHH:mm:ssZ";
		
		public IceTask (IceBackend iceBackend,
						IceCategory iceCategory,
						TaskEntry taskEntry)
		{
			this.backend = iceBackend;
			this.category = iceCategory;
			this.entry = taskEntry;
			if (entry.Status == TaskStatus.Completed)
				this.state = TaskState.Completed;
			else if (entry.Status == TaskStatus.Cancelled)
				this.state = TaskState.Inactive; // TODO: Is this the right thing to do?
			else
				this.state = TaskState.Active;
			
			// Construct a unique id in the format of
			// <team-id>-<team-folder-id>-<task-entry-id>
			id = string.Format ("{0}-{1}-{2}",
								iceCategory.Team.ID,
								iceCategory.Folder.ID,
								taskEntry.ID);
		}

		#region Properties
		public TaskEntry Entry
		{
			get { return entry; }
		}
		
		/// <value>
		/// A unique ID in the format of
		/// <team-id>-<team-folder-id>-<task-entry-id>
		/// </value>
		public override string Id
		{
			get { return id; }
		}
		
		/// <value>
		/// A Task's Name will be used to show the task in the main list window.
		/// </value>
		public override string Name
		{
			get { return entry.Title; }
			set {
				if (value == null)
					entry.Title = string.Empty;
				else
					entry.Title = value.Trim ();
				
				backend.SaveAndUpdateTask (this);
			}
		}
		
		/// <value>
		/// A DueDate of DateTime.MinValue indicates that a due date is not set.
		/// </value>
		public override DateTime DueDate
		{
			get {
				string dateString = entry.DueDate;
				if (dateString == null || dateString == string.Empty)
					return DateTime.MinValue;
				else
					return DateTime.Parse (dateString);
			}
			set {
				// Set the date using UTC format
				entry.DueDate = value.ToUniversalTime ().ToString (UtcDateFormat);
				backend.SaveAndUpdateTask (this);
			}
		}
		
		public string DueDateString
		{
			get { return DueDate.ToUniversalTime ().ToString (UtcDateFormat); }
		}
		
		/// <value>
		/// If set to CompletionDate.MinValue, the task has not been completed.
		/// </value>
		public override DateTime CompletionDate
		{
			get {
				string dateString = entry.CompletionDate;
				if (dateString == null || dateString == string.Empty)
					return DateTime.MinValue;
				else
					return DateTime.Parse (dateString);
			}
			set {
				// Set the date using UTC format
				entry.CompletionDate = value.ToUniversalTime ().ToString (UtcDateFormat);
				backend.SaveAndUpdateTask (this);
			}
		}
		
		/// <value>
		/// This is a convenience property which should use the CompletionDate
		/// to determine whether a task is completed.
		/// </value>
		public override bool IsComplete 
		{
			get { return state == TaskState.Completed; }
		}
		
		/// <value>
		/// Backends should, by default, set the priority of a task to
		/// TaskPriority.None.
		///
		/// ICEcore uses 1 = critical, 2 = high, 3 = medium, 4 = low, 5 = not
		/// important.  We'll map 1 -> 2, 2 = TaskPriority.High,
		/// 3 = TaskPriority.Medium, 4 = TaskPriority.Low, and
		/// 5 = TaskPriority.None.
		/// </value>
		public override TaskPriority Priority
		{
			get {
				string priorityString = entry.Priority;
				if (priorityString == null || priorityString == string.Empty)
					return TaskPriority.None;
				
				switch (priorityString) {
				case "1":
					return TaskPriority.High;
				case "2":
					return TaskPriority.Medium;
				case "3":
				case "4":
					return TaskPriority.Low;
				}
				
				return TaskPriority.None;
			}
			set {
				switch (value) {
				case TaskPriority.High:
					entry.Priority = "1";
					break;
				case TaskPriority.Medium:
					entry.Priority = "2";
					break;
				case TaskPriority.Low:
					entry.Priority = "3";
					break;
				default:
					entry.Priority = "5";
					break;
				}
				
				backend.SaveAndUpdateTask (this);
			}
		}
		
		/// <value>
		/// Indicates whether any notes exist in this task.  If a backend does
		/// not support notes, it should always return false.
		///
		/// ICEcore task descriptions will be exposed in Tasque as a Task Note.
		/// </value>
		public override bool HasNotes
		{
			get {
				string description = entry.Description;
				if (description == null || description.Trim () == string.Empty)
					return false;
				
				return true;
			}
		}
		
		/// <value>
		/// Should be true if the task supports having multiple notes.
		/// </value>
		public override bool SupportsMultipleNotes
		{
			get { return false; }
		}
		
		/// <value>
		/// The state of the task.  Note: sreeves' LOVES source code comments
		/// like these.
		/// </value>
		public override TaskState State
		{
			get { return state; }
		}
		
		public Novell.IceDesktop.TaskStatus IceDesktopStatus
		{
			get {
				switch (state) {
				case TaskState.Deleted:
					return TaskStatus.Cancelled;
				case TaskState.Completed:
					return TaskStatus.Completed;
				}
				
				return TaskStatus.InProcess;
			}
		}
		
		/// <value>
		/// The category to which this task belongs
		/// </value>
		public override ICategory Category
		{
			get { return category; } 
			set {}
		}
		
		/// <value>
		/// The notes associated with this task
		/// </value>
		public override List<INote> Notes
		{
			get {
				List<INote> notes = new List<INote> ();
				notes.Add (new IceNote (backend, this));
				return notes;
			}
		}
		
		#endregion // Properties
		
		#region Methods
		
		/// <summary>
		/// Activate (Reopen) a task that's Inactivated or Completed.
		/// </summary>
		public override void Activate ()
		{
			if (entry.Status != TaskStatus.Cancelled
					&& entry.Status != TaskStatus.Completed)
				return;
			
			entry.Status = TaskStatus.NeedsAction;
			CompletionDate = DateTime.MinValue;
			state = TaskState.Active;
			
			backend.SaveAndUpdateTask (this);
		}
		
		/// <summary>
		/// Inactivate a task (this is the "limbo" mode).
		/// </summary>
		public override void Inactivate ()
		{
			state = TaskState.Inactive;
			backend.UpdateTask (this);
		}
		
		/// <summary>
		/// Mark a task as completed.
		/// </summary>
		public override void Complete ()
		{
			entry.Status = TaskStatus.Completed;
			CompletionDate = DateTime.Now;
			state = TaskState.Completed;
			backend.SaveAndUpdateTask (this);
		}
		
		/// <summary>
		/// Delete a task from the backend.
		/// </summary>
		public override void Delete ()
		{
			// TODO: Implement IceTask.Delete ()
		}
		
		/// <summary>
		/// Creates a new note on this task
		/// </summary>
		public override INote CreateNote(string text)
		{
			// TODO: Implement IceTask.CreateNote()
			return null;
		}
		
		/// <summary>
		/// Removes a note from this task
		/// </summary>
		public override void DeleteNote(INote note)
		{
			// TODO: Implement IceTask.DeleteNote ()
		}
		
		/// <summary>
		/// Updates an exising note on the task
		/// </summary>
		public override void SaveNote(INote note)
		{
			// TODO: Implement IceTask.SaveNote ()
		}
		#endregion // Methods
	}
}
