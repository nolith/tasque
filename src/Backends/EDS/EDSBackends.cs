/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// EDSBackend.cs
// User: Johnny

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Unix;
using Evolution;
using Tasque.Backends;
using GLib;

namespace Tasque.Backends.EDS
{
       public class EDSBackend : IBackend
       {
               /// <summary>
               /// Keep track of the Gtk.TreeIters for the tasks so that they can
               /// be referenced later.
               ///
               /// Key   = Task ID
               /// Value = Gtk.TreeIter in taskStore
               /// </summary>
               private Dictionary<string, Gtk.TreeIter> taskIters;
               private Gtk.TreeStore taskStore;
               private Gtk.TreeModelSort sortedTasksModel;
               private bool initialized;
               private object taskLock;

               private Gtk.ListStore categoryListStore;
               private Gtk.TreeModelSort sortedCategoriesModel;

	       private EDSCategory defaultCategory;
		
               public event BackendInitializedHandler BackendInitialized;
               public event BackendSyncStartedHandler BackendSyncStarted;
               public event BackendSyncFinishedHandler BackendSyncFinished;

               public EDSBackend ()
               {
                       initialized = false;

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

		       defaultCategory = null;
               }

               #region Public Properties

               public string Name
               {
                       get { return "Evolution Data Server"; }
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
		       Gtk.TreeIter taskIter;
		       EDSTask edsTask;

                       if (category == null )
                               return null;

		       if (category is Tasque.AllCategory && defaultCategory != null)
			       category = this.defaultCategory;

                       EDSCategory edsCategory = category as EDSCategory;
                       CalComponent task = new CalComponent (edsCategory.TaskList);
                       task.Summary = taskName;

                       lock (taskLock) {
			       edsTask = new EDSTask (task, edsCategory);
			       taskIter = taskStore.AppendNode ();
			       taskStore.SetValue (taskIter, 0, edsTask);
			       taskIters [task.Uid] = taskIter;
		       }

                       task.Commit ();

                       return edsTask;
               }

	       public void DeleteTask(ITask task)
	       {
		       EDSTask edsTask = task as EDSTask;
		       edsTask.Remove();
	       }

               public void Refresh()
               {}

               public void Initialize()
               {
                       Gtk.TreeIter iter;

                       AllCategory allCategory = new Tasque.AllCategory ();
                       iter = categoryListStore.Append ();
                       categoryListStore.SetValue (iter, 0, allCategory);

		       Logger.Debug ("Initializing EDS Backend ");

                       try {
			       ListenForGroups ();
                       } catch (Exception e) {
                               Logger.Debug ("Fatal : " + e);
                       }

                       initialized = true;
                       if(BackendInitialized != null) {
                               BackendInitialized();
                       }
               }

               public Gtk.Widget GetPreferencesWidget ()
               {
		       Logger.Debug ("No Preference Widget ");
                       return null;
               }

               public void Cleanup()
               {}
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

               public bool Configured
               {
                       get { return true; }
               }

               public void TasksAdded (object o, Evolution.ObjectsAddedArgs args)
               {
		       Logger.Debug ("Tasks Added ");
                       CalComponent[] addedTasks = CalUtil.ICalToCalComponentArray (args.Objects.Handle, ((CalView) o).Client);
                       lock (taskLock) {
                               Gtk.TreeIter taskIter;
                               EDSTask edsTask;
                               EDSCategory edsCategory;
                               foreach (CalComponent task in addedTasks) {
				       if(!taskIters.ContainsKey(task.Uid)) {
					       edsCategory = new EDSCategory (task.Source);
					       edsTask = new EDSTask (task, edsCategory);
					       taskIter = taskStore.AppendNode ();
					       taskStore.SetValue (taskIter, 0, edsTask);
					       taskIters [edsTask.Id] = taskIter;
				       }
                               }
                       }
               }

               public void TasksModified (object o, Evolution.ObjectsModifiedArgs args)
               {
		       Logger.Debug ("Tasks Modified ");
                       Gtk.TreeIter iter;
                       EDSTask edsTask;
                       EDSCategory edsCategory;

                       CalComponent[] modifiedTasks = CalUtil.ICalToCalComponentArray (args.Objects.Handle, ((CalView) o).Client);

                       foreach (CalComponent task in modifiedTasks) {
			       Logger.Debug ("Modified : " + task.Summary);
                               if(taskIters.ContainsKey(task.Uid)) {
				       edsCategory = new EDSCategory (task.Source);
				       edsTask = new EDSTask (task, edsCategory);
                                       iter = taskIters[edsTask.Id];
                                       taskStore.SetValue (iter, 0, edsTask);
                               }
                       }
               }

               //FIXME : in evolution-sharp. Add this type.
               [StructLayout (LayoutKind.Sequential)]
               private struct CalComponentId {
                       public string Uid;
                       public string Rid;
               }

               public void TasksRemoved (object o, Evolution.ObjectsRemovedArgs args)
               {
		       Logger.Debug ("Tasks Removed");
                       Gtk.TreeIter iter;

                       GLib.List removedTasksList = new GLib.List (args.Uids.Handle,
                                                                      typeof (CalComponentId));

                       foreach (CalComponentId id in removedTasksList) {
                               if(taskIters.ContainsKey(id.Uid)) {
                                       iter = taskIters[id.Uid];
                                       taskStore.Remove (ref iter);
                               }

                       }

                       Logger.Debug ("{0} Tasks removed in EDS", removedTasksList.Count);

               }

	       private void ListenForGroups ()
	       {
		       Logger.Debug ("Listening for Changes in EDS Task Groups ");
		       SourceList slist = new SourceList ("/apps/evolution/tasks/sources");

		       if (slist == null)
			       Logger.Debug ("Unable to find sources");

		       slist.GroupAdded += OnGroupAdded;
		       slist.GroupRemoved += OnGroupRemoved;

		       foreach (SourceGroup group in slist.Groups) {
			       ListenForSources (group);
		       }
	       }

	       private void OnGroupAdded (object o, GroupAddedArgs args)
	       {
		       Logger.Debug ("Groups Added.");
		       SourceGroup group = args.Group;
		       ListenForSources (group);
	       }

	       private void ListenForSources (SourceGroup group)
	       {
		       Logger.Debug ("ListenForSources.");

		       //FIXME : Bug in e-sharp :( ? 
		       group.SourceAdded += OnSourceAdded;
		       group.SourceRemoved += OnSourceRemoved;

		       foreach (Evolution.Source source in group.Sources) {
			       AddCategory (source);
		       }
	       }

	       private void OnGroupRemoved (object o, GroupRemovedArgs args)
	       {
		       Logger.Debug ("Groups Removed.");
	       }

	       private void OnSourceAdded (object o, SourceAddedArgs args) 
	       {
		       Logger.Debug ("Source Added");
		       Evolution.Source source = args.Source;
		       AddCategory (source);
	       }

	       private void OnSourceRemoved (object o, SourceRemovedArgs args) 
	       {
		       Logger.Debug ("Source Removed");
		       Evolution.Source source = args.Source;
		       //RemoveCategory (source);
	       }

	       private void AddCategory (Evolution.Source source)
	       {
		       Logger.Debug ("AddCategory");
                       EDSCategory edsCategory;
                       Gtk.TreeIter iter;

		       if (source.IsLocal()) {
			       Cal taskList = new Cal (source, CalSourceType.Todo);

			       edsCategory = new EDSCategory (source, taskList);
			       iter = categoryListStore.Append ();
			       categoryListStore.SetValue (iter, 0, edsCategory);

			       //Assumption : EDS Creates atleast one System category.
			       if (edsCategory.IsSystem)
				       this.defaultCategory = edsCategory;
				
			       if (!taskList.Open (true)) {
				       Logger.Debug ("laskList Open failed");
				       return;
			       }

			       CalView query = taskList.GetCalView ("#t");
			       if (query == null) {
				       Logger.Debug ("Query object creation failed");
				       return;
			       } else
				       query.Start ();

			       query.ObjectsModified += TasksModified;
			       query.ObjectsAdded += TasksAdded;
			       query.ObjectsRemoved += TasksRemoved;
		       }
	       }

               public void UpdateTask (EDSTask task)
               {
                       // Set the task in the store so the model will update the UI.
                       Gtk.TreeIter iter;

                       if (!taskIters.ContainsKey (task.Id))
                               return;

                       iter = taskIters [task.Id];

                       if (task.State == TaskState.Deleted) {
                               taskIters.Remove (task.Id);
                               if (!taskStore.Remove (ref iter)) {
                                       Logger.Debug ("Successfully deleted from taskStore: {0}",
                                               task.Name);
                               } else {
                                       Logger.Debug ("Problem removing from taskStore: {0}",
                                               task.Name);
                               }
                       } else {
                               taskStore.SetValue (iter, 0, task);
                       }
               }
               #endregion // Private Methods

               #region Event Handlers
               #endregion // Event Handlers
       }
}
