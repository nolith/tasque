/***************************************************************************
 *  Preferences.cs
 *
 *  Copyright (C) 2008 Novell, Inc.
 *  Written by:
 *      Calvin Gaisford <calvinrg@gmail.com>
 *      Boyd Timothy <btimothy@gmail.com>
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
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Tasque 
{
	// <summary>
	// Class used to store Tasque preferences
	// </summary>
	public class Preferences
	{
		private System.Xml.XmlDocument document;
		private string location;
		
		public const string AuthTokenKey = "AuthToken";
		public const string CurrentBackend = "CurrentBackend";
		public const string InactivateTimeoutKey = "InactivateTimeout";
		public const string SelectedCategoryKey = "SelectedCategory";
		public const string ParseDateEnabledKey = "ParseDateEnabled";
		public const string TodayTaskTextColor = "TodayTaskTextColor";
		public const string OverdueTaskTextColor = "OverdueTaskTextColor";

		/// <summary>
		/// A list of category names to show in the TaskWindow when the "All"
		/// category is selected.
		/// </summary>
		public const string HideInAllCategory = "HideInAllCategory";
		public const string ShowCompletedTasksKey = "ShowCompletedTasks";
		public const string UserNameKey = "UserName";
		public const string UserIdKey = "UserID";
		
		/// <summary>
		/// This setting allows a user to specify how many completed tasks to
		/// show in the Completed Tasks Category.  The setting should be one of:
		/// "Yesterday", "Last7Days", "LastMonth", "LastYear", or "All".
		/// </summary>
		/// <param name="settingKey">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public const string CompletedTasksRange = "CompletedTasksRange";
		
		public delegate void SettingChangedHandler (Preferences preferences,
													string settingKey);
		public event SettingChangedHandler SettingChanged;
		
		public string Get (string settingKey)
		{
			if (settingKey == null || settingKey.Trim () == string.Empty)
				throw new ArgumentNullException ("settingKey", "Preferences.Get() called with a null/empty settingKey");
			
			string xPath = string.Format ("//{0}", settingKey.Trim ());
			XmlNode node = document.SelectSingleNode (xPath);
			if (node == null || !(node is XmlElement))
				return SetDefault (settingKey);
			
			XmlElement element = node as XmlElement;
			if( (element == null) || (element.InnerText.Length < 1) )
				return SetDefault (settingKey);
			else
				return element.InnerText;
		}
		
		private string SetDefault (string settingKey)
		{
			string val = GetDefault (settingKey);
			if (val != null)
				Set (settingKey, val);
			return val;
		}
		
		private string GetDefault (string settingKey)
		{
			switch (settingKey) {
			case ParseDateEnabledKey:
				return true.ToString ();
			case TodayTaskTextColor:
				return "#181AB7";
			case OverdueTaskTextColor:
				return "#EB3320";
			default:
				return null;
			}
		}
		
		public void Set (string settingKey, string settingValue)
		{
			if (settingKey == null || settingKey.Trim () == string.Empty)
				throw new ArgumentNullException ("settingKey", "Preferences.Set() called with a null/empty settingKey");
			
			string xPath = string.Format ("//{0}", settingKey.Trim ());
			XmlNode node = document.SelectSingleNode (xPath);
			XmlElement element = null;
			if (node != null && node is XmlElement)
				element = node as XmlElement;
			
			if (element == null) {
				element = document.CreateElement(settingKey);
				document.DocumentElement.AppendChild (element);
			}
			
			if (settingValue == null)
				element.InnerText = string.Empty;
			else
				element.InnerText = settingValue;
			
			SavePrefs();
			
			NotifyHandlersOfSettingChange (settingKey.Trim ());
		}
		
		public int GetInt (string settingKey)
		{
			string val = Get (settingKey);
			if (val == null)
				return -1;
			
			return int.Parse (val);
		}
		
		public void SetInt (string settingKey, int settingValue)
		{
			Set (settingKey, string.Format ("{0}", settingValue));
		}
		
		public bool GetBool (string settingKey)
		{
			string val = Get (settingKey);
			if (val == null)
				return false;
			
			return bool.Parse (val);
		}
		
		public void SetBool (string settingKey, bool settingValue)
		{
			Set (settingKey, settingValue.ToString ());
		}
		
		public List<string> GetStringList (string settingKey)
		{
			if (settingKey == null || settingKey.Trim () == string.Empty)
				throw new ArgumentNullException ("settingKey", "Preferences.GetStringList() called with a null/empty settingKey");
			
			List<string> stringList = new List<string> ();
			
			// Select all nodes whose parent is the settingKey
			string xPath = string.Format ("//{0}/*", settingKey.Trim ());
			XmlNodeList list = document.SelectNodes (xPath);
			if (list == null)
				return stringList;
			
			foreach (XmlNode node in list) {
				if (node.InnerText != null && node.InnerText.Length > 0)
					stringList.Add (node.InnerText);
			}
			
			return stringList;
		}
		
		public void SetStringList (string settingKey, List<string> stringList)
		{
			if (settingKey == null || settingKey.Trim () == string.Empty)
				throw new ArgumentNullException ("settingKey", "Preferences.SetStringList() called with null/empty settingKey");
			
			// Assume that the caller meant to null out an existing list
			if (stringList == null)
				stringList = new List<string> ();
			
			// Select the specific node
			string xPath = string.Format ("//{0}", settingKey.Trim ());
			XmlNode node = document.SelectSingleNode (xPath);
			XmlElement element = null;
			if (node != null && node is XmlElement) {
				element = node as XmlElement;
				// Clear out any old children
				if (element.HasChildNodes) {
					element.RemoveAll ();
				}
			}
			
			if (element == null) {
				element = document.CreateElement(settingKey);
				document.DocumentElement.AppendChild (element);
			}
			
			foreach (string listItem in stringList) {
				XmlElement child = document.CreateElement ("list-item");
				child.InnerText = listItem;
				element.AppendChild (child);
			}
			
			SavePrefs();
			
			NotifyHandlersOfSettingChange (settingKey.Trim ());
		}

		public Preferences (string confDir)
		{
			document = new XmlDocument();
			location = Path.Combine (confDir, "preferences");
			if(!File.Exists(location)) {
				CreateDefaultPrefs();
			} else {
				try {
					document.Load(location);
				} catch {
					CreateDefaultPrefs ();
					document.Load (location);
				}
			}
			
			ValidatePrefs ();
		}
		
		/// <summary>
		/// Validate existing preferences just in case we're running on a
		/// machine that already has an existing file without having the
		/// settings specified here.
		/// </summary>
		private void ValidatePrefs ()
		{
			if (GetInt (Preferences.InactivateTimeoutKey) <= 0)
				SetInt (Preferences.InactivateTimeoutKey, 5);
		}


		private void SavePrefs()
		{
			XmlTextWriter writer = new XmlTextWriter(location, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			document.WriteTo( writer );
			writer.Flush();
			writer.Close();
		}


		private void CreateDefaultPrefs()
		{
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(location));

       			document.LoadXml(
       				"<tasqueprefs></tasqueprefs>");
				SavePrefs();
/* 
		       // Create a new element node.
		       XmlNode newElem = doc.CreateNode("element", "pages", "");  
		       newElem.InnerText = "290";
		     
		       Console.WriteLine("Add the new element to the document...");
		       XmlElement root = doc.DocumentElement;
		       root.AppendChild(newElem);
		     
		       Console.WriteLine("Display the modified XML document...");
		       Console.WriteLine(doc.OuterXml);
*/
			} catch (Exception e) {
				Logger.Debug("Exception thrown in Preferences {0}", e);
				return;
			}

		}
		
		/// <summary>
		/// Notify all SettingChanged event handlers that the specified
		/// setting has changed.
		/// </summary>
		/// <param name="settingKey">
		/// A <see cref="System.String"/>.  The setting that changed.
		/// </param>
		private void NotifyHandlersOfSettingChange (string settingKey)
		{
			// Notify SettingChanged handlers of the change
			if (SettingChanged != null) {
				try {
					SettingChanged (this, settingKey);
				} catch (Exception e) {
					Logger.Warn ("Exception calling SettingChangedHandlers for setting '{0}': {1}",
								 settingKey,
								 e.Message);
				}
			}
		}
	}
}
