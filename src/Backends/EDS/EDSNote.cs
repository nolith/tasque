/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// EDSNote.cs
// User: Johnny Jacob <johnnyjacob@gmail.com>
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;

namespace Tasque.Backends.EDS
{
       public class EDSNote : INote
       {
	       private string description;

	       public EDSNote(string description)
	       {
		       this.description = description;	
	       }

               public string Text
               {
                       get { return this.description; }
                       set { 
			       this.description = value;
                       }
               }

       }
}
