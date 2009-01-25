/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// Task.cs
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
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace Hiveminder
{
	[XmlRoot("value", Namespace="", IsNullable=false)]
	[Serializable]
	public class Task
	{
		private string description;
		
		#region PublicProperties
		
		[XmlElement("id", Form=XmlSchemaForm.Unqualified)]
		public string Id;
		
		[XmlElement("priority", Form=XmlSchemaForm.Unqualified)]
		public int Priority;
		
		[XmlElement("complete", Form=XmlSchemaForm.Unqualified)]
		public bool IsComplete;
		
		[XmlElement("summary", Form=XmlSchemaForm.Unqualified)]
		public string Summary;

		[XmlElement("created", Form=XmlSchemaForm.Unqualified)]
		public string Created;

		[XmlElement("started", Form=XmlSchemaForm.Unqualified)]
		public string Started;

		[XmlElement("due", Form=XmlSchemaForm.Unqualified)]
		public string Due;

		[XmlElement("description", Form=XmlSchemaForm.Unqualified)]
		public string Description;
		
		[XmlElement("group_id", Form=XmlSchemaForm.Unqualified)]
		public string GroupId;
		
		#endregion
		
		public Task()
		{
			
		}
		
		#region Debug Functions
		
		public void Dump ()
		{
			Console.WriteLine ("id : " + this.Id);
			Console.WriteLine ("priority : " + this.Priority);
			Console.WriteLine ("summary : " + this.Summary);
			Console.WriteLine ("started : " + this.Started);
			Console.WriteLine ("created : " + this.Created);
			Console.WriteLine ("Due : " + this.Due);
			Console.WriteLine ("complete : " + this.IsComplete);
			Console.WriteLine ("Description : " + this.Description);
		}

		#endregion 

		public string ToUrlEncodedString
		{
			get {
				string url = "summary=" + this.Summary + "&" +
					"description=" + this.Description + "&" +
 					"priority=" + this.Priority + "&" +
					"complete=" + (this.IsComplete ? "1" : "0") + "&" +
					"id=" + this.Id + "&" +
					"due=" + this.Due + "&" +
					"started" + this.Started + "&" +
					"created" + this.Created + "&" +
					"group_id" + this.GroupId + "&";

				return url;
			}
		}
	}
}
