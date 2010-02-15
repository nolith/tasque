// SqliteCategory.cs created with MonoDevelop
// User: boyd at 9:06 AM 2/11/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;

namespace Tasque.Backends.Sqlite
{
	public class SqliteCategory : ICategory
	{
		private int id;
		SqliteBackend backend;
		
		public int ID
		{
			get { return id; }
		}
		
		public string Name
		{
			get {
				string command = String.Format("SELECT Name FROM Categories where ID='{0}'", id);
				return backend.Database.GetSingleString(command);
			}
			set {
				string command = String.Format("UPDATE Categories set Name='{0}' where ID='{0}'", value, id);
				backend.Database.ExecuteScalar(command);
			}
		}
		
		public string ExternalID
		{
			get {
				string command = String.Format("SELECT ExternalID FROM Categories where ID='{0}'", id);
				return backend.Database.GetSingleString(command);
			}
			set {
				string command = String.Format("UPDATE Categories set ExternalID='{0}' where ID='{0}'", value, id);
				backend.Database.ExecuteScalar(command);
			}
		}
		
		public SqliteCategory (SqliteBackend backend, string name)
		{
			this.backend = backend;
			string command = String.Format("INSERT INTO Categories (Name, ExternalID) values ('{0}', '{1}'); SELECT last_insert_rowid();", name, string.Empty);
			this.id = Convert.ToInt32 (backend.Database.ExecuteScalar(command));
			//Logger.Debug("Inserted category named: {0} with id {1}", name, id);
		}
		
		public SqliteCategory (SqliteBackend backend, int id)
		{
			this.backend = backend;
			this.id = id;
		}

		public bool ContainsTask(ITask task)
		{
			if(task.Category is SqliteCategory)
				return ((task.Category as SqliteCategory).ID == id);

			return false;
		}
		
	}
}
