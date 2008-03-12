// IceNote.cs created with MonoDevelop
// User: boyd at 8:32 AM 2/14/2008

using System;

namespace Tasque.Backends.IceCore
{
	public class IceNote : INote
	{
		private IceBackend backend;
		private IceTask task;
		
		public IceNote (IceBackend iceBackend,
						IceTask iceTask)
		{
			backend = iceBackend;
			task = iceTask;
		}
		
		public string Name
		{
			get { return task.Entry.Title; }
			set {}
		}
    
		public string Text
		{
			get { return task.Entry.Description; }
			set {}
		}
	}
}
