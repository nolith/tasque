// DummyTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008

using System;
using Tasque;
using System.Collections.Generic;

namespace Tasque.Backends.Dummy
{
	public class DummyTask : AbstractTask
	{
		DummyBackend backend;
		string name;
		DateTime dueDate;
		DateTime completionDate;
		TaskPriority priority;
		TaskState state;
		int id;
		DummyCategory category;
		
		public DummyTask(DummyBackend backend, int id, string taskName)
		{
			this.backend = backend;
			this.id = id;
			this.name = taskName;
			this.dueDate = DateTime.MinValue; // No due date specified
			this.completionDate = DateTime.MinValue; // No due date specified
			this.priority = TaskPriority.None;
			this.state = TaskState.Active;
		}
		
		#region Public Properties
		
		public int DummyId
		{
			get { return id; }
			set { id = value; }
		}

		public override string Id
		{
			get { return id.ToString(); }
		}

		
		public override string Name
		{
			get { return name; }
			set {
Logger.Debug ("Setting new task name");
				if (value == null)
					name = string.Empty;
				
				name = value.Trim ();
				
				backend.UpdateTask (this);
			}
		}
		
		public override DateTime DueDate
		{
			get { return dueDate; }
			set {
Logger.Debug ("Setting new task due date");
				dueDate = value;
				
				backend.UpdateTask (this);
			}
		}
		
		public override DateTime CompletionDate
		{
			get { return completionDate; }
			set {
Logger.Debug ("Setting new task completion date");
				completionDate = value;
				
				backend.UpdateTask (this);
			}
		}
		
		public override bool IsComplete
		{
			get { return state == TaskState.Completed; }
		}
		
		public override TaskPriority Priority
		{
			get { return priority; }
			set {
Logger.Debug ("Setting new task priority");
				priority = value;
				
				backend.UpdateTask (this);
			}
		}

		public override bool HasNotes
		{
			get { return true; }
		}
		
		public override bool SupportsMultipleNotes
		{
			get { return true; }
		}
		
		public override TaskState State
		{
			get { return state; }
		}
		
		public override ICategory Category
		{
			get { return category; } 
			set {
				category = value as DummyCategory;
			}
		}
		
		public override List<INote> Notes
		{
			get { return null; }
		}		
		
		#endregion // Public Properties
		
		#region Public Methods
		public override void Activate ()
		{
Logger.Debug ("DummyTask.Activate ()");
			completionDate = DateTime.MinValue;
			state = TaskState.Active;
			backend.UpdateTask (this);
		}
		
		public override void Inactivate ()
		{
Logger.Debug ("DummyTask.Inactivate ()");
			completionDate = DateTime.Now;
			state = TaskState.Inactive;
			backend.UpdateTask (this);
		}
		
		public override void Complete ()
		{
			Logger.Debug ("DummyTask.Complete ()");
			CompletionDate = DateTime.Now;
			state = TaskState.Completed;
			backend.UpdateTask (this);
		}
		
		public override void Delete ()
		{
Logger.Debug ("DummyTask.Delete ()");
			state = TaskState.Deleted;
			backend.UpdateTask (this);
		}
		
		public override INote CreateNote(string text)
		{
			return null;
		}
		
		public override void DeleteNote(INote note)
		{
		}

		public override void SaveNote(INote note)
		{
		}

		#endregion // Public Methods
	}
}
