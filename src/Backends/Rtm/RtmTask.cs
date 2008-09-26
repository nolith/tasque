// Task.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008

using System;
using RtmNet;
using System.Collections.Generic;

namespace Tasque.Backends.RtmBackend
{
	public class RtmTask : AbstractTask
	{
		private RtmBackend rtmBackend;
		private TaskState state;
		private RtmCategory category;
		private List<INote> notes;		
		
		TaskSeries taskSeries;
		
		/// <summary>
		/// Constructor that is created from an RTM Task Series
		/// </summary>
		/// <param name="taskSeries">
		/// A <see cref="TaskSeries"/>
		/// </param>
		public RtmTask(TaskSeries taskSeries, RtmBackend be, string listID)
		{
			this.taskSeries = taskSeries;
			this.rtmBackend = be;
			this.category = be.GetCategory(listID);
			
			if(CompletionDate == DateTime.MinValue )
				state = TaskState.Active;
			else
				state = TaskState.Completed;
			notes = new List<INote>();

			if (taskSeries.Notes.NoteCollection != null) {
				foreach(Note note in taskSeries.Notes.NoteCollection) {
					RtmNote rtmNote = new RtmNote(note);
					notes.Add(rtmNote);
				}
			}
		}
		
		#region Public Properties
		/// <value>
		/// Gets the id of the task
		/// </value>
		public override string Id
		{
			get { return taskSeries.Task.TaskID; } 
		}

		/// <value>
		/// Holds the name of the task
		/// </value>		
		public override string Name
		{
			get { return taskSeries.Name; }
			set {
				if (value != null) {
					taskSeries.Name = value.Trim ();
					rtmBackend.UpdateTaskName(this);
				}
			}
		}
		
		/// <value>
		/// Due Date for the task
		/// </value>
		public override DateTime DueDate
		{
			get { return taskSeries.Task.Due; }
			set { 
				taskSeries.Task.Due = value;
				rtmBackend.UpdateTaskDueDate(this);			
			}
		}
		

		/// <value>
		/// Due Date for the task
		/// </value>
		public string DueDateString
		{
			get {
				// Return the due date in UTC format
				string format = "yyyy-MM-ddTHH:mm:ssZ";
				string dateString = taskSeries.Task.Due.ToUniversalTime ().ToString (format);
				return dateString;
			}
		}

		
		/// <value>
		/// Completion Date for the task
		/// </value>
		public override DateTime CompletionDate
		{
			get { return taskSeries.Task.Completed; }
			set { 
				taskSeries.Task.Completed = value;
				rtmBackend.UpdateTaskCompleteDate(this);
			}
		}
		
		/// <value>
		/// Returns if the task is complete
		/// </value>
		public override bool IsComplete
		{
			get { return state == TaskState.Completed; }
		}
		
		/// <value>
		/// Holds the priority of the task
		/// </value>
		public override TaskPriority Priority
		{
			get { 
				switch (taskSeries.Task.Priority) {
					default:
					case "N":
						return TaskPriority.None;
					case "1":
						return TaskPriority.High;
					case "2":
						return TaskPriority.Medium;
					case "3":
						return TaskPriority.Low;
				}
			}
			set {
				switch (value) {
					default:
					case TaskPriority.None:
						taskSeries.Task.Priority = "N";
						break;
					case TaskPriority.High:
						taskSeries.Task.Priority = "1";
						break;
					case TaskPriority.Medium:
						taskSeries.Task.Priority = "2";
						break;
					case TaskPriority.Low:
						taskSeries.Task.Priority = "3";
						break;
				}
				rtmBackend.UpdateTaskPriority(this);				
			}
		}
		
		public string PriorityString
		{
			get { return taskSeries.Task.Priority; }
		}		
		
		
		/// <value>
		/// Returns if the task has any notes
		/// </value>
		public override bool HasNotes
		{
			get { return (notes.Count > 0); }
		}
		
		/// <value>
		/// Returns if the task supports multiple notes
		/// </value>
		public override bool SupportsMultipleNotes
		{
			get { return true; }
		}
		
		/// <value>
		/// Holds the current state of the task
		/// </value>
		public override TaskState State
		{
			get { return state; }
		}
		
		/// <value>
		/// Returns the category object for this task
		/// </value>
		public override ICategory Category
		{
			get { return category; } 
			set {
				RtmCategory rtmCategory = value as RtmCategory;
				rtmBackend.MoveTaskCategory(this, rtmCategory.ID);				
			}
		}
		
		/// <value>
		/// Returns the notes associates with this task
		/// </value>
		public override List<INote> Notes
		{
			get { return notes; }
		}
		
		/// <value>
		/// Holds the current RtmBackend for this task
		/// </value>
		public RtmBackend RtmBackend
		{
			get { return this.rtmBackend; }
		}
		
		public string ID
		{
			get {return taskSeries.TaskID; }
		}
		
		public string SeriesTaskID
		{
			get { return taskSeries.TaskID; }
		}
		
		public string TaskTaskID
		{
			get { return taskSeries.Task.TaskID; }
		}
		
		public string ListID
		{
			get { return category.ID; }
		}
		#endregion // Public Properties
		
		#region Public Methods
		/// <summary>
		/// Activates the task
		/// </summary>
		public override void Activate ()
		{
			Logger.Debug("Activating Task: " + Name);
			state = TaskState.Active;
			taskSeries.Task.Completed = DateTime.MinValue;
			rtmBackend.UpdateTaskActive(this);
		}
		
		/// <summary>
		/// Sets the task to be inactive
		/// </summary>
		public override void Inactivate ()
		{
			Logger.Debug("Inactivating Task: " + Name);		
			state = TaskState.Inactive;
			taskSeries.Task.Completed = DateTime.Now;
			rtmBackend.UpdateTaskInactive(this);
		}
		
		/// <summary>
		/// Completes the task
		/// </summary>
		public override void Complete ()
		{
			Logger.Debug("Completing Task: " + Name);			
			state = TaskState.Completed;
			if(taskSeries.Task.Completed == DateTime.MinValue)
				taskSeries.Task.Completed = DateTime.Now;
			rtmBackend.UpdateTaskCompleted(this);
		}
		
		/// <summary>
		/// Deletes the task
		/// </summary>
		public override void Delete ()
		{
			state = TaskState.Deleted;
			rtmBackend.UpdateTaskDeleted(this);
		}
		
		/// <summary>
		/// Adds a note to a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override INote CreateNote(string text)
		{
			RtmNote rtmNote;
			
			rtmNote = rtmBackend.CreateNote(this, text);
			notes.Add(rtmNote);
			
			return rtmNote;
		}
		
		/// <summary>
		/// Deletes a note from a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override void DeleteNote(INote note)
		{
			RtmNote rtmNote = (note as RtmNote);
			
			foreach(RtmNote lRtmNote in notes) {
				if(lRtmNote.ID == rtmNote.ID) {
					notes.Remove(lRtmNote);
					break;
				}
			}
			rtmBackend.DeleteNote(this, rtmNote);
		}		

		/// <summary>
		/// Deletes a note from a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override void SaveNote(INote note)
		{		
			rtmBackend.SaveNote(this, (note as RtmNote));
		}		

		#endregion // Public Methods
	}
}
