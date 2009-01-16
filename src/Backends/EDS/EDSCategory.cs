/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// EDSCategory.cs
// User: Johnny <johnnyjacob@gmail.com>
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Tasque;
using Evolution;

namespace Tasque.Backends.EDS
{
       public class EDSCategory : ICategory
       {
               private string name;
               private string uid;
               private Evolution.Cal taskList;
	       private bool isSystem;

               public EDSCategory (Evolution.Source source, Evolution.Cal taskList)
               {
                       this.name = source.Name;
                       this.uid = source.Uid;
                       this.taskList = taskList;

		       //Based on evolution/calendar/gui/e-cal-popup.c : 
		       //e_cal_popup_target_new_source
		       this.isSystem = (string.Compare (source.RelativeUri, "system") == 0);       
               }

               public EDSCategory (Evolution.Source source)
               {
                       this.name = source.Name;
                       this.uid = source.Uid;
                       this.taskList = new Cal (source, CalSourceType.Todo);
		       this.isSystem = (string.Compare (source.RelativeUri, "system") == 0);
               }

	       public bool IsSystem
	       {
		       get { return isSystem; }
	       }
		
               public string Name
               {
                       get { return name; }
               }

               public string UID
               {
                       get { return uid; }
               }

               public Evolution.Cal TaskList
               {
                       get { return taskList;}
               }

               public bool ContainsTask(ITask task)
               {
                       if(task.Category is EDSCategory)
                               return (task.Category.Name.CompareTo(name) == 0);
                       else
                               return false;
               }

       }
}
