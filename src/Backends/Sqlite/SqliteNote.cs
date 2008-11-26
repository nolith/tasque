// SqliteNote.cs created with MonoDevelop
// User: calvin at 10:56 AM 2/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;

namespace Tasque.Backends.Sqlite
{
	public class SqliteNote : INote
	{
		private int id;
		private string text;

		public SqliteNote(int id, string text)
		{
			this.id = id;
			this.text = text;
		}

		public string Text
		{
			get { return this.text; }
			set { this.text = value; }
		}

		public int ID
		{
			get { return this.id; }
		}

	}
}
