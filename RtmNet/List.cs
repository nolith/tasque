using System;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;

namespace RtmNet
{
	/// <summary>
	/// Contains a list of <see cref="Contact"/> items for a given user.
	/// </summary>
	[System.Serializable]
	public class Lists
	{
		/// <summary>
		/// An array of <see cref="Contact"/> items for the user.
		/// </summary>
		[XmlElement("list", Form=XmlSchemaForm.Unqualified)]
		public List[] listCollection = new List[0];
	}

	/// <summary>
	/// Contains details of a contact for a particular user.
	/// </summary>
	[System.Serializable]
	public class List
	{
		/// <summary>
		/// The user id of the contact.
		/// </summary>
		[XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
		public string ID;
    
		/// <summary>
		/// The username (or screen name) of the contact.
		/// </summary>
		[XmlAttribute("name", Form=XmlSchemaForm.Unqualified)]
		public string Name;
    
		/// <summary>
		/// Is this contact marked as a friend contact?
		/// </summary>
		[XmlAttribute("deleted", Form=XmlSchemaForm.Unqualified)]
		public int Deleted;
    
		/// <summary>
		/// Is this user marked a family contact?
		/// </summary>
		[XmlAttribute("locked", Form=XmlSchemaForm.Unqualified)]
		public int Locked;
    
		/// <summary>
		/// Unsure how to even set this!
		/// </summary>
		[XmlAttribute("archived", Form=XmlSchemaForm.Unqualified)]
		public int Archived;

		/// <summary>
		/// Is the user online at the moment RtmLive)
		/// </summary>
		[XmlAttribute("position", Form=XmlSchemaForm.Unqualified)]
		public int Position;

		/// <summary>
		/// Is the user online at the moment RtmLive)
		/// </summary>
		[XmlAttribute("smart", Form=XmlSchemaForm.Unqualified)]
		public int Smart;

		
		/// <summary>
		/// An array of TaskSeries objects
		/// </summary>
		[XmlElement("taskseries", Form=XmlSchemaForm.Unqualified)]
		public TaskSeries[] TaskSeriesCollection = new TaskSeries[0];		
	}	
}