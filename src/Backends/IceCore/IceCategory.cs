// IceCategory.cs created with MonoDevelop
// User: boyd at 8:32 AM 2/14/2008

using System;
using Novell.IceDesktop;

namespace Tasque.Backends.IceCore
{
	public class IceCategory : ICategory
	{
		private Teamspace team;
		private TaskFolder folder;
		private string id;
		
		public IceCategory(Teamspace iceTeam, TaskFolder iceFolder)
		{
			team = iceTeam;
			folder = iceFolder;
			
			// Construct a unique ID in the format:
			// <team-id>-<task-folder-id>
			id = string.Format ("{0}-{1}",
								team.ID,
								folder.ID);
		}
		
		public string Name
		{
			get { return team.Name; }
		}
		
		public bool ContainsTask(ITask task)
		{
			if (task == null)
				return false;
			
			IceCategory iceCategory = task.Category as IceCategory;
			if (iceCategory == null)
				return false;
			
			if (Id.CompareTo (iceCategory.Id) == 0)
				return true;
			
			return false;
		}
		
		public string Id
		{
			get { return id; }
		}
		
		public Teamspace Team
		{
			get { return team; }
		}
		
		public TaskFolder Folder
		{
			get { return folder; }
		}
	}
}
