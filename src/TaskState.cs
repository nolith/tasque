// TaskState.cs created with MonoDevelop
// User: boyd at 8:37 AM 2/12/2008

using System;

namespace Tasque
{
	public enum TaskState
	{
		/// <summary>
		/// A task that has not been completed.
		/// </summary>
		Active,
		
		/// <summary>
		/// A task that's in limbo...the user has clicked that it should be
		/// completed, but we're delaying so the user can get a visual of what's
		/// gonna happen.  This feature ROCKS!
		/// </summary>
		Inactive,
		
		/// <summary>
		/// A completed task.
		/// </summary>
		Completed,
		
		/// <summary>
		/// A tasks that's deleted.  This is used when tasks are cached locally.
		/// As soon as the task is actually deleted from the backend system, the
		/// task should really be deleted.
		/// </summary>
		Deleted
	}
}
