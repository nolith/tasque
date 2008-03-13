/***************************************************************************
 *  TargetWindow.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by:
 *		Calvin Gaisford <calvinrg@gmail.com>
 *		Boyd Timothy <btimothy@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

using Tasque.Backends;

namespace Tasque
{
	public class TaskWindow : Gtk.Window 
	{
		private static TaskWindow taskWindow = null;
		private static int lastXPos;
		private static int lastYPos;
		private static Gdk.Pixbuf noteIcon;
		
		private IBackend backend;
		private ScrolledWindow scrolledWindow;
		
		private MenuToolButton addTaskButton;
		private Gtk.ComboBox categoryComboBox;
		private Gtk.VBox targetVBox;
		
		private TaskGroup overdueGroup;
		private TaskGroup todayGroup;
		private TaskGroup tomorrowGroup;
		private TaskGroup nextSevenDaysGroup;
		private TaskGroup futureGroup;
		private CompletedTaskGroup completedTaskGroup;
		
		private List<TaskGroup> taskGroups;
		
		private Dictionary<ITask, NoteDialog> noteDialogs;
		
		private Gtk.Statusbar statusbar;
		private uint statusContext;
		private uint currentStatusMessageId;
		
		private ITask clickedTask;
		
		private Gtk.AccelGroup accelGroup;
		private GlobalKeybinder globalKeys;
		
		static TaskWindow ()
		{
			noteIcon = Utilities.GetIcon ("note", 16);
		}
		
		public TaskWindow (IBackend aBackend) : base (WindowType.Toplevel)
		{
			this.backend = aBackend;
			taskGroups = new List<TaskGroup> ();
			noteDialogs = new Dictionary<ITask, NoteDialog> ();
			InitWindow();
		}

		void InitWindow()
		{
			int height;
			int width;
			
			this.Icon = Utilities.GetIcon ("tasque-48", 48);
			// Update the window title
			Title = string.Format ("Tasque");	

			width = Application.Preferences.GetInt("MainWindowWidth");
			height = Application.Preferences.GetInt("MainWindowHeight");
			
			if(width == -1)
				width = 600;
			if(height == -1)
				height = 600;
			
			this.DefaultSize = new Gdk.Size( width, height);
			
			accelGroup = new AccelGroup ();
			AddAccelGroup (accelGroup);
			globalKeys = new GlobalKeybinder (accelGroup);

			// Start with an event box to paint the background white
			EventBox eb = new EventBox();
			eb.BorderWidth = 0;
			eb.ModifyBg(	StateType.Normal, 
					new Gdk.Color(255,255,255));
			eb.ModifyBase(	StateType.Normal, 
					new Gdk.Color(255,255,255));
			
			VBox mainVBox = new VBox();
			mainVBox.BorderWidth = 0;
			mainVBox.Show ();
			eb.Add(mainVBox);
			this.Add (eb);
			
			HBox topHBox = new HBox (false, 0);
			topHBox.BorderWidth = 4;
			
			categoryComboBox = new ComboBox ();
			categoryComboBox.WrapWidth = 1;
			CellRendererText comboBoxRenderer = new Gtk.CellRendererText ();
			categoryComboBox.PackStart (comboBoxRenderer, true);
			categoryComboBox.SetCellDataFunc (comboBoxRenderer,
				new Gtk.CellLayoutDataFunc (CategoryComboBoxDataFunc));
			
			categoryComboBox.Show ();
			topHBox.PackStart (categoryComboBox, false, false, 0);
			
			// Space the addTaskButton and the categoryComboBox
			// far apart by using a blank label that expands
			Label spacer = new Label (string.Empty);
			spacer.Show ();
			topHBox.PackStart (spacer, true, true, 0);
			
			// Use a small add icon so the button isn't mammoth-sized
			HBox buttonHBox = new HBox (false, 6);
			Image addImage = new Image (Gtk.Stock.Add, IconSize.Menu);
			addImage.Show ();
			buttonHBox.PackStart (addImage, false, false, 0);
			Label l = new Label (Catalog.GetString ("_Add Task"));
			l.Show ();
			buttonHBox.PackStart (l, true, true, 0);
			buttonHBox.Show ();
			addTaskButton =
				new MenuToolButton (buttonHBox, Catalog.GetString ("_Add Task"));
			addTaskButton.UseUnderline = true;
			// Disactivate the button until the backend is initialized
			addTaskButton.Sensitive = false;
			Gtk.Menu addTaskMenu = new Gtk.Menu ();
			addTaskButton.Menu = addTaskMenu;
			addTaskButton.Clicked += OnAddTask;
			addTaskButton.Show ();
			topHBox.PackStart (addTaskButton, false, false, 0);
			
			globalKeys.AddAccelerator (OnAddTask,
			                           (uint) Gdk.Key.n,
			                           Gdk.ModifierType.ControlMask,
			                           Gtk.AccelFlags.Visible);
			
			topHBox.Show ();
			mainVBox.PackStart (topHBox, false, false, 0);

			scrolledWindow = new ScrolledWindow ();
			scrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
			scrolledWindow.HscrollbarPolicy = PolicyType.Never;

			scrolledWindow.BorderWidth = 0;
			scrolledWindow.CanFocus = true;
			scrolledWindow.Show ();
			mainVBox.PackStart (scrolledWindow, true, true, 0);

			EventBox innerEb = new EventBox();
			innerEb.BorderWidth = 0;
			innerEb.ModifyBg(	StateType.Normal, 
						new Gdk.Color(255,255,255));
			innerEb.ModifyBase(	StateType.Normal, 
						new Gdk.Color(255,255,255));

			targetVBox = new VBox();
			targetVBox.BorderWidth = 5;
			targetVBox.Show ();
			innerEb.Add(targetVBox);

			scrolledWindow.AddWithViewport(innerEb);
			
			statusbar = new Gtk.Statusbar ();
			statusContext = statusbar.GetContextId ("tasque-statusbar");
			currentStatusMessageId =
				statusbar.Push (statusContext,
								Catalog.GetString ("Loading tasks..."));
			
			statusbar.HasResizeGrip = true;
			
			statusbar.Show ();

			mainVBox.PackEnd (statusbar, false, false, 0);
			
			//
			// Delay adding in the TaskGroups until the backend is initialized
			//
			
			Shown += OnWindowShown;
			DeleteEvent += WindowDeleted;
			
			backend.BackendInitialized += OnBackendInitialized;
			backend.BackendSyncStarted += OnBackendSyncStarted;
			backend.BackendSyncFinished += OnBackendSyncFinished;
			// if the backend is already initialized, go ahead... initialize
			if(backend.Initialized) {
				OnBackendInitialized();
			}
			
			Application.Preferences.SettingChanged += OnSettingChanged;
		}


		void PopulateWindow()
		{
			

			// Add in the groups
			
			//
			// Overdue Group
			//
			DateTime rangeStart;
			DateTime rangeEnd;
			
			rangeStart = DateTime.MinValue;
			rangeEnd = DateTime.Now.AddDays (-1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month, rangeEnd.Day,
									 23, 59, 59);
			
			overdueGroup = new TaskGroup (Catalog.GetString ("Overdue"),
										  rangeStart, rangeEnd,
										  backend.Tasks);
			overdueGroup.RowActivated += OnRowActivated;
			overdueGroup.ButtonPressed += OnButtonPressed;
			overdueGroup.Show ();
			targetVBox.PackStart (overdueGroup, false, false, 0);
			taskGroups.Add(overdueGroup);
			
			//
			// Today Group
			//
			rangeStart = DateTime.Now;
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now;
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			todayGroup = new TaskGroup (Catalog.GetString ("Today"),
										rangeStart, rangeEnd,
										backend.Tasks);
			todayGroup.RowActivated += OnRowActivated;
			todayGroup.ButtonPressed += OnButtonPressed;
			todayGroup.Show ();
			targetVBox.PackStart (todayGroup, false, false, 0);
			taskGroups.Add (todayGroup);
			
			//
			// Tomorrow Group
			//
			rangeStart = DateTime.Now.AddDays (1);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now.AddDays (1);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			tomorrowGroup = new TaskGroup (Catalog.GetString ("Tomorrow"),
										   rangeStart, rangeEnd,
										   backend.Tasks);
			tomorrowGroup.RowActivated += OnRowActivated;
			tomorrowGroup.ButtonPressed += OnButtonPressed;			
			tomorrowGroup.Show ();
			targetVBox.PackStart (tomorrowGroup, false, false, 0);
			taskGroups.Add (tomorrowGroup);
			
			//
			// Next 7 Days Group
			//
			rangeStart = DateTime.Now.AddDays (2);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.Now.AddDays (6);
			rangeEnd = new DateTime (rangeEnd.Year, rangeEnd.Month,
									 rangeEnd.Day, 23, 59, 59);
			nextSevenDaysGroup = new TaskGroup (Catalog.GetString ("Next 7 Days"),
										   rangeStart, rangeEnd,
										   backend.Tasks);
			nextSevenDaysGroup.RowActivated += OnRowActivated;
			nextSevenDaysGroup.ButtonPressed += OnButtonPressed;				
			nextSevenDaysGroup.Show ();
			targetVBox.PackStart (nextSevenDaysGroup, false, false, 0);
			taskGroups.Add (nextSevenDaysGroup);
			
			//
			// Future Group
			//
			rangeStart = DateTime.Now.AddDays (7);
			rangeStart = new DateTime (rangeStart.Year, rangeStart.Month,
									   rangeStart.Day, 0, 0, 0);
			rangeEnd = DateTime.MaxValue;
			futureGroup = new TaskGroup (Catalog.GetString ("Future"),
										 rangeStart, rangeEnd,
										 backend.Tasks);
			futureGroup.RowActivated += OnRowActivated;
			futureGroup.ButtonPressed += OnButtonPressed;			
			futureGroup.Show ();
			targetVBox.PackStart (futureGroup, false, false, 0);
			taskGroups.Add (futureGroup);
			
			//
			// Completed Tasks Group
			//
			rangeStart = DateTime.MinValue;
			rangeEnd = DateTime.MaxValue;
			completedTaskGroup = new CompletedTaskGroup (
					Catalog.GetString ("Completed"),
					rangeStart, rangeEnd,
					backend.Tasks);
			completedTaskGroup.RowActivated += OnRowActivated;
			completedTaskGroup.ButtonPressed += OnButtonPressed;
			completedTaskGroup.Show ();
			targetVBox.PackStart (completedTaskGroup, false, false, 0);
			taskGroups.Add (completedTaskGroup);
			

			//manualTarget = new TargetService();
			//manualTarget.Show ();
			//mainVBox.PackStart(manualTarget, false, false, 0);
			
			
			// Set up the combo box (after the above to set the current filter)

			categoryComboBox.Model = Application.Backend.Categories;		

			// Read preferences for the last-selected category and select it
			string selectedCategoryName =
				Application.Preferences.Get (Preferences.SelectedCategoryKey);
			
			categoryComboBox.Changed += OnCategoryChanged;
			
			SelectCategory (selectedCategoryName);
		}

		
		#region Public Methods
		/// <summary>
		/// Method to allow other classes to "click" on the "Add Task" button.
		/// </summary>
		public static void AddTask ()
		{
			if (taskWindow == null)
				TaskWindow.ShowWindow ();
			
			taskWindow.OnAddTask (null, EventArgs.Empty);
		}

		public static void SavePosition()
		{
			if(taskWindow != null) {
				int x;
				int y;
				int width;
				int height;

				taskWindow.GetPosition(out x, out y);
				taskWindow.GetSize(out width, out height);

				lastXPos = x;
				lastYPos = y;
				
				Application.Preferences.SetInt("MainWindowLastXPos", lastXPos);
				Application.Preferences.SetInt("MainWindowLastYPos", lastYPos);
				Application.Preferences.SetInt("MainWindowWidth", width);
				Application.Preferences.SetInt("MainWindowHeight", height);	
			}

		}
		
		public static void ShowWindow()
		{
			if(taskWindow != null) {
				if(taskWindow.IsActive) {
					int x;
					int y;

					taskWindow.GetPosition(out x, out y);

					lastXPos = x;
					lastYPos = y;

					taskWindow.Hide();
				} else {
					if(!taskWindow.Visible) {
						int x = lastXPos;
						int y = lastYPos;

						if (x >= 0 && y >= 0)
							taskWindow.Move(x, y);						
					}
					taskWindow.Present();
				}
			} else {
				// TODO: Eventually move the creation of the IBackend into
				// something that will parse the command line to read which
				// backend should be used.  If no specific backend is specified,
				// use RtmTaskBackend by default.
				TaskWindow.taskWindow = new TaskWindow(Application.Backend);
				if(lastXPos == 0 || lastYPos == 0)
				{
					lastXPos = Application.Preferences.GetInt("MainWindowLastXPos");
					lastYPos = Application.Preferences.GetInt("MainWindowLastYPos");				
				}

				int x = lastXPos;
				int y = lastYPos;

				if (x >= 0 && y >= 0)
					taskWindow.Move(x, y);						

				taskWindow.ShowAll();
			}
		}
		
		public static void SelectAndEdit (ITask task)
		{
			ShowWindow ();
			taskWindow.EnterEditMode (task, true);
			taskWindow.Present ();
		}
		
		public static void ShowStatus (string statusText)
		{
			if (taskWindow == null) {
				Logger.Warn ("Cannot set status when taskWindow is null");
				return;
			}
			
			if (taskWindow.currentStatusMessageId != 0) {
				// Pop the old message
				taskWindow.statusbar.Remove (taskWindow.statusContext,
											 taskWindow.currentStatusMessageId);
				taskWindow.currentStatusMessageId = 0;
			}
			
			taskWindow.currentStatusMessageId =
				taskWindow.statusbar.Push (taskWindow.statusContext,
										   statusText);
		}
		
		/// <summary>
		/// This should be called after a new IBackend has been set
		/// </summary>
		public static void Reinitialize ()
		{
			if (TaskWindow.taskWindow != null) {
				TaskWindow.taskWindow.Hide ();
				TaskWindow.taskWindow.Destroy ();
				TaskWindow.taskWindow = null;
			}
			
			TaskWindow.ShowWindow ();
		}
		#endregion // Public Methods
		
		#region Private Methods
		void CategoryComboBoxDataFunc (Gtk.CellLayout layout,
									   Gtk.CellRenderer renderer,
									   Gtk.TreeModel model,
									   Gtk.TreeIter iter)
		{
			Gtk.CellRendererText crt = renderer as Gtk.CellRendererText;
			ICategory category = model.GetValue (iter, 0) as ICategory;

			// CRG: What?  I added this check for null and we don't crash
			// but I never see anything called unknown
			if(category != null && category.Name != null) {
				crt.Text =
					string.Format ("{0} ({1})",
								   category.Name,
								   GetTaskCountInCategory (category));
			} else
				crt.Text = "unknown";
		}
		
		// TODO: Move this method into a property of ICategory.TaskCount
		private int GetTaskCountInCategory (ICategory category)
		{
			// This is disgustingly inefficient, but, oh well
			int count = 0;
			
			Gtk.TreeIter iter;
			Gtk.TreeModel model = Application.Backend.Tasks;
			
			if (model.GetIterFirst (out iter) == false)
				return 0;
			
			do {
				ITask task = model.GetValue (iter, 0) as ITask;
				if (task == null)
					continue;
				if (task.State != TaskState.Active
						&& task.State != TaskState.Inactive)
					continue;
				
				if (category.ContainsTask (task))
					count++;
			} while (model.IterNext (ref iter) == true);
			
			return count;
		}
		
		
		/// <summary>
		/// Search through the TaskGroups looking for the specified task and:
		/// 1) scroll the window to its location, 2) enter directly into edit
		/// mode.  This method should be called right after a new task is
		/// created.
		/// </summary>
		/// <param name="task">
		/// A <see cref="ITask"/>
		/// </param>
		/// <param name="adjustScrolledWindow">
		/// A <see cref="bool"/> which indicates whether the task should be
		/// scrolled to.
		/// </param>
		private void EnterEditMode (ITask task, bool adjustScrolledWindow)
		{
			Gtk.TreeIter iter;
			
			// Make sure we've waited around for the new task to fully
			// be added to the TreeModel before continuing.  Some
			// backends might be threaded and will have used something
			// like Gtk.Idle.Add () to actually store the new Task in
			// their TreeModel.
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();

			int taskGroupHeights = 0;
			
			foreach (TaskGroup taskGroup in taskGroups) {
				
				//Logger.Debug("taskGroupHeights: {0}", taskGroupHeights);
				
				if (taskGroup.ContainsTask (task, out iter) == true) {
					Logger.Debug ("Found new task group: {0}", taskGroup.DisplayName);
					
					// Get the header height
					int headerHeight = taskGroup.HeaderHeight;
				
					// Get the total number items in the TaskGroup
					int nChildren = taskGroup.GetNChildren(iter);
					//Logger.Debug("n children: {0}", nChildren);

					// Calculate size of each item
					double itemSize = (double)(taskGroup.Requisition.Height-headerHeight) / nChildren;
					//Logger.Debug("item size: {0}", itemSize);
				
					// Get the index of the new item within the TaskGroup
					int newTaskIndex = taskGroup.GetIterIndex (iter);
					//Logger.Debug("new task index: {0}", newTaskIndex);
						
					// Calculate the scrolling distance
					double scrollDistance = (itemSize*newTaskIndex)+taskGroupHeights;
					//Logger.Debug("Scroll distance = ({0}*{1})+{2}+{3}: {4}", itemSize, newTaskIndex, taskGroupHeights, headerHeight, scrollDistance);
	
					//scroll to the new task
					if (adjustScrolledWindow == true)
						scrolledWindow.Vadjustment.Value = scrollDistance;
					
					taskGroup.EnterEditMode (task, iter);
					return;
				}
				if (taskGroup.Visible) {
					taskGroupHeights += taskGroup.Requisition.Height;
				}
			}
		}
		
		private void RebuildAddTaskMenu (Gtk.TreeModel categoriesModel)
		{
			Gtk.Menu menu = new Menu ();
			
			Gtk.TreeIter iter;
			if (categoriesModel.GetIterFirst (out iter) == true) {
				do {
					ICategory category =
						categoriesModel.GetValue (iter, 0) as ICategory;
					
					if (category is AllCategory)
						continue; // Skip this one
					
					CategoryMenuItem item = new CategoryMenuItem (category);
					item.Activated += OnNewTaskByCategory;
					item.ShowAll ();
					menu.Add (item);
				} while (categoriesModel.IterNext (ref iter) == true);
			}
			
			addTaskButton.Menu = menu;
		}
		
		private void SelectCategory (string categoryName)
		{
			Gtk.TreeIter iter;
			Gtk.TreeModel model = categoryComboBox.Model;
			bool categoryWasSelected = false;

			if (categoryName != null) {
				// Iterate through (yeah, I know this is gross!) and find the
				// matching category
				if (model.GetIterFirst (out iter) == true) {
					do {
						ICategory cat = model.GetValue (iter, 0) as ICategory;
						if (cat == null)
							continue; // Needed for some reason to prevent crashes from some backends
						if (cat.Name.CompareTo (categoryName) == 0) {
							categoryComboBox.SetActiveIter (iter);
							categoryWasSelected = true;
							break;
						}
					} while (model.IterNext (ref iter) == true);
				}
			}
			
			if (categoryWasSelected == false) {
				// Select the first item in the list (which should be the "All"
				// category.
				if (model.GetIterFirst (out iter) == true) {
					// Make sure we can actually get a category
					ICategory cat = model.GetValue (iter, 0) as ICategory;
					if (cat != null)
						categoryComboBox.SetActiveIter (iter);
				}
			}
		}
		
		private void ShowTaskNotes (ITask task)
		{
			NoteDialog dialog = null;
			if (noteDialogs.ContainsKey (task) == false) {
				dialog = new NoteDialog (this, task);
				dialog.Hidden += OnNoteDialogHidden;
				noteDialogs [task] = dialog;
			} else {
				dialog = noteDialogs [task];
			}
			
			dialog.Present ();
		}
		#endregion // Private Methods

		#region Event Handlers
		private void WindowDeleted (object sender, DeleteEventArgs args)
		{
			int x;
			int y;
			int width;
			int height;

			this.GetPosition(out x, out y);
			this.GetSize(out width, out height);

			lastXPos = x;
			lastYPos = y;
			
			Application.Preferences.SetInt("MainWindowLastXPos", lastXPos);
			Application.Preferences.SetInt("MainWindowLastYPos", lastYPos);
			Application.Preferences.SetInt("MainWindowWidth", width);
			Application.Preferences.SetInt("MainWindowHeight", height);

			Logger.Debug("WindowDeleted was called");
			taskWindow = null;
		}

		private void OnWindowShown (object sender, EventArgs args)
		{

		}
		
		void OnSettingChanged (Preferences preferences, string settingKey)
		{
			if (settingKey.CompareTo (Preferences.HideInAllCategory) != 0)
				return;
			
			OnCategoryChanged (this, EventArgs.Empty);
		}

		void OnAddTask (object sender, EventArgs args)
		{
			Gtk.TreeIter iter;
			if (categoryComboBox.GetActiveIter (out iter) == false)
				return;
			
			ICategory category =
				categoryComboBox.Model.GetValue (iter, 0) as ICategory;

			ITask task = backend.CreateTask (Catalog.GetString ("New task"), category);
			
			// Scroll to the task and put it into "edit" mode
			EnterEditMode (task, true);
			
		}
		
		void OnNewTaskByCategory (object sender, EventArgs args)
		{
			CategoryMenuItem item = sender as CategoryMenuItem;
			if (item == null)
				return;
			
			// Determine if the selected category is currently shown in the
			// task window.  If we're in a specific cateogory or on the All
			// category and the selected category is not showing, we've got
			// to switch the category first so the user will be able to edit
			// the title of the task.
			Gtk.TreeIter iter;
			if (categoryComboBox.GetActiveIter (out iter) == true) {
				ICategory selectedCategory =
					categoryComboBox.Model.GetValue (iter, 0) as ICategory;
				
				// Check to see if "All" is selected
				if (selectedCategory is AllCategory) {
					// See if the item.Category is currently being shown in
					// the "All" category and if not, select the category
					// specifically.
					List<string> categoriesToHide =
						Application.Preferences.GetStringList (
							Preferences.HideInAllCategory);
					if (categoriesToHide != null && categoriesToHide.Contains (item.Category.Name)) {
						SelectCategory (item.Category.Name);
					}
				} else if (selectedCategory.Name.CompareTo (item.Category.Name) != 0) {
					SelectCategory (item.Category.Name);
				}
			}
			
			ITask task =
				backend.CreateTask (Catalog.GetString ("New task"),
									item.Category);
			
			EnterEditMode (task, true);
		}
		
		void OnCategoryChanged (object sender, EventArgs args)
		{
			Gtk.TreeIter iter;
			if (categoryComboBox.GetActiveIter (out iter) == false)
				return;
			
			ICategory category =
				categoryComboBox.Model.GetValue (iter, 0) as ICategory;
				
			// Update the TaskGroups so they can filter accordingly
			overdueGroup.Refilter (category);
			todayGroup.Refilter (category);
			tomorrowGroup.Refilter (category);
			nextSevenDaysGroup.Refilter (category);
			futureGroup.Refilter (category);
			completedTaskGroup.Refilter (category);
			
			// Save the selected category in preferences
			Application.Preferences.Set (Preferences.SelectedCategoryKey,
										 category.Name);
		}
		
		void OnRowActivated (object sender, Gtk.RowActivatedArgs args)
		{
			// Check to see if a note dialog is already open for the activated
			// task.  If so, just bring it forward.  Otherwise, open a new one.
			Gtk.TreeView tv = sender as Gtk.TreeView;
			if (tv == null)
				return;
			
			Gtk.TreeModel model = tv.Model;
			
			Gtk.TreeIter iter;
			
			if (model.GetIter (out iter, args.Path) == false)
				return;
			
			ITask task = model.GetValue (iter, 0) as ITask;
			if (task == null)
				return;
			
			ShowTaskNotes (task);
		}
		

		[GLib.ConnectBefore]
		void OnButtonPressed (object sender, Gtk.ButtonPressEventArgs args)
		{
	        switch (args.Event.Button) {
	            case 3: // third mouse button (right-click)
		            clickedTask = null;

					Gtk.TreeView tv = sender as Gtk.TreeView;
					if (tv == null)
						return;
					
					Gtk.TreeModel model = tv.Model;
					
					Gtk.TreeIter iter;
					Gtk.TreePath path;
					Gtk.TreeViewColumn column = null;

					if (tv.GetPathAtPos ((int) args.Event.X,
									(int) args.Event.Y, out path, out column) == false)
						return;

					if (model.GetIter (out iter, path) == false)
						return;
					
					clickedTask = model.GetValue (iter, 0) as ITask;
					if (clickedTask == null)
						return;
					
					Menu popupMenu = new Menu ();
					ImageMenuItem item;
					
					item = new ImageMenuItem (Catalog.GetString ("_Notes..."));
					item.Image = new Image (noteIcon);
					item.Activated += OnShowTaskNotes;
					popupMenu.Add (item);
					
					popupMenu.Add (new SeparatorMenuItem ());

					item = new ImageMenuItem (Catalog.GetString ("_Delete task"));
					item.Image = new Image(Gtk.Stock.Delete, IconSize.Menu);
					item.Activated += OnDeleteTask;
					popupMenu.Add (item);

					item = new ImageMenuItem(Catalog.GetString ("_Edit task"));
					item.Image = new Image(Gtk.Stock.Edit, IconSize.Menu);
					item.Activated += OnEditTask;
					popupMenu.Add (item);

					popupMenu.ShowAll();
					popupMenu.Popup ();
				
					// Logger.Debug ("Right clicked on task: " + task.Name);
					break;
			}
		}
		
		private void OnShowTaskNotes (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			ShowTaskNotes (clickedTask);
		}
		
		private void OnDeleteTask (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			Application.Backend.DeleteTask(clickedTask);
		}


		private void OnEditTask (object sender, EventArgs args)
		{
			if (clickedTask == null)
				return;
			
			EnterEditMode (clickedTask, false);
		}
		
		
		void OnNoteDialogHidden (object sender, EventArgs args)
		{
			NoteDialog dialog = sender as NoteDialog;
			if (dialog == null) {
				Logger.Warn ("OnNoteDialogHidden (), sender is not NoteDialog, it's: {0}", sender.GetType ().ToString ());
				return;
			}
			
			if (noteDialogs.ContainsKey (dialog.Task) == false) {
				Logger.Warn ("Closed NoteDialog not found in noteDialogs");
				return;
			}
			
			Logger.Debug ("Removing NoteDialog from noteDialogs");
			noteDialogs.Remove (dialog.Task);
			
			dialog.Destroy ();
		}
		
		private void OnBackendInitialized()
		{		
			backend.BackendInitialized -= OnBackendInitialized;
			PopulateWindow();
			OnBackendSyncFinished (); // To update the statusbar
		}
		
		private void OnBackendSyncStarted ()
		{
			TaskWindow.ShowStatus (Catalog.GetString ("Reloading tasks..."));
		}
		
		private void OnBackendSyncFinished ()
		{
			Logger.Debug("Backend sync finished");
			string status =
				string.Format ("Tasks loaded: {0}",DateTime.Now.ToString ());
			TaskWindow.ShowStatus (status);
			if (Application.Backend.Configured) {
				RebuildAddTaskMenu (Application.Backend.Categories);
				addTaskButton.Sensitive = true;
			}
		}
		#endregion // Event Handlers
		
		#region Private Classes
		class CategoryMenuItem : Gtk.MenuItem
		{
			private ICategory cat;
			
			public CategoryMenuItem (ICategory category) : base (category.Name)
			{
				cat = category;
			}
			
			public ICategory Category
			{
				get { return cat; }
			}
		}
		#endregion // Private Classes
	}
	
	/// <summary>
	/// Provide keybindings via a fake Gtk.Menu.
	/// </summary>
	public class GlobalKeybinder
	{
		Gtk.AccelGroup accel_group;
		Gtk.Menu fake_menu;

		/// <summary>
		/// Create a global keybinder for the given Gtk.AccelGroup.
		/// </summary>
		/// </param>
		public GlobalKeybinder (Gtk.AccelGroup accel_group)
		{
			this.accel_group = accel_group;

			fake_menu = new Gtk.Menu ();
			fake_menu.AccelGroup = accel_group;
		}

		/// <summary>
		/// Add a keybinding for this keybinder's AccelGroup.
		/// </summary>
		/// <param name="handler">
		/// A <see cref="EventHandler"/> for when the keybinding is
		/// activated.
		/// </param>
		/// <param name="key">
		/// A <see cref="System.UInt32"/> specifying the key that will
		/// be bound (see the Gdk.Key enumeration for common values).
		/// </param>
		/// <param name="modifiers">
		/// The <see cref="Gdk.ModifierType"/> to be used on key
		/// for this binding.
		/// </param>
		/// <param name="flags">
		/// The <see cref="Gtk.AccelFlags"/> for this binding.
		/// </param>
		public void AddAccelerator (EventHandler handler,
		                            uint key,
		                            Gdk.ModifierType modifiers,
		                            Gtk.AccelFlags flags)
		{
			Gtk.MenuItem foo = new Gtk.MenuItem ();
			foo.Activated += handler;
			foo.AddAccelerator ("activate",
			                    accel_group,
			                    key,
			                    modifiers,
			                    flags);
			foo.Show ();

			fake_menu.Append (foo);
		}
	}
}
