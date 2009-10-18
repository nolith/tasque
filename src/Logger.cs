/***************************************************************************
 *  Logger.cs
 *
 *  Copyright (C) 2008 Novell, Inc.
 *  Written by Calvin Gaisford <calvinrg@gmail.com>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */

using System;
using System.IO;

namespace Tasque
{
	public enum LogLevel { Debug, Info, Warn, Error, Fatal };
	
	public interface ILogger
	{
		void Log (LogLevel lvl, string message, params object[] args);
	}
	
	class NullLogger : ILogger
	{
		public void Log (LogLevel lvl, string msg, params object[] args)
		{
		}
	}

	class ConsoleLogger : ILogger
	{
		public void Log (LogLevel lvl, string msg, params object[] args)
		{
			msg = string.Format ("[{0}]: {1}", Enum.GetName (typeof (LogLevel), lvl), msg);
			if (args.Length > 0)
				Console.WriteLine (msg, args);
			else
				Console.WriteLine (msg);
		}
	}

	class FileLogger : ILogger
	{
		StreamWriter log;
		ConsoleLogger console;

		public FileLogger ()
		{
			try {
				log = File.CreateText (Path.Combine (
#if WIN32
					Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData),
#else
					Environment.GetEnvironmentVariable ("HOME"), 
#endif
					".tasque.log"));
				log.Flush ();
			} catch (IOException) {
				// FIXME: Use temp file
			}

			console = new ConsoleLogger ();
		}

		~FileLogger ()
		{
			if (log != null)
#if WIN32
				try {
#endif
					log.Flush ();
#if WIN32
				} catch { }
#endif
		}

		public void Log (LogLevel lvl, string msg, params object[] args)
		{
			console.Log (lvl, msg, args);

			if (log != null) {
				msg = string.Format ("{0} [{1}]: {2}", 
						     DateTime.Now.ToString(), 
						     Enum.GetName (typeof (LogLevel), lvl), 
						     msg);
				if (args.Length > 0)
					log.WriteLine (msg, args);
				else
					log.WriteLine (msg);
				log.Flush();
			}
		}
	}

	// <summary>
	// This class provides a generic logging facility. By default all
	// information is written to standard out and a log file, but other 
	// loggers are pluggable.
	// </summary>
	public static class Logger
	{
		private static LogLevel logLevel = LogLevel.Debug;

		static ILogger logDev = new FileLogger ();

		static bool muted = false;

		public static LogLevel LogLevel
		{
			get { return logLevel; }
			set { logLevel = value; }
		}

		public static ILogger LogDevice
		{
			get { return logDev; }
			set { logDev = value; }
		}

		public static void Debug (string msg, params object[] args)
		{
			Log (LogLevel.Debug, msg, args);
		}

		public static void Info (string msg, params object[] args)
		{
			Log (LogLevel.Info, msg, args);
		}

		public static void Warn (string msg, params object[] args)
		{
			Log (LogLevel.Warn, msg, args);
		}

		public static void Error (string msg, params object[] args)
		{
			Log (LogLevel.Error, msg, args);
		}

		public static void Fatal (string msg, params object[] args)
		{
			Log (LogLevel.Fatal, msg, args);
		}

		public static void Log (LogLevel lvl, string msg, params object[] args)
		{
			if (!muted && lvl >= logLevel)
				logDev.Log (lvl, msg, args);
		}

		public static void Mute ()
		{
			muted = true;
		}

		public static void Unmute ()
		{
			muted = false;
		}
	}
}
