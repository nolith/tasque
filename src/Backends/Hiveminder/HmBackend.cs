/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// HMBackend.cs
//
// Copyright (c) 2008 Johnny Jacob <johnnyjacob@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

using Mono.Unix;
using Hiveminder;
using Tasque.Backends;

namespace Tasque.Backends.HmBackend
{
	public class HmBackend : IBackend
	{

		private Hiveminder.Hiveminder hm;
		/// <summary>
		/// Keep track of the Gtk.TreeIters for the tasks so that they can
		/// be referenced later.
		///
		/// Key   = Task ID
		/// Value = Gtk.TreeIter in taskStore
		/// </summary>
		private Dictionary<string, Gtk.TreeIter> taskIters;
		private int newTaskId;
		private Gtk.TreeStore taskStore;
		private Gtk.TreeModelSort sortedTasksModel;
		private object taskLock;
		
		private bool initialized;
		private bool configured;

		private Thread refreshThread;
		private bool runningRefreshThread;
		private AutoResetEvent runRefreshEvent;
		
		private Gtk.ListStore categoryListStore;
		private Gtk.TreeModelSort sortedCategoriesModel;

		public event BackendInitializedHandler BackendInitialized;
		public event BackendSyncStartedHandler BackendSyncStarted;
		public event BackendSyncFinishedHandler BackendSyncFinished;

		private static string credentialFile = System.IO.Path.Combine (
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "tasque" + Path.DirectorySeparatorChar + "hm");
		
		
		public HmBackend ()
		{
			initialized = false;
			configured = false;
			
			newTaskId = 0;
			taskIters = new Dictionary<string, Gtk.TreeIter> (); 
			taskStore = new Gtk.TreeStore (typeof (ITask));

			taskLock = new object ();

			sortedTasksModel = new Gtk.TreeModelSort (taskStore);
			sortedTasksModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareTasksSortFunc));
			sortedTasksModel.SetSortColumnId (0, Gtk.SortType.Ascending);
			
			categoryListStore = new Gtk.ListStore (typeof (ICategory));
			
			sortedCategoriesModel = new Gtk.TreeModelSort (categoryListStore);
			sortedCategoriesModel.SetSortFunc (0, new Gtk.TreeIterCompareFunc (CompareCategorySortFunc));
			sortedCategoriesModel.SetSortColumnId (0, Gtk.SortType.Ascending);

			runRefreshEvent = new AutoResetEvent(false);
			runningRefreshThread = false;
		}

		void HandleRowChanged(object o, Gtk.RowChangedArgs args)
		{
			Logger.Debug ("Handle Row Changed : Task Modified.");
			HmTask task = (HmTask) taskStore.GetValue (args.Iter, 0);
			Logger.Debug (task.Name);
			
		}
		
		#region Public Properties
		public string Name
		{
			get { return "Hiveminder"; }
		}
		
		/// <value>
		/// All the tasks .
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
		
		/// <value>
		/// Indication that the backend is configured
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
			Hiveminder.Task task = new Task ();
			Hiveminder.Task createdTask;
			Gtk.TreeIter taskIter;

			task.Summary = taskName;

			createdTask = this.hm.CreateTask (task);
			HmTask hmTask = new HmTask (createdTask, this);

			//Add the newly created task into our store.
			lock (taskLock) {
				taskIter = taskStore.AppendNode ();
				taskStore.SetValue (taskIter, 0, hmTask);
				taskIters [hmTask.Id] = taskIter;
			}

			return hmTask;
		}
		
		public void DeleteTask(ITask task)
		{

		}		
		
		public void Refresh()
		{
			Logger.Debug("Refreshing data...");

			runRefreshEvent.Set();
			
			Logger.Debug("Done refreshing data!");
		}
		
		public void Initialize()
		{
			Gtk.TreeIter iter;
			try {
				string username, password;
				LoadCredentials (out username,out password);
				this.hm = new Hiveminder.Hiveminder(username, password);
				configured = true;
			} catch (HiveminderAuthException e) {
				Logger.Debug (e.ToString());
				Logger.Error ("Hiveminder authentication failed.");
			} catch (Exception e) {
				Logger.Debug (e.ToString());
				Logger.Error ("Unable to connect to Hiveminder");
			}
			//
			// Add in the "All" Category
			//
			AllCategory allCategory = new Tasque.AllCategory ();
			iter = categoryListStore.Append ();
			categoryListStore.SetValue (iter, 0, allCategory);
			
			runningRefreshThread = true;
			if (refreshThread == null || refreshThread.ThreadState == ThreadState.Running) {
				Logger.Debug ("RtmBackend refreshThread already running");
			} else {
				if (!refreshThread.IsAlive) {
					refreshThread  = new Thread(RefreshThreadLoop);
				}
				refreshThread.Start();
			}
			runRefreshEvent.Set();		
		}

		public void RefreshTasks()
		{
			Gtk.TreeIter iter;

			Logger.Debug ("Fetching tasks");

			HmTask[] tasks = HmTask.GetTasks (this.hm.DownloadTasks(), this);

			foreach (HmTask task in tasks) {
				task.Dump();
				iter = taskStore.AppendNode();
				taskStore.SetValue (iter, 0, task);				
			}

			Logger.Debug ("Fetching tasks Completed");
		}

		public void RefreshCategories ()
		{
			Gtk.TreeIter iter;
			HmCategory[] categories = HmCategory.GetCategories (this.hm.DownloadGroups());

			foreach (HmCategory category in categories) {
				category.Dump();
				iter = categoryListStore.Append ();
				categoryListStore.SetValue (iter, 0, category);
			}

		    Logger.Debug ("Fetching Categories");
		}

		public void Cleanup()
		{}
		
		public Gtk.Widget GetPreferencesWidget ()
		{
			return new HmPreferencesWidget();
		}

		
		public static bool LoadCredentials (out string username, out string password)
		{
			try {
				TextReader configFile = new StreamReader (new FileStream(credentialFile, FileMode.OpenOrCreate, FileAccess.Read));

				username = configFile.ReadLine ();
				password = configFile.ReadLine ();
				
				if (username == string.Empty || password == string.Empty)
					return false;
				
				configFile.Close ();
				
			} catch (Exception e) {
				Console.WriteLine (e);
				
				username = string.Empty;
				password = string.Empty;
				
				return false;
			}

			return true;
		}

		public static bool PreserveCredentials (string username, string password)
		{
			if (username == string.Empty || password == string.Empty)
					return false;

			try {
				
				TextWriter configFile = new StreamWriter(new FileStream(credentialFile, FileMode.Create, FileAccess.Write));

				configFile.WriteLine (username);
				configFile.WriteLine (password);
				
				configFile.Close();
				
			} catch (Exception e) {
				return false;
			}
			
			return true;
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

				if(this.hm != null) {
					RefreshCategories();
					RefreshTasks();
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

		public void UpdateTask (HmTask task)
		{
			Logger.Debug ("Updating task : " + task.Id);
			this.hm.UpdateTask (task.RemoteTask);
		}

		#endregion // Private Methods
		
		#region Event Handlers
		#endregion // Event Handlers
	}
}
