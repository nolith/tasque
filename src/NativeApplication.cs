using System;

namespace Tasque
{
	public interface INativeApplication
	{
		void Initialize (string locale_dir,
		                 string display_name,
		                 string process_name,
		                 string [] args);

		event EventHandler ExitingEvent;

		void Exit (int exitcode);
		void StartMainLoop ();
		void QuitMainLoop ();

		string ConfDir { get; }

		void OpenUrl (string url);
	}
}
