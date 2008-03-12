// RtmNote.cs created with MonoDevelop
// User: calvin at 11:05 AM 2/12/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;
using RtmNet;

namespace Tasque.Backends.RtmBackend
{
	public class RtmNote : INote
	{
		Note note;
		
		public RtmNote(Note note)
		{
			this.note = note;
			if( (note.Title != null) && (note.Title.Length > 0) ) {
				note.Text = note.Title + note.Text;
			}
			note.Title = String.Empty;
		}
		
		public string ID
		{
			get { return note.ID; }
		}
    
		public string Text
		{
			get { return note.Text; }
			set { note.Text = value; }
		}
	}
}
