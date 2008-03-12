// RtmCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;
using RtmNet;

namespace Tasque.Backends.RtmBackend
{
	public class RtmCategory : ICategory
	{
		private List list;
		private Gtk.TreeIter iter;

		public RtmCategory(List list)
		{
			this.list = list;
		}
		
		public string Name
		{
			get { return list.Name; }
		}

		public string ID
		{
			get { return list.ID; }
		}
    
		public int Deleted
		{
			get { return list.Deleted; }
		}

		public int Locked
		{
			get { return list.Locked; }
		}
    
		public int Archived
		{
			get { return list.Archived; }
		}

		public int Position
		{
			get { return list.Position; }
		}

		public int Smart
		{
			get { return list.Smart; }
		}
		
		public Gtk.TreeIter Iter
		{
			get { return iter; }
			set { iter = value; }
		}

		public bool ContainsTask(ITask task)
		{
			if(task.Category is RtmCategory)
				return ((task.Category as RtmCategory).ID.CompareTo(ID) == 0);
			else
				return false;
		}

	}
}
