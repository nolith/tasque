// AllCategory.cs created with MonoDevelop
// User: boyd at 3:45 PM 2/12/2008

using System;
using System.Collections.Generic;
using Mono.Unix;

namespace Tasque
{
	public class AllCategory : ICategory
	{
		// A "set" of categories specified by the user to show when the "All"
		// category is selected in the TaskWindow.  If the list is empty, tasks
		// from all categories will be shown.  Otherwise, only tasks from the
		// specified lists will be shown.
		List<string> categoriesToHide;
		
		public AllCategory ()
		{
			Preferences preferences = Application.Preferences;
			categoriesToHide =
				preferences.GetStringList (Preferences.HideInAllCategory);
			Application.Preferences.SettingChanged += OnSettingChanged;
		}
		
		public string Name
		{
			get { return Catalog.GetString ("All"); }
		}
		
		public bool ContainsTask(ITask task)
		{
			// Filter out tasks based on the user's preferences of which
			// categories should be displayed in the AllCategory.
			ICategory category = task.Category;
			if (category == null)
				return true;
			
			//if (categoriesToHide.Count == 0)
			//	return true;
			
			return (!categoriesToHide.Contains (category.Name));
		}
		
		private void OnSettingChanged (Preferences preferences, string settingKey)
		{
			if (settingKey.CompareTo (Preferences.HideInAllCategory) != 0)
				return;
			
			categoriesToHide =
				preferences.GetStringList (Preferences.HideInAllCategory);
		}
	}
}
