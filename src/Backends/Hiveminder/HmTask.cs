/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// HmTask.cs
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
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using Hiveminder;

namespace Tasque.Backends.HmBackend
{
	public class HmTask : AbstractTask
	{
		Task task;

		private HmBackend backend;
		private List <INote> notes;
		
		#region Properties
		
		public override string Id
		{
			get { return this.task.Id; } 
		}
	
		public override string Name
		{
			get { return this.task.Summary; }
			set { 
				this.task.Summary = value; 
				this.backend.UpdateTask (this);
			}
		}

		public override DateTime DueDate
		{
			get { 
				if (!string.IsNullOrEmpty(this.task.Due)) {
					return DateTime.Parse (this.task.Due);
				}
				return DateTime.MinValue;
			}
			set {
				this.task.Due = value.ToString();
				this.backend.UpdateTask (this);
			}
		}

		public override DateTime CompletionDate
		{
			get {return DateTime.Now;}
			set {Logger.Info ("Not implemented");}
		}
		
		public override bool IsComplete 
		{
			get {return this.task.IsComplete;}
		}
		
		public override TaskPriority Priority
		{
			get {
					switch (this.task.Priority) {
					case 1 :
						return TaskPriority.Low;
					case 3:
						return TaskPriority.Medium;
					case 5:
						return TaskPriority.High;
					default:
						return TaskPriority.None;
					}
					return TaskPriority.None; 
				}
			
			set {
					switch (value) {
					case TaskPriority.High:
						this.task.Priority = 1;
						break;
					case TaskPriority.Low:
						this.task.Priority = 3;
						break;
					case TaskPriority.Medium:
						this.task.Priority = 5;
						break;
					default:
						this.task.Priority = 0;
						break;
					}
					this.backend.UpdateTask (this);
				}
		}
		
		public override bool HasNotes
		{
			get {return !string.IsNullOrEmpty(this.task.Description);}
		}
		
		public override bool SupportsMultipleNotes
		{
			get {return false;}
		}
		
		public override TaskState State
		{
			get {
				if (this.task.IsComplete)
					return TaskState.Completed;
				
				return TaskState.Active;
			}
		}
		
		public override ICategory Category
		{
			get {return null;} 
			set { Logger.Info ("Not implemented");}
		}
		
		public override List<INote> Notes
		{
			get {
				return this.notes; 
			}
		}

		public Hiveminder.Task RemoteTask
		{
			get { return this.task; }
		}

		/// <value>
		/// The ID of the timer used to complete a task after being marked
		/// inactive.
		/// </value>
		public uint TimerID
		{
			get { return 0; }
			set {Logger.Info ("Not implemented"); }
		}

		public string GroupId
		{
			get {return task.GroupId;}
		}
		
		#endregion Properties

		public static HmTask[] GetTasks (XmlNodeList list, HmBackend backend)
		{
			uint i = 0;
			XmlSerializer serializer = new XmlSerializer(typeof(Task));
			HmTask[] tasks = new HmTask[list.Count];
			
			foreach (XmlNode node in list ) 
				tasks[i++] = new HmTask ((Task)serializer.
							 Deserialize(new StringReader(node.OuterXml)), 
							 backend);
			
			return tasks;
		}
		
		#region Constructors
		
		public HmTask () : this (new Task(), null)
		{
		}
		
		public HmTask (Hiveminder.Task task, HmBackend hmBackend)
		{
			this.task = task;
			this.backend = hmBackend;

			//Add Description as note.
			this.notes = new List<INote>();

			if (!string.IsNullOrEmpty (this.task.Description)) 
				notes.Add (new HmNote (this.task));
		}
		
		#endregion
		
		#region Methods	
		public override void Activate ()
		{
			Logger.Info ("Activate : Not implemented");
		}
		
		/// <summary>
		/// Sets the task to be inactive
		/// </summary>
		public override void Inactivate ()
		{
			Logger.Info ("Inactivate " + Name);
			this.task.IsComplete = true;
			this.backend.UpdateTask (this);
		}
		
		/// <summary>
		/// Completes the task
		/// </summary>
		public override void Complete ()
		{
                       Logger.Debug ("Complete : " + Name);
                       this.task.IsComplete = true;
		       this.backend.UpdateTask (this);
		}
		
		/// <summary>
		/// Deletes the task
		/// </summary>
		public override void Delete ()
		{
			Logger.Info ("Delete : Not implemented");
		}
		
		/// <summary>
		/// Adds a note to a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override INote CreateNote(string text)
		{
			Logger.Debug ("CreateNote : " + text);

			HmNote hmNote = new HmNote (this.task);
			hmNote.Text = text;

			notes.Clear ();
			notes.Add (hmNote);

			this.backend.UpdateTask (this);

			return hmNote;
		}
		
		/// <summary>
		/// Deletes a note from a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override void DeleteNote(INote note)
		{
		       Logger.Debug ("DeleteNote : " + note.Text);

		       foreach(HmNote hmNote in notes) {
			       if(string.Equals (hmNote.Text, note.Text)) {
				       hmNote.Text = null;
				       notes.Remove(hmNote);
				       break;
			       }
		       }

		       this.backend.UpdateTask (this);
		}		

		/// <summary>
		/// Saves a note in a task
		/// </summary>
		/// <param name="note">
		/// A <see cref="INote"/>
		/// </param>
		public override void SaveNote(INote note)
		{		
			Logger.Debug ("SaveNote : " + note.Text);

			HmNote hmNote = new HmNote (this.task);
			hmNote.Text = note.Text;

			notes.Clear ();
			notes.Add (hmNote);

			this.backend.UpdateTask (this);
		}		

		public int CompareTo (ITask task)
		{
			bool isSameDate = true;
			if (DueDate.Year != task.DueDate.Year
					|| DueDate.DayOfYear != task.DueDate.DayOfYear)
				isSameDate = false;
			
			if (isSameDate == false) {
				if (DueDate == DateTime.MinValue) {
					// No due date set on this task. Since we already tested to see
					// if the dates were the same above, we know that the passed-in
					// task has a due date set and it should be "higher" in a sort.
					return 1;
				} else if (task.DueDate == DateTime.MinValue) {
					// This task has a due date and should be "first" in sort order.
					return -1;
				}
				
				int result = DueDate.CompareTo (task.DueDate);
				
				if (result != 0) {
					return result;
				}
			}
			
			// The due dates match, so now sort based on priority and name
			return CompareByPriorityAndName (task);
		}
		
		public int CompareToByCompletionDate (ITask task)
		{
			bool isSameDate = true;
			if (CompletionDate.Year != task.CompletionDate.Year
					|| CompletionDate.DayOfYear != task.CompletionDate.DayOfYear)
				isSameDate = false;
			
			if (isSameDate == false) {
				if (CompletionDate == DateTime.MinValue) {
					// No completion date set for some reason.  Since we already
					// tested to see if the dates were the same above, we know
					// that the passed-in task has a CompletionDate set, so the
					// passed-in task should be "higher" in the sort.
					return 1;
				} else if (task.CompletionDate == DateTime.MinValue) {
					// "this" task has a completion date and should evaluate
					// higher than the passed-in task which doesn't have a
					// completion date.
					return -1;
				}
				
				return CompletionDate.CompareTo (task.CompletionDate);
			}
			
			// The completion dates are the same, so no sort based on other
			// things.
			return CompareByPriorityAndName (task);
		}
		#endregion // Methods
		
		#region Private Methods
		
		private int CompareByPriorityAndName (ITask task)
		{
			// The due dates match, so now sort based on priority
			if (Priority != task.Priority) {
				switch (Priority) {
				case TaskPriority.High:
					return -1;
				case TaskPriority.Medium:
					if (task.Priority == TaskPriority.High) {
						return 1;
					} else {
						return -1;
					}
				case TaskPriority.Low:
					if (task.Priority == TaskPriority.None) {
						return -1;
					} else {
						return 1;
					}
				case TaskPriority.None:
					return 1;
				}
			}
			
			// Due dates and priorities match, now sort by name
			return Name.CompareTo (task.Name);
		}
		#endregion // Private Methods

		#region Debug Methods
		public void Dump ()
		{
			this.task.Dump();
		}
		#endregion
	}
}
