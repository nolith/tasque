// RtmBackend.cs created with MonoDevelop
// User: boyd at 7:10 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Mono.Unix;
using Tasque.Backends;
using RtmNet;
using System.Threading;
using System.Collections.Generic;

namespace Tasque.Backends.RtmBackend
{
	public class RtmBackend : IBackend
	{
		private const string apiKey = "b29f7517b6584035d07df3170b80c430";
		private const string sharedSecret = "93eb5f83628b2066";
		private Gtk.TreeStore taskStore;
		private Gtk.TreeModelSort sortedTasksModel;

		private Gtk.ListStore categoryListStore;
		private Gtk.TreeModelSort sortedCategoriesModel;
		
		private Thread refreshThread;
		private bool runningRefreshThread;
		private AutoResetEvent runRefreshEvent;

		private Rtm rtm;
		private string frob;
		private Auth rtmAuth;
		private string timeline;
		
		private Dictionary<string, Gtk.TreeIter> taskIters;
		private object taskLock;

		private Dictionary<string, RtmCategory> categories;
		private object catLock;
		private bool initialized;
		private bool configured;

		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;
		
		public RtmBackend ()
		{
			initialized = false;
			configured = false;

			taskIters = new Dictionary<string, Gtk.TreeIter> ();
			taskLock = new Object();
			
			categories = new Dictionary<string, RtmCategory> ();
			catLock = new Object();

			// *************************************
			// Data Model Set up
			// *************************************
			taskStore = new Gtk.TreeStore (typeof (ITask));

			sortedTasksModel = new Gtk.TreeModelSort (taskStore);
			sortedTasksModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareTasksSortFunc));
			sortedTasksModel.SetSortColumnId (0, Gtk.SortType.Ascending);

			categoryListStore = new Gtk.ListStore (typeof (ICategory));

			sortedCategoriesModel = new Gtk.TreeModelSort (categoryListStore);
			sortedCategoriesModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareCategorySortFunc));
			sortedCategoriesModel.SetSortColumnId (0, Gtk.SortType.Ascending);

			// make sure we have the all Category in our list
			Gtk.Application.Invoke ( delegate {
				AllCategory allCategory = new Tasque.AllCategory ();
				Gtk.TreeIter iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, allCategory);				
			});

			runRefreshEvent = new AutoResetEvent(false);
			
			runningRefreshThread = false;
			refreshThread  = new Thread(RefreshThreadLoop);
		}

		#region Public Properties
		public string Name
		{
			get { return "Remember the Milk"; }
		}
		
		/// <value>
		/// All the tasks including ITaskDivider items.
		/// </value>
		public Gtk.TreeModel Tasks
		{
			get { return sortedTasksModel; }
		}

		/// <value>
		/// This returns all the task lists (categories) that exist.
		/// </value>
		public Gtk.TreeModel Categories
		{
			get { return sortedCategoriesModel; }
		}

		public string RtmUserName
		{
			get {
				if( (rtmAuth != null) && (rtmAuth.User != null) ) {
					return rtmAuth.User.Username;
				} else
					return null;
			}
		}
		
		/// <value>
		/// Indication that the rtm backend is configured
		/// </value>
		public bool Configured
		{
			get { return configured; }
		}
		
		/// <value>
		/// Inidication that the backend is initialized
		/// </value>
		public bool Initialized
		{
			get { return initialized; }
		}
		
#endregion // Public Properties

#region Public Methods
		public ITask CreateTask (string taskName, ICategory category)
		{
			string categoryID;
			RtmTask rtmTask = null;
			
			if(category is Tasque.AllCategory)
				categoryID = null;
			else
				categoryID = (category as RtmCategory).ID;	

			if(rtm != null) {
				try {
					List list;
					
					if(categoryID == null)
						list = rtm.TasksAdd(timeline, taskName);
					else
						list = rtm.TasksAdd(timeline, taskName, categoryID);

					rtmTask = UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set create task: " + taskName);
					Logger.Debug(e.ToString());
				}
			}
			else
				throw new Exception("Unable to communicate with Remember The Milk");
				
			return rtmTask;
		}
		
		public void DeleteTask(ITask task)
		{
			RtmTask rtmTask = task as RtmTask;
			if(rtm != null) {
				try {
					rtm.TasksDelete(timeline, rtmTask.ListID, rtmTask.SeriesTaskID, rtmTask.TaskTaskID);

					lock(taskLock)
					{
						Gtk.Application.Invoke ( delegate {
							if(taskIters.ContainsKey(rtmTask.ID)) {
								Gtk.TreeIter iter = taskIters[rtmTask.ID];
								taskStore.Remove(ref iter);
								taskIters.Remove(rtmTask.ID);
							}
						});
					}
				} catch(Exception e) {
					Logger.Debug("Unable to delete task: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}
			else
				throw new Exception("Unable to communicate with Remember The Milk");
		}
		
		public void Refresh()
		{
			Logger.Debug("Refreshing data...");

			if (!runningRefreshThread)
				StartThread();

			runRefreshEvent.Set();
			
			Logger.Debug("Done refreshing data!");
		}

		public void Initialize()
		{
			// *************************************
			// AUTHENTICATION to Remember The Milk
			// *************************************
			string authToken =
				Application.Preferences.Get (Preferences.AuthTokenKey);
			if (authToken != null ) {
				Logger.Debug("Found AuthToken, checking credentials...");
				try {
					rtm = new Rtm(apiKey, sharedSecret, authToken);
					rtmAuth = rtm.AuthCheckToken(authToken);
					timeline = rtm.TimelineCreate();
					Logger.Debug("RTM Auth Token is valid!");
					Logger.Debug("Setting configured status to true");
					configured = true;
				} catch (RtmNet.RtmApiException e) {
					
					Application.Preferences.Set (Preferences.AuthTokenKey, null);
					Application.Preferences.Set (Preferences.UserIdKey, null);
					Application.Preferences.Set (Preferences.UserNameKey, null);
					rtm = null;
					rtmAuth = null;
					Logger.Error("Exception authenticating, reverting" + e.Message);
				} 			
				catch (RtmNet.RtmWebException e) {
					rtm = null;
					rtmAuth = null;
					Logger.Error("Not connected to RTM, maybe proxy: #{0}", e.Message);
 				}
				catch (System.Net.WebException e) {
					rtm = null;
					rtmAuth = null;
					Logger.Error("Problem connecting to internet: #{0}", e.Message);
				}
			}

			if(rtm == null)
				rtm = new Rtm(apiKey, sharedSecret);
	
			StartThread();	
		}

		public void StartThread()
		{
			if (!configured) {
				Logger.Debug("Backend not configured, not starting thread");
				return;
			}
			runningRefreshThread = true;
			Logger.Debug("ThreadState: " + refreshThread.ThreadState);
			if (refreshThread.ThreadState == ThreadState.Running) {
				Logger.Debug ("RtmBackend refreshThread already running");
			} else {
				if (!refreshThread.IsAlive) {
					refreshThread  = new Thread(RefreshThreadLoop);
				}
				refreshThread.Start();
			}
			runRefreshEvent.Set();
		}

		public void Cleanup()
		{
			runningRefreshThread = false;
			runRefreshEvent.Set();
			refreshThread.Abort ();
		}

		public Gtk.Widget GetPreferencesWidget ()
		{
			return new RtmPreferencesWidget ();
		}

		public string GetAuthUrl()
		{
			frob = rtm.AuthGetFrob();
			string url = rtm.AuthCalcUrl(frob, AuthLevel.Delete);
			return url;
		}

		public void FinishedAuth()
		{
			rtmAuth = rtm.AuthGetToken(frob);
			if (rtmAuth != null) {
				Preferences prefs = Application.Preferences;
				prefs.Set (Preferences.AuthTokenKey, rtmAuth.Token);
				if (rtmAuth.User != null) {
					prefs.Set (Preferences.UserNameKey, rtmAuth.User.Username);
					prefs.Set (Preferences.UserIdKey, rtmAuth.User.UserId);
				}
			}
			
			string authToken =
				Application.Preferences.Get (Preferences.AuthTokenKey);
			if (authToken != null ) {
				Logger.Debug("Found AuthToken, checking credentials...");
				try {
					rtm = new Rtm(apiKey, sharedSecret, authToken);
					rtmAuth = rtm.AuthCheckToken(authToken);
					timeline = rtm.TimelineCreate();
					Logger.Debug("RTM Auth Token is valid!");
					Logger.Debug("Setting configured status to true");
					configured = true;
					Refresh();
				} catch (Exception e) {
					rtm = null;
					rtmAuth = null;				
					Logger.Error("Exception authenticating, reverting" + e.Message);
				}	
			}
		}

		public void UpdateTaskName(RtmTask task)
		{
			if(rtm != null) {
				try {
					List list = rtm.TasksSetName(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID, task.Name);		
					UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set name on task: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}
		}
		
		public void UpdateTaskDueDate(RtmTask task)
		{
			if(rtm != null) {
				try {
					List list;
					if(task.DueDate == DateTime.MinValue)
						list = rtm.TasksSetDueDate(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
					else	
						list = rtm.TasksSetDueDate(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID, task.DueDateString);
					UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set due date on task: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}
		}
		
		public void UpdateTaskCompleteDate(RtmTask task)
		{
			UpdateTask(task);
		}
		
		public void UpdateTaskPriority(RtmTask task)
		{
			if(rtm != null) {
				try {
					List list = rtm.TasksSetPriority(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID, task.PriorityString);
					UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set priority on task: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}
		}
		
		public void UpdateTaskActive(RtmTask task)
		{
			if(task.State == TaskState.Completed)
			{
				if(rtm != null) {
					try {
						List list = rtm.TasksUncomplete(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
						UpdateTaskFromResult(list);
					} catch(Exception e) {
						Logger.Debug("Unable to set Task as completed: " + task.Name);
						Logger.Debug(e.ToString());
					}
				}
			}
			else
				UpdateTask(task);
		}
		
		public void UpdateTaskInactive(RtmTask task)
		{
			UpdateTask(task);
		}	

		public void UpdateTaskCompleted(RtmTask task)
		{
			if(rtm != null) {
				try {
					List list = rtm.TasksComplete(timeline, task.ListID, task.SeriesTaskID, task.TaskTaskID);
					UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set Task as completed: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}
		}	

		public void UpdateTaskDeleted(RtmTask task)
		{
			UpdateTask(task);
		}
		

		public void MoveTaskCategory(RtmTask task, string id)
		{
			if(rtm != null) {
				try {
					List list = rtm.TasksMoveTo(timeline, task.ListID, id, task.SeriesTaskID, task.TaskTaskID);
					UpdateTaskFromResult(list);
				} catch(Exception e) {
					Logger.Debug("Unable to set Task as completed: " + task.Name);
					Logger.Debug(e.ToString());
				}
			}					
		}
		
		
		public void UpdateTask(RtmTask task)
		{
			lock(taskLock)
			{
				Gtk.TreeIter iter;
				
				Gtk.Application.Invoke ( delegate {
					if(taskIters.ContainsKey(task.ID)) {
						iter = taskIters[task.ID];
						taskStore.SetValue (iter, 0, task);
					}
				});
			}		
		}
		
		public RtmTask UpdateTaskFromResult(List list)
		{
			TaskSeries ts = list.TaskSeriesCollection[0];
			if(ts != null) {
				RtmTask rtmTask = new RtmTask(ts, this, list.ID);
				lock(taskLock)
				{
					Gtk.Application.Invoke ( delegate {
						if(taskIters.ContainsKey(rtmTask.ID)) {
							Gtk.TreeIter iter = taskIters[rtmTask.ID];
							taskStore.SetValue (iter, 0, rtmTask);
						} else {
							Gtk.TreeIter iter = taskStore.AppendNode();
							taskIters.Add(rtmTask.ID, iter);
							taskStore.SetValue (iter, 0, rtmTask);
						}
					});
				}
				return rtmTask;				
			}
			return null;
		}
		
		public RtmCategory GetCategory(string id)
		{
			if(categories.ContainsKey(id))
				return categories[id];
			else
				return null;
		}
		
		public RtmNote CreateNote (RtmTask rtmTask, string text)
		{
			RtmNet.Note note = null;
			RtmNote rtmNote = null;
			
			if(rtm != null) {
				try {
					note = rtm.NotesAdd(timeline, rtmTask.ListID, rtmTask.SeriesTaskID, rtmTask.TaskTaskID, String.Empty, text);
					rtmNote = new RtmNote(note);
				} catch(Exception e) {
					Logger.Debug("RtmBackend.CreateNote: Unable to create a new note");
					Logger.Debug(e.ToString());
				}
			}
			else
				throw new Exception("Unable to communicate with Remember The Milk");
				
			return rtmNote;
		}


		public void DeleteNote (RtmTask rtmTask, RtmNote note)
		{
			if(rtm != null) {
				try {
					rtm.NotesDelete(timeline, note.ID);
				} catch(Exception e) {
					Logger.Debug("RtmBackend.DeleteNote: Unable to delete note");
					Logger.Debug(e.ToString());
				}
			}
			else
				throw new Exception("Unable to communicate with Remember The Milk");
		}

		public void SaveNote (RtmTask rtmTask, RtmNote note)
		{
			if(rtm != null) {
				try {
					rtm.NotesEdit(timeline, note.ID, String.Empty, note.Text);
				} catch(Exception e) {
					Logger.Debug("RtmBackend.SaveNote: Unable to save note");
					Logger.Debug(e.ToString());
				}
			}
			else
				throw new Exception("Unable to communicate with Remember The Milk");
		}

#endregion // Public Methods

#region Private Methods
		static int CompareTasksSortFunc (Gtk.TreeModel model,
				Gtk.TreeIter a,
				Gtk.TreeIter b)
		{
			ITask taskA = model.GetValue (a, 0) as ITask;
			ITask taskB = model.GetValue (b, 0) as ITask;

			if (taskA == null || taskB == null)
				return 0;

			return (taskA.CompareTo (taskB));
		}

		static int CompareCategorySortFunc (Gtk.TreeModel model,
											Gtk.TreeIter a,
											Gtk.TreeIter b)
		{
			ICategory categoryA = model.GetValue (a, 0) as ICategory;
			ICategory categoryB = model.GetValue (b, 0) as ICategory;
			
			if (categoryA == null || categoryB == null)
				return 0;
			
			if (categoryA is Tasque.AllCategory)
				return -1;
			else if (categoryB is Tasque.AllCategory)
				return 1;
			
			return (categoryA.Name.CompareTo (categoryB.Name));
		}

		/// <summary>
		/// Update the model to match what is in RTM
		/// FIXME: This is a lame implementation and needs to be optimized
		/// </summary>		
		private void UpdateCategories(Lists lists)
		{
			Logger.Debug("RtmBackend.UpdateCategories was called");
			
			try {
				foreach(List list in lists.listCollection)
				{
					RtmCategory rtmCategory = new RtmCategory(list);

					lock(catLock)
					{
						Gtk.TreeIter iter;
						
						Gtk.Application.Invoke ( delegate {

							if(categories.ContainsKey(rtmCategory.ID)) {
								iter = categories[rtmCategory.ID].Iter;
								categoryListStore.SetValue (iter, 0, rtmCategory);
							} else {
								iter = categoryListStore.Append();
								categoryListStore.SetValue (iter, 0, rtmCategory);
								rtmCategory.Iter = iter;
								categories.Add(rtmCategory.ID, rtmCategory);
							}
						});
					}
				}
			} catch (Exception e) {
				Logger.Debug("Exception in fetch " + e.Message);
			}
			Logger.Debug("RtmBackend.UpdateCategories is done");			
		}

		/// <summary>
		/// Update the model to match what is in RTM
		/// FIXME: This is a lame implementation and needs to be optimized
		/// </summary>		
		private void UpdateTasks(Lists lists)
		{
			Logger.Debug("RtmBackend.UpdateTasks was called");
			
			try {
				foreach(List list in lists.listCollection)
				{
					Tasks tasks = null;
					try {
						tasks = rtm.TasksGetList(list.ID);
					} catch (Exception tglex) {
						Logger.Debug("Exception calling TasksGetList(list.ListID) " + tglex.Message);
					}

					if(tasks != null) {
						foreach(List tList in tasks.ListCollection)
						{
							if (tList.TaskSeriesCollection == null)
								continue;
							foreach(TaskSeries ts in tList.TaskSeriesCollection)
							{
								RtmTask rtmTask = new RtmTask(ts, this, list.ID);
								
								lock(taskLock)
								{
									Gtk.TreeIter iter;
									
									Gtk.Application.Invoke ( delegate {

										if(taskIters.ContainsKey(rtmTask.ID)) {
											iter = taskIters[rtmTask.ID];
										} else {
											iter = taskStore.AppendNode ();
											taskIters.Add(rtmTask.ID, iter);
										}

										taskStore.SetValue (iter, 0, rtmTask);
									});
								}
							}
						}
					}
				}
			} catch (Exception e) {
				Logger.Debug("Exception in fetch " + e.Message);
				Logger.Debug(e.ToString());
			}
			Logger.Debug("RtmBackend.UpdateTasks is done");			
		}
		
		
		
		private void RefreshThreadLoop()
		{
			while(runningRefreshThread) {
				runRefreshEvent.WaitOne();

				if(!runningRefreshThread)
					return;

				// Fire the event on the main thread
				Gtk.Application.Invoke ( delegate {
					if(BackendSyncStarted != null)
						BackendSyncStarted();
				});

				runRefreshEvent.Reset();

				if(rtmAuth != null) {
					Lists lists = rtm.ListsGetList();
					UpdateCategories(lists);
					UpdateTasks(lists);
				}
				if(!initialized) {
					initialized = true;

					// Fire the event on the main thread
					Gtk.Application.Invoke ( delegate {
						if(BackendInitialized != null)
							BackendInitialized();
					});
				}

				// Fire the event on the main thread
				Gtk.Application.Invoke ( delegate {
					if(BackendSyncFinished != null)
						BackendSyncFinished();
				});
			}
		}
		
#endregion // Private Methods

#region Event Handlers
#endregion // Event Handlers
	}
}
