
using System;

using Gtk;

namespace Tasque
{
	public static class TaskGroupModelFactory
	{
		public static TaskGroupModel CreateTodayModel (TreeModel tasks)
		{
			DateTime rangeStart = DateTime.Now;
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			DateTime rangeEnd = DateTime.Now;
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}

		public static TaskGroupModel CreateOverdueModel (TreeModel tasks)
		{
			DateTime rangeStart = DateTime.MinValue;
			DateTime rangeEnd = DateTime.Now.AddDays (-1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month, rangeEnd.Day,
									 23, 59, 59);

			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}

		public static TaskGroupModel CreateTomorrowModel (TreeModel tasks)
		{
			DateTime rangeStart = DateTime.Now.AddDays (1);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			DateTime rangeEnd = DateTime.Now.AddDays (1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);

			return new TaskGroupModel (rangeStart, rangeEnd, tasks);
		}
	}
}
