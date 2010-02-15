// SqliteTask.cs created with MonoDevelop
// User: boyd at 8:50 PM 2/10/2008

using System;
using Tasque;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace Tasque.Backends.Sqlite
{
	public class SqliteTask : AbstractTask
	{
		private SqliteBackend backend;
		private int id;

		public SqliteTask(SqliteBackend backend, string name)
		{
			this.backend = backend;
			Logger.Debug("Creating New Task Object : {0} (id={1})", name, id);
			name = backend.SanitizeText (name);
			string command = String.Format("INSERT INTO Tasks (Name, DueDate, CompletionDate, Priority, State, Category, ExternalID) values ('{0}','{1}', '{2}','{3}', '{4}', '{5}', '{6}'); SELECT last_insert_rowid();", 
								name, Database.FromDateTime(DateTime.MinValue), Database.FromDateTime(DateTime.MinValue), 
								((int)(TaskPriority.None)), ((int)TaskState.Active), 0, string.Empty );
			this.id = Convert.ToInt32 (backend.Database.ExecuteScalar (command));
		}
		
		public SqliteTask (SqliteBackend backend, int id)
		{
			this.backend = backend;
			this.id = id;
		}		
		
		#region Public Properties
		
		public override string Id
		{
			get { return id.ToString(); }
		}

		internal int SqliteId
		{
			get { return id; } 
		}
		
		public override string Name
		{
			get {
				string command = String.Format("SELECT Name FROM Tasks where ID='{0}'", id);
				return backend.Database.GetSingleString(command);
			}
			set {
				string name = backend.SanitizeText (value);
				string command = String.Format("UPDATE Tasks set Name='{0}' where ID='{1}'", name, id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);
			}
		}
		
		public override DateTime DueDate
		{
			get {
				string command = String.Format("SELECT DueDate FROM Tasks where ID='{0}'", id);
				return backend.Database.GetDateTime(command);
			}
			set {
				string command = String.Format("UPDATE Tasks set DueDate='{0}' where ID='{1}'", Database.FromDateTime(value), id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);				
			}
		}
		
		
		public override DateTime CompletionDate
		{
			get {
				string command = String.Format("SELECT CompletionDate FROM Tasks where ID='{0}'", id);
				return backend.Database.GetDateTime(command);
			}
			set {
				string command = String.Format("UPDATE Tasks set CompletionDate='{0}' where ID='{1}'", Database.FromDateTime(value), id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);				
			}
		}		
		
		
		public override bool IsComplete
		{
			get {
				if (CompletionDate == DateTime.MinValue)
					return false;
				
				return true;
			}
		}
		
		public override TaskPriority Priority
		{
			get {
				string command = String.Format("SELECT Priority FROM Tasks where ID='{0}'", id);
				return (TaskPriority)backend.Database.GetSingleInt(command);
			}
			set {
				string command = String.Format("UPDATE Tasks set Priority='{0}' where ID='{1}'", ((int)value), id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);				
			}
		}

		public override bool HasNotes
		{
			get {
				string command = String.Format("SELECT COUNT(*) FROM Notes WHERE Task='{0}'", id);
				return backend.Database.GetSingleInt(command) > 0;
			}
		}
		
		public override bool SupportsMultipleNotes
		{
			get { return true; }
		}
		
		public override TaskState State
		{
			get { return LocalState; }
		}
		
		public TaskState LocalState
		{
			get {
				string command = String.Format("SELECT State FROM Tasks where ID='{0}'", id);
				return (TaskState)backend.Database.GetSingleInt(command);
			}
			set {
				string command = String.Format("UPDATE Tasks set State='{0}' where ID='{1}'", ((int)value), id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);				
			}
		}

		public override ICategory Category
		{
			get {
				string command = String.Format("SELECT Category FROM Tasks where ID='{0}'", id);
				int catID = backend.Database.GetSingleInt(command);
				SqliteCategory sqCat = new SqliteCategory(backend, catID);
				return sqCat;
			}
			set {
 				string command = String.Format("UPDATE Tasks set Category='{0}' where ID='{1}'", ((int)(value as SqliteCategory).ID), id);
				backend.Database.ExecuteScalar(command);
				backend.UpdateTask(this);
			}
		}
		
		public override List<INote> Notes
		{
			get {
				List<INote> notes = new List<INote>();

				string command = String.Format("SELECT ID, Text FROM Notes WHERE Task='{0}'", id);
				SqliteCommand cmd = backend.Database.Connection.CreateCommand();
				cmd.CommandText = command;
				SqliteDataReader dataReader = cmd.ExecuteReader();
				while(dataReader.Read()) {
					int taskId = dataReader.GetInt32(0);
					string text = dataReader.GetString(1);
					notes.Add (new SqliteNote (taskId, text));
        			}

				return notes;
			}
		}		
		
		#endregion // Public Properties
		
		#region Public Methods
		public override void Activate ()
		{
			// Logger.Debug ("SqliteTask.Activate ()");
			CompletionDate = DateTime.MinValue;
			LocalState = TaskState.Active;
			backend.UpdateTask (this);
		}
		
		public override void Inactivate ()
		{
			// Logger.Debug ("SqliteTask.Inactivate ()");
			CompletionDate = DateTime.Now;
			LocalState = TaskState.Inactive;
			backend.UpdateTask (this);
		}
		
		public override void Complete ()
		{
			//Logger.Debug ("SqliteTask.Complete ()");
			CompletionDate = DateTime.Now;
			LocalState = TaskState.Completed;
			backend.UpdateTask (this);
		}
		
		public override void Delete ()
		{
			//Logger.Debug ("SqliteTask.Delete ()");
			LocalState = TaskState.Deleted;
			backend.UpdateTask (this);
		}
		
		public override INote CreateNote(string text)
		{
			Logger.Debug("Creating New Note Object : {0} (id={1})", text, id);
			text = backend.SanitizeText (text);
			string command = String.Format("INSERT INTO Notes (Task, Text) VALUES ('{0}','{1}'); SELECT last_insert_rowid();", id, text);
			int taskId = Convert.ToInt32 (backend.Database.ExecuteScalar(command));

			return new SqliteNote (taskId, text);
		}
		
		public override void DeleteNote(INote note)
		{
			SqliteNote sqNote = (note as SqliteNote);

 			string command = String.Format("DELETE FROM Notes WHERE ID='{0}'", sqNote.ID);
			backend.Database.ExecuteScalar(command);
		}

		public override void SaveNote(INote note)
		{
			SqliteNote sqNote = (note as SqliteNote);

			string text = backend.SanitizeText (sqNote.Text);
			string command = String.Format("UPDATE Notes SET Text='{0}' WHERE ID='{1}'", text, sqNote.ID);
			backend.Database.ExecuteScalar(command);
		}

		#endregion // Public Methods
	}
}
