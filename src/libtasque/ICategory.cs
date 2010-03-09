// ICategory.cs created with MonoDevelop
// User: boyd at 9:04 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;

namespace Tasque
{
	public interface ICategory
	{
		string Name
		{
			get;
		}
		
		bool ContainsTask(ITask task);
	}
}
