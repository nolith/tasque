
using System;

using Gtk;

namespace Tasque
{
	public class TaskGroupModel : TreeModelFilter
	{
		public bool ShowCompletedTasks
		{
			get { return showCompletedTasks; }
			set {
				showCompletedTasks = value;
				base.Refilter ();
			}
		}

		public DateTime TimeRangeStart
		{
			get { return timeRangeStart; }
		}

		public DateTime TimeRangeEnd
		{
			get { return timeRangeEnd; }
		}
		
		public TaskGroupModel (DateTime rangeStart, DateTime rangeEnd,
		                       Gtk.TreeModel tasks) : base (tasks, null)
		{
			this.timeRangeStart = rangeStart;
			this.timeRangeEnd = rangeEnd;

			base.VisibleFunc = FilterTasks;
		}

		public void SetRange (DateTime rangeStart, DateTime rangeEnd)
		{
			this.timeRangeStart = rangeStart;
			this.timeRangeEnd = rangeEnd;
			base.Refilter ();
		}

		/// <summary>
	        /// Filter out tasks that don't fit within the group's date range
	        /// </summary>
		protected virtual bool FilterTasks (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			ITask task = model.GetValue (iter, 0) as ITask;
			if (task == null || task.State == TaskState.Deleted)
				return false;
			
			// Do something special when task.DueDate == DateTime.MinValue since
			// these tasks should always be in the very last category.
			if (task.DueDate == DateTime.MinValue) {
				if (timeRangeEnd == DateTime.MaxValue) {
					if (!ShowCompletedTask (task))
						return false;
					
					return true;
				} else {
					return false;
				}
			}
			
			if (task.DueDate < timeRangeStart || task.DueDate > timeRangeEnd)
				return false;
			
			if (!ShowCompletedTask (task))
				return false;

			return true;
		}
		
		protected DateTime timeRangeStart;
		protected DateTime timeRangeEnd;
		protected bool showCompletedTasks = false;

		private bool ShowCompletedTask (ITask task)
		{
			if (task.State == TaskState.Completed) {
				if (!showCompletedTasks)
					return false;
				
				// Only show completed tasks that are from "Today".  Once it's
				// tomorrow, don't show completed tasks in this group and
				// instead, show them in the Completed Tasks Group.
				if (task.CompletionDate == DateTime.MinValue)
					return false; // Just in case
				
				if (!IsToday (task.CompletionDate))
					return false;
			}
			
			return true;
		}
		
		private bool IsToday (DateTime testDate)
		{
			DateTime today = DateTime.Now;
			if (today.Year != testDate.Year
					|| today.DayOfYear != testDate.DayOfYear)
				return false;
			
			return true;
		}
	}
}
