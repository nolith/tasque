/***************************************************************************
 *  Utilities.cs
 *
 *  Copyright (C) 2007 Novell, Inc.
 *  Written by:
 * 		Calvin Gaisford <calvinrg@gmail.com>
 *		Boyd Timothy <btimothy@gmail.com>
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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using Mono.Unix;


namespace Tasque
{
	internal class Utilities
	{
		public static string ReplaceString (string originalString, string searchString, string replaceString)
		{
			return Utilities.ReplaceString (originalString, searchString, replaceString, false);
		}

		public static string EscapeForJavaScript (string text)
		{
			// Replace all single quote characters with: &apos;
			text = ReplaceString (text, "'", "&apos;", true);

			// Replace all double quote characters with: &quot;
			text = ReplaceString (text, "\"", "&quot;", true);

			return text;
		}

		public static string ReplaceString (
				string originalString,
				string searchString,
				string replaceString,
				bool replaceAllOccurrences)
		{
			string replacedString = originalString;
			int pos = replacedString.IndexOf (searchString);
			while (pos >= 0) {
				replacedString = string.Format (
						"{0}{1}{2}",
						replacedString.Substring (0, pos),
						replaceString,
						replacedString.Substring (pos + searchString.Length));

				if (!replaceAllOccurrences)
					break;

				pos = replacedString.IndexOf (searchString);
			}

			return replacedString;
		}

		public static Gdk.Pixbuf GetIcon (string iconName, int size)
		{
			try {
				return Gtk.IconTheme.Default.LoadIcon (iconName, size, 0);
			} catch (GLib.GException) {}

			try {
				Gdk.Pixbuf ret = new Gdk.Pixbuf (null, iconName + ".png");
				return ret.ScaleSimple (size, size, Gdk.InterpType.Bilinear);
			} catch (ArgumentException) {}

			Logger.Debug ("Unable to load icon '{0}'.", iconName);
			return null;
		}

		public static bool ParseNameValuePair (string pair, out string name, out string nameValue)
		{
			name = null;
			nameValue = null;
			if (pair == null || pair.Trim ().Length == 0 || pair.IndexOf ("=") <= 0)
				//				throw new ArgumentException ("The pair passed in does not contain a valid name/value");
				return false;

			int equalsPos = pair.IndexOf ("=");
			name = pair.Substring (0, equalsPos);

			if (pair.Length <= equalsPos)
				return false; // Not enough room for a value to exist

			nameValue = pair.Substring (equalsPos + 1);

			if (name == null || name.Length < 1 || nameValue == null || nameValue.Length < 1)
				return false;

			return true;
		}

		/// <summary>
		/// Create the specified path if needed
		/// <param name="path">The path to create if it does not exist</param>
		/// </summary>
		public static void CreateDirectoryIfNeeded (string path)
		{
			if (!System.IO.Directory.Exists (path)) {
				try {
					System.IO.Directory.CreateDirectory (path);
				} catch {
					Logger.Warn ("Couldn't create the directory: {0}", path);
				}
			}
		}


		public static Gdk.Pixbuf GetPhotoFromUri(string uri)
		{
			Gdk.Pixbuf pixbuf = null;

			System.Net.HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(uri);

			// Send request to send the file
			request.Method = "GET";
			//request.ContentLength = 0;
			// request.GetRequestStream().Close();

			// Read the response to the request
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			if( response.StatusCode == HttpStatusCode.OK ) {
				Logger.Debug("Response was OK");

				byte[] buffer = new byte[response.ContentLength];
				int sizeRead = 0;
				int totalRead = 0;
				Stream stream = response.GetResponseStream();
				Logger.Debug("About to read photo of size {0}", response.ContentLength);

				try {

					do {
						sizeRead = stream.Read(buffer, totalRead, (int)(response.ContentLength - totalRead));
						totalRead += sizeRead;
						Logger.Debug("SizeRead = {0}, totalRead = {1}", sizeRead, totalRead);
					} while( (sizeRead > 0) && (totalRead < response.ContentLength) );

					Logger.Debug("We Read the photo and it's {0} bytes", totalRead);
					Logger.Debug("The content length is {0} bytes", response.ContentLength);

					stream.Close();
				} catch (Exception e) {
					Logger.Debug("Exception when reading file from stream: {0}", e.Message);
					Logger.Debug("Exception {0}", e);
				}

				pixbuf = new Gdk.Pixbuf(buffer);

			} else {
				Logger.Debug("Unable to get the photo because {0}", response.StatusDescription);
			}
			return pixbuf;
		}

		public static string GetGravatarUri(string gravatarID)
		{
			System.Text.StringBuilder image = new System.Text.StringBuilder();
			image.Append("http://www.gravatar.com/avatar.php?");
			image.Append("gravatar_id=");
			image.Append(gravatarID);
			//image.Append("&rating=G");
			image.Append("&size=48");

			Logger.Debug("The Gravatar Uri is {0}", image.ToString());

			return image.ToString();
		}


		// Create an md5 sum string of this string
		public static string GetMd5Sum(string str)
		{
			System.Security.Cryptography.MD5CryptoServiceProvider x = 
				new System.Security.Cryptography.MD5CryptoServiceProvider();

			byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
			bs = x.ComputeHash(bs);

			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in bs)
			{
				s.Append(b.ToString("x2").ToLower());
			}

			return s.ToString();
			/*



			// First we need to convert the string into bytes, which
			// means using a text encoder.
			Encoder enc = System.Text.Encoding.Unicode.GetEncoder();

			// Create a buffer large enough to hold the string
			byte[] unicodeText = new byte[str.Length * 2];
			enc.GetBytes(str.ToCharArray(), 0, str.Length, unicodeText, 0, true);

			// Now that we have a byte array we can ask the CSP to hash it
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(unicodeText);

			// Build the final string by converting each byte
			// into hex and appending it to a StringBuilder
			StringBuilder sb = new StringBuilder();
			for (int i=0;i<result.Length;i++)
			{
			sb.Append(result[i].ToString("X2"));
			}

			// And return it
			return sb.ToString();
			 */
		}
		
		/// <summary>
		/// Get a string that is more friendly/pretty for the specified date.
		/// For example, "Today, 3:00 PM", "4 days ago, 9:20 AM".
		/// <param name="date">The DateTime to evaluate</param>
		/// <param name="show_time">If true, output the time along with the
		/// date</param>
		/// </summary>
		public static string GetPrettyPrintDate (DateTime date, bool show_time)
		{
			string pretty_str = String.Empty;
			DateTime now = DateTime.Now;
			string short_time = date.ToShortTimeString ();

			if (date.Year == now.Year) {
				if (date.DayOfYear == now.DayOfYear)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Today, {0}"),
					                            short_time) :
					             Catalog.GetString ("Today");
				else if (date.DayOfYear < now.DayOfYear
				                && date.DayOfYear == now.DayOfYear - 1)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Yesterday, {0}"),
					                            short_time) :
					             Catalog.GetString ("Yesterday");
				else if (date.DayOfYear < now.DayOfYear
				                && date.DayOfYear > now.DayOfYear - 6)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("{0} days ago, {1}"),
					                            now.DayOfYear - date.DayOfYear, short_time) :
					             String.Format (Catalog.GetString ("{0} days ago"),
					                            now.DayOfYear - date.DayOfYear);
				else if (date.DayOfYear > now.DayOfYear
				                && date.DayOfYear == now.DayOfYear + 1)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("Tomorrow, {0}"),
					                            short_time) :
					             Catalog.GetString ("Tomorrow");
				else if (date.DayOfYear > now.DayOfYear
				                && date.DayOfYear < now.DayOfYear + 6)
					pretty_str = show_time ?
					             String.Format (Catalog.GetString ("In {0} days, {1}"),
					                            date.DayOfYear - now.DayOfYear, short_time) :
					             String.Format (Catalog.GetString ("In {0} days"),
					                            date.DayOfYear - now.DayOfYear);
				else
					pretty_str = show_time ?
					             date.ToString (Catalog.GetString ("MMMM d, h:mm tt")) :
					             date.ToString (Catalog.GetString ("MMMM d"));
			} else if (date == DateTime.MinValue)
				pretty_str = Catalog.GetString ("No Date");
			else
				pretty_str = show_time ?
				             date.ToString (Catalog.GetString ("MMMM d yyyy, h:mm tt")) :
				             date.ToString (Catalog.GetString ("MMMM d yyyy"));

			return pretty_str;
		}
		
		public static string GetLocalizedDayOfWeek (System.DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek) {
			case DayOfWeek.Sunday:
				return Catalog.GetString ("Sunday");
			case DayOfWeek.Monday:
				return Catalog.GetString ("Monday");
			case DayOfWeek.Tuesday:
				return Catalog.GetString ("Tuesday");
			case DayOfWeek.Wednesday:
				return Catalog.GetString ("Wednesday");
			case DayOfWeek.Thursday:
				return Catalog.GetString ("Thursday");
			case DayOfWeek.Friday:
				return Catalog.GetString ("Friday");
			case DayOfWeek.Saturday:
				return Catalog.GetString ("Saturday");
			}
			
			return string.Empty;
		}
		
		/// <summary>
		/// Parse the task name in order to derive due date information.
		/// </summary>
		/// <param name="enteredTaskText">
		/// A <see cref="System.String"/> representing the text entered
		/// into the task name field.
		/// </param>
		/// <param name="parsedTaskText">
		/// The enteredTaskText with the due date section of the string
		/// removed.
		/// </param>
		/// <param name="parsedDueDate">
		/// The due date derived from enteredTaskText, or
		/// DateTime.MinValue if no date information was found.
		/// </param>
		public static void ParseTaskText (string enteredTaskText, out string parsedTaskText, out DateTime parsedDueDate)
		{
			// First, look for ways that the right side of the entered
			// text can be directly parsed as a date
			string[] words = enteredTaskText.Split (' ');
			for (int i = 1; i < words.Length; i++) {
				string possibleDate = string.Join (" ", words, i, words.Length - i);
				DateTime result;
				if (DateTime.TryParse (possibleDate, out result)) {
					// Favor future dates, unless year was specifically mentioned
					if (!possibleDate.Contains (result.Year.ToString ()))
						while (result < DateTime.Today)
							result = result.AddYears (1);
					
					// Set task due date and return the task
					// name with the date part removed.
					parsedDueDate = result;
					parsedTaskText = string.Join (" ", words, 0, i);
					return;
				}
			}
			
			// Then try some more natural language parsing
			
			// A regular expression to capture a task that is due today
			string today = Catalog.GetString (@"^(?<task>.+)\s+today\W*$");
			// A regular expression to capture a task that is due tomorrow
			string tomorrow = Catalog.GetString (@"^(?<task>.+)\s+tomorrow\W*$");
			
			// Additional regular expressions to consider using
			//string abbrevDate = Catalog.GetString (@"^(?<task>.+)(on )?(the )?(?<day>\d{1,2})((th)|(nd)|(rd)|(st))\W*$");
			//string nextDayName = Catalog.GetString (@"^(?<task>.+)(on )?next\s+(?<day>[a-z]+)\W*$");
			//string dayName = Catalog.GetString (@"^(?<task>.+)\s+(on )?(?<day>[a-z]+)\W*$");
			
			Match match = Regex.Match (enteredTaskText, today, RegexOptions.IgnoreCase);
			if (match.Success) {
				string trimmedTaskText = match.Groups ["task"].Value;
				if (!string.IsNullOrEmpty (trimmedTaskText)) {
					parsedDueDate = DateTime.Now;
					parsedTaskText = trimmedTaskText;
					return;
				}
			}
			
			match = Regex.Match (enteredTaskText, tomorrow, RegexOptions.IgnoreCase);
			if (match.Success) {
				string trimmedTaskText = match.Groups ["task"].Value;
				if (!string.IsNullOrEmpty (trimmedTaskText)) {
					parsedDueDate = DateTime.Now.AddDays (1);
					parsedTaskText = trimmedTaskText;
					return;
				}
			}
			
			parsedTaskText = enteredTaskText;
			parsedDueDate = DateTime.MinValue;
			return;
		}
	}
}
