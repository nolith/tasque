// CompletedTaskGroup.cs created with MonoDevelop
// User: boyd at 12:34 AM 3/1/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Mono.Unix;

namespace Tasque
{
	public enum ShowCompletedRange : uint
	{
		Yesterday = 0,
		Last7Days,
		LastMonth,
		LastYear,
		All
	}
	
	public class CompletedTaskGroup : TaskGroup
	{
		ICategory selectedCategory;
		HScale rangeSlider;
		ShowCompletedRange currentRange;
		
		public CompletedTaskGroup (string groupName, DateTime rangeStart,
								   DateTime rangeEnd, Gtk.TreeModel tasks)
			: base (groupName, rangeStart, rangeEnd,
					new CompletedTasksSortModel(tasks))
		{
			// Don't hide this group when it's empty because then the range
			// slider won't appear and the user won't be able to customize the
			// range.
			this.HideWhenEmpty = false;
			
			selectedCategory = GetSelectedCategory ();
			Application.Preferences.SettingChanged += OnSelectedCategorySettingChanged;
			
			CreateRangeSlider ();
			UpdateDateRanges ();
		}
		
		/// <summary>
		/// Create and show a slider (HScale) that will allow the user to
		/// customize how far in the past to show completed items.
		/// </summary>
		private void CreateRangeSlider ()
		{
			// There are five (5) different values allowed here:
			// "Yesterday", "Last7Days", "LastMonth", "LastYear", or "All"
			// Create the slider with 5 distinct "stops"
			rangeSlider = new HScale (0, 4, 1);
			rangeSlider.SetIncrements (1, 1);
			rangeSlider.WidthRequest = 100;
			rangeSlider.DrawValue = true;
			
			// TODO: Set the initial value and range
			string rangeStr =
				Application.Preferences.Get (Preferences.CompletedTasksRange);
			if (rangeStr == null) {
				// Set a default value of All
				rangeStr = ShowCompletedRange.All.ToString ();
				Application.Preferences.Set (Preferences.CompletedTasksRange,
											 rangeStr);
			}
			
			currentRange = ParseRange (rangeStr);
			rangeSlider.Value = (double)currentRange;
			rangeSlider.FormatValue += OnFormatRangeSliderValue;
			rangeSlider.ValueChanged += OnRangeSliderChanged;
			rangeSlider.Show ();
			
			this.ExtraWidget = rangeSlider;
		}

		protected override TaskGroupModel CreateModel (DateTime rangeStart,
		                                               DateTime rangeEnd,
		                                               TreeModel tasks)
		{
			return new CompletedTaskGroupModel (rangeStart, rangeEnd, tasks);
		}
		
		protected void OnSelectedCategorySettingChanged (
								Preferences preferences,
								string settingKey)
		{
			if (settingKey.CompareTo (Preferences.SelectedCategoryKey) != 0)
				return;
			
			selectedCategory = GetSelectedCategory ();
			Refilter (selectedCategory);
		}
		
		protected ICategory GetSelectedCategory ()
		{
			ICategory foundCategory = null;
			
			string cat = Application.Preferences.Get (
							Preferences.SelectedCategoryKey);
			if (cat != null) {
				TreeIter iter;
				TreeModel model = Application.Backend.Categories;
				
				if (model.GetIterFirst (out iter)) {
					do {
						ICategory category = model.GetValue (iter, 0) as ICategory;
						if (category.Name.CompareTo (cat) == 0) {
							foundCategory = category;
							break;
						}
					} while (model.IterNext (ref iter));
				}
			}
			
			return foundCategory;
		}
		
		private void OnRangeSliderChanged (object sender, EventArgs args)
		{
			ShowCompletedRange range = (ShowCompletedRange)(uint)rangeSlider.Value;
			
			// If the value is different than what we already have, adjust it in
			// the UI and set the preference.
			if (range == this.currentRange)
				return;
			
			this.currentRange = range;
			Application.Preferences.Set (Preferences.CompletedTasksRange,
										 range.ToString ());
			
			UpdateDateRanges ();
		}
		
		private void OnFormatRangeSliderValue (object sender,
											   FormatValueArgs args)
		{
			ShowCompletedRange range = (ShowCompletedRange)args.Value;
			args.RetVal = GetTranslatedRangeValue (range);
		}
		
		private ShowCompletedRange ParseRange (string rangeStr)
		{
			switch (rangeStr) {
			case "Yesterday":
				return ShowCompletedRange.Yesterday;
			case "Last7Days":
				return ShowCompletedRange.Last7Days;
			case "LastMonth":
				return ShowCompletedRange.LastMonth;
			case "LastYear":
				return ShowCompletedRange.LastYear;
			}
			
			// If the string doesn't match for some reason just return the
			// default, which is All.
			return ShowCompletedRange.All;
		}
		
		private string GetTranslatedRangeValue (ShowCompletedRange range)
		{
			switch (range) {
			case ShowCompletedRange.Yesterday:
				return Catalog.GetString ("Yesterday");
			case ShowCompletedRange.Last7Days:
				return Catalog.GetString ("Last 7 Days");
			case ShowCompletedRange.LastMonth:
				return Catalog.GetString ("Last Month");
			case ShowCompletedRange.LastYear:
				return Catalog.GetString ("Last Year");
			}
			
			return Catalog.GetString ("All");
		}
		
		private void UpdateDateRanges ()
		{
			DateTime date = DateTime.MinValue;
			DateTime today = DateTime.Now;
			
			switch (currentRange) {
			case ShowCompletedRange.Yesterday:
				date = today.AddDays (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case ShowCompletedRange.Last7Days:
				date = today.AddDays (-7);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case ShowCompletedRange.LastMonth:
				date = today.AddMonths (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			case ShowCompletedRange.LastYear:
				date = today.AddYears (-1);
				date = new DateTime (date.Year, date.Month, date.Day,
									 0, 0, 0);
				break;
			}
			
			this.TimeRangeStart = date;
		}
	}
	
	/// <summary>
	/// The purpose of this class is to allow the CompletedTaskGroup to show
	/// completed tasks in reverse order (i.e., most recently completed tasks
	/// at the top of the list).
	/// </summary>
	class CompletedTasksSortModel : Gtk.TreeModelSort
	{
		public CompletedTasksSortModel (Gtk.TreeModel childModel)
			: base (childModel)
		{
			SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareTasksSortFunc));
			SetSortColumnId (0, Gtk.SortType.Descending);
		}
		
		#region Private Methods
		static int CompareTasksSortFunc (Gtk.TreeModel model,
										 Gtk.TreeIter a,
										 Gtk.TreeIter b)
		{
			ITask taskA = model.GetValue (a, 0) as ITask;
			ITask taskB = model.GetValue (b, 0) as ITask;
			
			if (taskA == null || taskB == null)
				return 0;
			
			// Reverse the logic with the '!' so it's in re
			return (taskA.CompareToByCompletionDate (taskB));
		}
		#endregion // Private Methods
	}
}
