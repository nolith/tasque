/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// EDSTask.cs
// User: Johnny

using System;
using Tasque;
using System.Collections.Generic;
using Evolution;

namespace Tasque.Backends.EDS
{
       public class EDSTask : AbstractTask
       {
               private CalComponent taskComp;
               private string name;
               private DateTime dueDate;
               private DateTime completionDate;
               private TaskPriority priority;
               private TaskState state;
               private string id;
               private EDSCategory category;

	       private List<INote> notes;		

               public EDSTask(CalComponent task, EDSCategory category)
               {

                       this.name = task.Summary;
                       Logger.Debug ("Creating New Task Object : {0}",this.name);
                       this.id = task.Uid;
                       this.category = category;
                       this.completionDate = task.Dtend;
                       this.taskComp = task;
		       
		       if (task.Status == CalStatus.Completed)
			       this.state = TaskState.Completed;
		       else
			       this.state = TaskState.Active;

		       //Descriptions
		       notes = new List<INote>();

		       foreach(string description in task.Descriptions) {
			       EDSNote edsNote = new EDSNote (description);
			       Logger.Debug ("Note :" + description);
			       notes.Add(edsNote);
		       }

               }

               #region Public Properties

	       public void Remove () {
		       Logger.Debug ("Removing task : {0} - {1}", this.Name, this.Id);
		       this.taskComp.ECal.RemoveObject (this.Id);
               }

               private void Commit () {
                       this.taskComp.Commit ();
               }

               public override string Id
               {
                       get { return id; }
               }

               public override string Name
               {
                       get {
                               // BUG : issue with e# ?? :(
                               // Should be using taskComp.Summary here.
                               return this.name;
                       }
                       set {
                               Logger.Debug ("Setting new task name");
                               if (value == null)
                                       this.taskComp.Summary = string.Empty;

                               // BUG : issue with e# ?? :(
                               this.name = value.Trim ();
                               this.taskComp.Summary = value.Trim ();

                               this.Commit ();
                       }
               }

               public override DateTime DueDate
               {
                       get { return taskComp.Due; }
                       set {
                               Logger.Debug ("Setting new task due date");
                               taskComp.Due = value;
                               this.Commit ();
                       }
               }

               public override DateTime CompletionDate
               {
                       get { return taskComp.Completed; }
                       set {
                               Logger.Debug ("Setting new task completion date");
                               taskComp.Completed = value;

                               this.Commit ();
                       }
               }

               public override bool IsComplete
               {
		       get { return state == TaskState.Completed; }
               }

               public override TaskPriority Priority
               {
                       get {
                               switch (taskComp.Priority) {
                                       default:
                                       case CalPriority.Undefined:
                                               return TaskPriority.None;
                                       case CalPriority.High:
                                               return TaskPriority.High;
                                       case CalPriority.Normal:
                                               return TaskPriority.Medium;
                                       case CalPriority.Low:
                                               return TaskPriority.Low;
                               }
                       }
                       set {
                               Console.WriteLine ("Setting Priority : {0}", value);
                               switch (value) {
                                       default:
                                       case TaskPriority.None:
                                               taskComp.Priority = CalPriority.Undefined;
                                               break;
                                       case TaskPriority.High:
                                               taskComp.Priority = CalPriority.High;
                                               break;
                                       case TaskPriority.Medium:
                                               taskComp.Priority = CalPriority.Normal;
                                               break;
                                       case TaskPriority.Low:
                                               taskComp.Priority = CalPriority.Low;
                                               break;
                               }
                               Console.WriteLine ("taskComp : priority : {0}", taskComp.Priority);
                               this.Commit ();
                       }
               }

               public override bool HasNotes
               {
		       get { return (notes.Count > 0); }
               }

               public override bool SupportsMultipleNotes
               {
                       get { return true; }
               }

               public override TaskState State
               {
                       get { return state; }
               }

               public override ICategory Category
               {
                       get { return category; }
                       set {
                               category = value as EDSCategory;
                       }
               }

               public override List<INote> Notes
               {
                       get { return notes; }
               }

               #endregion // Public Properties

	       private void UpdateNotes ()
	       {
		       string[] descriptions = new string [this.notes.Count];
		       // Too much 'c' influence here ? 
		       int i = 0;
		       foreach (EDSNote note in this.notes ) {
			       descriptions [i] = string.Copy (note.Text);
			       i++;
		       }

		       this.taskComp.Descriptions = descriptions;
		       this.Commit ();
	       }

               #region Public Methods
               public override void Activate ()
               {
                       Logger.Debug ("EDSTask.Activate ()");
                       state = TaskState.Active;
                       this.taskComp.Status = CalStatus.InProcess;
                       CompletionDate = DateTime.MinValue;
               }

               public override void Inactivate ()
               {
                       Logger.Debug ("EDSTask.Inactivate ()");
                       state = TaskState.Inactive;
                       this.taskComp.Status = CalStatus.None;
                       CompletionDate = DateTime.Now;
               }

               public override void Complete ()
               {
                       Logger.Debug ("EDSTask.Complete () : " + Name);
                       this.taskComp.Status = CalStatus.Completed;
                       CompletionDate = DateTime.Now;
                       state = TaskState.Completed;
               }

               public override void Delete ()
               {
                       Logger.Debug ("EDSTask.Delete ()");
                       state = TaskState.Deleted;
               }

               public override INote CreateNote(string text)
               {
		       EDSNote edsNote;
			
		       edsNote = new EDSNote (text);
		       notes.Add(edsNote);
		       this.UpdateNotes ();

		       return edsNote;
               }

               public override void DeleteNote(INote note)
               {
		       foreach(EDSNote edsNote in notes) {
			       if(string.Equals (edsNote.Text, note.Text)) {
				       notes.Remove(edsNote);
				       break;
			       }
		       }
		       this.UpdateNotes ();
               }

               public override void SaveNote(INote note)
               {
		       this.UpdateNotes ();
               }

               #endregion // Public Methods
       }
}
