// DummyCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;

namespace Tasque.Backends.Dummy
{
	public class DummyCategory : ICategory
	{
		private string name;
		
		public DummyCategory (string name)
		{
			this.name = name;
		}
		
		public string Name
		{
			get {
				return name;
			}
		}

		public bool ContainsTask(ITask task)
		{
			if(task.Category is DummyCategory)
				return (task.Category.Name.CompareTo(name) == 0);
			else
				return false;
		}
		
	}
}
