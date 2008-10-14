/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// HmCategory.cs
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


using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

using Tasque;
using Hiveminder;

namespace Tasque.Backends.HmBackend
{
	public class HmCategory : ICategory
	{
		Group group;
		
		public string Name
		{
			get { return group.Name; }
		}

		public string Id
		{
			get { return group.Id; }
		}
		public bool ContainsTask(ITask task)
		{
			if (task is HmTask) {
				HmTask hmtask = task as HmTask;
				return hmtask.GroupId == this.group.Id;
			}
			
			return false;
		}

		public HmCategory ()
		{
			this.group = new Group();
		}

		public HmCategory (Group group)
		{
			this.group = group;
		}

		public static HmCategory[] GetCategories (XmlNodeList list)
		{
			uint i = 0;
			XmlSerializer serializer = new XmlSerializer(typeof(Group));
			HmCategory[] categories = new HmCategory[list.Count];
			
			foreach (XmlNode node in list ) 
				categories[i++] = new HmCategory ((Group)serializer.Deserialize(new StringReader(node.OuterXml)));
			
			return categories;
		}

		public void Dump ()
		{
			this.group.Dump();
		}
	}
}
