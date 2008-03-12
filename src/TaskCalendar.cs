// TaskCalendar.cs created with MonoDevelop
// User: calvin at 7:20 PM 2/13/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using GLib;
using Gtk;
using Tasque;

namespace Tasque
{
	public class TaskCalendar
	{
		private Window popup;
		private DateTime date;
		private Calendar cal;
		private bool show_time;
		int xPos;
		int yPos;
		Gtk.Widget parent;
		int eventCount;

		private ITask task;
		
		private const uint CURRENT_TIME = 0;

		public TaskCalendar(ITask task, Gtk.Widget parent)
		{
			this.task = task;
			
			// If there's no date set (DateTime.MinValue), load the calendar
			// with today's date.
			if (task.DueDate == DateTime.MinValue)
				date = DateTime.Now;
			else
				date = task.DueDate;
			this.parent = parent;
			eventCount = 0;
		}

		public void ShowCalendar()
		{
			popup = new Window(WindowType.Popup);
			popup.Screen = parent.Screen;

			Frame frame = new Frame();
			frame.Shadow = ShadowType.Out;
			frame.Show();

			popup.Add(frame);

			VBox box = new VBox(false, 0);
			box.Show();
			frame.Add(box);

			cal = new Calendar();
			cal.DisplayOptions = CalendarDisplayOptions.ShowHeading
			                     | CalendarDisplayOptions.ShowDayNames
			                     | CalendarDisplayOptions.ShowWeekNumbers;
			                     
			cal.KeyPressEvent += OnCalendarKeyPressed;
			popup.ButtonPressEvent += OnButtonPressed;

			cal.Show();

			Alignment calAlignment = new Alignment(0.0f, 0.0f, 1.0f, 1.0f);
			calAlignment.Show();
			calAlignment.SetPadding(4, 4, 4, 4);
			calAlignment.Add(cal);

			box.PackStart(calAlignment, false, false, 0);

			//Requisition req = SizeRequest();

			parent.GdkWindow.GetOrigin(out xPos, out yPos);
//			popup.Move(x + Allocation.X, y + Allocation.Y + req.Height + 3);
			popup.Move(xPos, yPos);
			popup.Show();
			popup.GrabFocus();

			Grab.Add(popup);

			Gdk.GrabStatus grabbed = Gdk.Pointer.Grab(popup.GdkWindow, true,
			                         Gdk.EventMask.ButtonPressMask
			                         | Gdk.EventMask.ButtonReleaseMask
			                         | Gdk.EventMask.PointerMotionMask, null, null, CURRENT_TIME);

			if (grabbed == Gdk.GrabStatus.Success) {
				grabbed = Gdk.Keyboard.Grab(popup.GdkWindow,
				                            true, CURRENT_TIME);

				if (grabbed != Gdk.GrabStatus.Success) {
					Grab.Remove(popup);
					popup.Destroy();
					popup = null;
				}
			} else {
				Grab.Remove(popup);
				popup.Destroy();
				popup = null;
			}

			cal.DaySelected += OnCalendarDaySelected;
			cal.MonthChanged += OnCalendarMonthChanged;

			cal.Date = date;
		}

		public void HideCalendar(bool update)
		{
			if (popup != null) {
				Grab.Remove(popup);
				Gdk.Pointer.Ungrab(CURRENT_TIME);
				Gdk.Keyboard.Ungrab(CURRENT_TIME);

				popup.Destroy();
				popup = null;
			}

			if (update) {
				date = cal.GetDate();
				// FIXME: If this is ever moved to its own library
				// this reference to Tomboy will obviously have to
				// be removed.
				// Label = Utilities.GetPrettyPrintDate (date, show_time);
			}

			//Active = false;
		}


		private void OnButtonPressed(object o, ButtonPressEventArgs args)
		{
			//Logger.Debug("OnButtonPressed");
			if (popup != null) {
				HideCalendar(false);
			}
		}

		private void OnCalendarDaySelected(object o, EventArgs args)
		{
			eventCount++;
			if(eventCount == 1) {
				// this is only a day selection, set the date and exit
				task.DueDate = cal.GetDate();
				eventCount = 0;
				HideCalendar(true);
			}
			eventCount = 0;
			//HideCalendar(true);
		}
		
		private void OnCalendarMonthChanged(object o, EventArgs args)
		{
			eventCount++;
		}			


		private void OnCalendarKeyPressed(object o, KeyPressEventArgs args)
		{
			//Logger.Debug("OnCalendarKeyPressed");
			switch (args.Event.Key) {
			case Gdk.Key.Escape:
				HideCalendar(false);
				break;
			//case Gdk.Key.KP_Enter:
			//case Gdk.Key.ISO_Enter:
			//case Gdk.Key.Key_3270_Enter:
			//case Gdk.Key.Return:
			//case Gdk.Key.space:
			//case Gdk.Key.KP_Space:
			//	HideCalendar(true);
			//	break;
			default:
				break;
			}
		}

		public DateTime Date
		{
			get {
				return date;
			}
			set {
				date = value;
				//Label = Utilities.GetPrettyPrintDate (date, show_time);
			}
		}

		/// <summary>
		/// If true, both the date and time will be shown.  If false, the time
		/// will be omitted.
		/// </summary>
		public bool ShowTime
		{
			get {
				return show_time;
			}
			set {
				show_time = value;
			}
		}
	}
}