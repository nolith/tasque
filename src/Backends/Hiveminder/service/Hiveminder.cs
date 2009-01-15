/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// Hiveminder.cs
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
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;

namespace Hiveminder
{
	class Hiveminder
	{
		private const string BaseURL = "http://hiveminder.com";
		private Cookie COOKIE_HIVEMINDER_SID;
		
		public Hiveminder (string username, string password)
		{
			string cookieValue;
			cookieValue = this.Login(username, password);
			this.COOKIE_HIVEMINDER_SID = new Cookie ("JIFTY_SID_HIVEMINDER", cookieValue, 
								 "/", "hiveminder.com");
		}

		public string CookieValue
		{
			get { return COOKIE_HIVEMINDER_SID.Value; }
		}

		/// <summary>
		/// Login to Hiveminder using HTTP Basic Auth
		/// </summary>
		/// <param name="username">
		/// A <see cref="System.String"/>
		/// </param>
		/// <param name="password">
		/// A <see cref="System.String"/>
		/// </param>
		public string Login (string username, string password)
		{
			string postURL = "/__jifty/webservices/xml";

			//TODO : Fix this.
			string postData = "J%3AA-fnord=Login&J%3AA%3AF-address-fnord="
				+username+"&J%3AA%3AF-password-fnord="+password;

			ASCIIEncoding encoding = new ASCIIEncoding ();
			byte[] encodedPostData = encoding.GetBytes (postData);	

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create (BaseURL + postURL);
			req.CookieContainer = new CookieContainer();	
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.ContentLength = encodedPostData.Length;

			Stream postDataStream = req.GetRequestStream ();
			postDataStream.Write (encodedPostData, 0, encodedPostData.Length);
			
			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			Console.WriteLine (resp.StatusCode);

			//Look for JIFTY_SID_HIVEMINDER
			this.COOKIE_HIVEMINDER_SID = resp.Cookies["JIFTY_SID_HIVEMINDER"];
			
			CheckLoginStatus();
			
			return this.COOKIE_HIVEMINDER_SID.Value;
		}
		/// <summary>
		/// Hack to Check the success of authentication.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		private bool CheckLoginStatus ()
		{
			string responseString;
			responseString = this.Command ("/=/version/");
			
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml (responseString);
			XmlNodeList list = xmlDoc.SelectNodes ("//data/REST");

			//Hack :(
			if (list.Count != 1)
				throw new HiveminderAuthException ("Authentication Failed");
			
			return true;
		}
		
		public string Command (string command, string method, string data)
		{

			Console.WriteLine ("Command : " + command + "Method : "
					   + method + "Data : " + data );

			HttpWebRequest req = (HttpWebRequest)WebRequest.Create (BaseURL + 
										command + ".xml");
			Console.WriteLine (BaseURL+command);
			
			req.CookieContainer = new CookieContainer();
			req.CookieContainer.Add (this.COOKIE_HIVEMINDER_SID);
			req.Method = method;

			//Data for POST
			if ((method.Equals ("POST") || method.Equals ("PUT"))  && data.Length > 0) {
				// We can handle only XML responses.
				req.Accept = "text/xml";
				req.ContentType = "application/x-www-form-urlencoded";

				req.ContentLength = data.Length;
				Stream dataStream = req.GetRequestStream ();
				dataStream.Write(Encoding.UTF8.GetBytes(data), 
						 0, Encoding.UTF8.GetByteCount (data));
				dataStream.Close ();
			}

			HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
			
			string responseString = string.Empty;
			StreamReader sr = new StreamReader(resp.GetResponseStream());
			responseString = sr.ReadToEnd();

			return responseString;
		}

		public string Command (string command, string method)
		{
			return this.Command (command, method, string.Empty);
		}

		public string Command (string command)
		{			
			return this.Command (command, "GET", string.Empty);
		}

		/// <summary>
		/// Get all the Tasks
		/// </summary>
		public XmlNodeList DownloadTasks ()
		{
			string responseString;
			uint i =0;
			
			/*FIXME : Fetches only 15 items.*/
			responseString = this.Command ("/=/search/BTDT.Model.Task/");
		
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml (responseString);
			XmlNodeList list = xmlDoc.SelectNodes ("//value");
			Console.WriteLine (responseString);
			return list;
		}


		/// <summary>
		/// Get all the Groups (Categories)
		/// </summary>
		public XmlNodeList DownloadGroups ()
		{
			string responseString;
			uint i =0;
			
			responseString = this.Command ("/=/action/BTDT.Action.SearchGroup/", "POST");
		
			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.LoadXml (responseString);
			XmlNodeList list = xmlDoc.SelectNodes ("//search");
			Console.WriteLine (responseString);
			return list;
		}

		/// <summary>
		/// Utility function to rename a node.
		/// </summary>
		private XmlNode RenameNode (XmlNode node, string namespaceURI, string qualifiedName)
		{
			if (node.NodeType == XmlNodeType.Element) {
				XmlElement oldElement = (XmlElement) node;
				XmlElement newElement =
					node.OwnerDocument.CreateElement(qualifiedName, namespaceURI);

				while (oldElement.HasAttributes)
					newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes[0]));
				
				while (oldElement.HasChildNodes)
					newElement.AppendChild(oldElement.FirstChild);

				if (oldElement.ParentNode != null)
					oldElement.ParentNode.ReplaceChild(newElement, oldElement);

				return newElement;
			}
			return null;
		}

		/// <summary>
		/// Create a new Task
		/// </summary>
		public Task CreateTask (Task task)
		{
			string responseString;
			Task createdTask;

			XmlSerializer serializer = new XmlSerializer(typeof(Task));
			
			// Can use /=/model/Task also.
			responseString = this.Command ("/=/action/BTDT.Action.CreateTask/", "POST", 
						       task.ToUrlEncodedString);

 			XmlDocument xmlDoc = new XmlDocument();
 			xmlDoc.LoadXml (responseString);

			// Created Task is contained inside 'data'
 			XmlNode node = xmlDoc.SelectSingleNode ("//data");

			// Task's root node is 'value'. 
			node = RenameNode (node, string.Empty, "value");

			createdTask = (Task) serializer.Deserialize(new StringReader(node.OuterXml));
			return createdTask;
		}

		/// <summary>
		/// Update Task on the server.
		/// </summary>
		public Task UpdateTask (Task task)
		{
			string responseString;
			Task updatedTask;

			XmlSerializer serializer = new XmlSerializer(typeof(Task));
			
			// Can use /=/model/Task/id/<fields> with PUT.
			responseString = this.Command ("/=/action/BTDT.Action.UpdateTask/", "POST", 
						       task.ToUrlEncodedString);

 			XmlDocument xmlDoc = new XmlDocument();
 			xmlDoc.LoadXml (responseString);

			// Updated Task is contained inside 'data' root node
 			XmlNode node = xmlDoc.SelectSingleNode ("//data");

			// Task's root node is 'value'. 
			node = RenameNode (node, string.Empty, "value");

			updatedTask = (Task) serializer.Deserialize(new StringReader(node.OuterXml));
			return updatedTask;
		}
	}
}