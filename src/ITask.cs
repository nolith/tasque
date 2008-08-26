// ITask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008

using System;
using System.Collections.Generic;

namespace Tasque
{
	public interface ITask
	{
		#region Properties
		/// <value>
		/// A unique identifier for the task
		/// </value>
		string Id
		{
			get; 
		}

		/// <value>
		/// A Task's Name will be used to show the task in the main list window.
		/// </value>
		string Name
		{
			get;
			set;
		}
		
		/// <value>
		/// A DueDate of DateTime.MinValue indicates that a due date is not set.
		/// </value>
		DateTime DueDate
		{
			get;
			set;
		}
		
		/// <value>
		/// If set to CompletionDate.MinValue, the task has not been completed.
		/// </value>
		DateTime CompletionDate
		{
			get;
			set;
		}
		
		/// <value>
		/// This is a convenience property which should use the CompletionDate
		/// to determine whether a task is completed.
		/// </value>
		bool IsComplete 
		{
			get;
		}
		
		/// <value>
		/// Backends should, by default, set the priority of a task to
		/// TaskPriority.None.
		/// </value>
		TaskPriority Priority
		{
			get;
			set;
		}
		
		/// <value>
		/// Indicates whether any notes exist in this task.  If a backend does
		/// not support notes, it should always return false.
		/// </value>
		bool HasNotes
		{
			get;
		}
		
		/// <value>
		/// Should be true if the task supports having multiple notes.
		/// </value>
		bool SupportsMultipleNotes
		{
			get;
		}
		
		/// <value>
		/// The state of the task.  Note: sreeves' LOVES source code comments
		/// like these.
		/// </value>
		TaskState State
		{
			get;
		}
		
		/// <value>
		/// The category to which this task belongs
		/// </value>
		ICategory Category
		{
			get; 
			set;
		}
		
		/// <value>
		/// The notes associated with this task
		/// </value>
		List<INote> Notes
		{
			get;
		}
		
		/// <value>
		/// The ID of the timer used to complete a task after being marked
		/// inactive.
		/// </value>
		uint TimerID
		{
			get;
			set;
		}	

		#endregion // Properties
		
		#region Methods
		
		/// <summary>
		/// Activate (Reopen) a task that's Inactivated or Completed.
		/// </summary>
		void Activate ();
		
		/// <summary>
		/// Inactivate a task (this is the "limbo" mode).
		/// </summary>
		void Inactivate ();
		
		/// <summary>
		/// Mark a task as completed.
		/// </summary>
		void Complete ();
		
		/// <summary>
		/// Delete a task from the backend.
		/// </summary>
		void Delete ();
		
		/// <summary>
		/// Creates a new note on this task
		/// </summary>
		INote CreateNote(string text);
		
		/// <summary>
		/// Removes a note from this task
		/// </summary>
		void DeleteNote(INote note);
		
		/// <summary>
		/// Updates an exising note on the task
		/// </summary>
		void SaveNote(INote note);
		
		/// <summary>
		/// This is used for sorting tasks in the TaskWindow and should compare
		/// based on due date.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		int CompareTo (ITask task);
		
		/// <summary>
		/// This is the same as CompareTo above but should use completion date
		/// instead of due date.  This is used to sort items in the
		/// CompletedTaskGroup.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		int CompareToByCompletionDate (ITask task);
		#endregion // Methods
	}
}
