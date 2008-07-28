// NoteWidget.cs created with MonoDevelop
// User: boyd at 5:28 PM 2/13/2008

using System;
using Mono.Unix;
using Gtk;

namespace Tasque
{
	public class NoteWidget : Gtk.Notebook
	{
		private INote note;
		private string text;
		
		private Gtk.Widget viewPage;
		private Gtk.Widget editPage;
		
		private int viewPageId;
		private int editPageId;
		
		//
		// View Page Items
		//
		private Gtk.Label textLabel;
		private Gtk.Button deleteButton;
		private Gtk.Button editButton;
		
		//
		// Edit Page Items
		//
		private Gtk.TextView textView;
		private Gtk.Button cancelButton;
		private Gtk.Button saveButton;
				
		#region Constructors
		public NoteWidget (INote note)
		{
			this.KeyPressEvent += OnNoteWidgetKeyPressed;
			this.note = note;
			this.text = ( (note == null) || (note.Text == null) ) ? string.Empty : note.Text.Trim ();
			
			this.ShowTabs = false;

			viewPage = MakeViewPage ();
			editPage = MakeEditPage ();
			
			// The label below does not need to be translated because it's
			// for debugging purposes only.
			viewPageId = AppendPage (viewPage, new Gtk.Label ("View"));
			
			// The label below does not need to be translated because it's
			// for debugging purposes only.
			editPageId = AppendPage (editPage, new Gtk.Label ("Edit"));
			
			if (text == null || text == string.Empty) {
				// Go into edit mode (switch to the edit page)
				ShowPage (editPageId);
			} else {
				// Go to view mode (switch to the view page)
				ShowPage (viewPageId);
			}
			this.textView.Buffer.Changed += OnTextViewChanged;

		}
		#endregion // Constructors
		
		#region Events
		public event EventHandler TextChanged;
		public event EventHandler DeleteButtonClicked;
		public event EventHandler EditCanceled;
		public event EventHandler EditButtonClicked;
		#endregion // Events
		
		#region Properties
		public INote Note
		{
			get { return note; }
			set { 
				note = value;
				Text = value.Text;
			}
		}
		
		public string Text
		{
			get {
				return text;
			}
			set {
				text = value == null ? string.Empty : value.Trim ();
				
				if (Page == this.viewPageId) {
					textLabel.Text = GLib.Markup.EscapeText (text);
				} else {
					textView.Buffer.Text = text;
				}
			}
		}

		public Gtk.Button SaveButton 
		{
			get { return saveButton; }
		}
		#endregion // Properties
		
		#region Public Methods
		public void FocusTextArea()
		{
			textView.GrabFocus();
		}
		#endregion // Public Methods
		
		#region Private Methods
		private Gtk.Widget MakeViewPage ()
		{
			Gtk.VBox vbox = new Gtk.VBox (false, 0);
			vbox.BorderWidth = 6;
			
			textLabel = new Gtk.Label ();
			textLabel.Xalign = 0;
			textLabel.UseUnderline = false;
			textLabel.Justify = Gtk.Justification.Left;
			textLabel.Wrap = true;
			textLabel.Text = GLib.Markup.EscapeText (text);
			textLabel.Show ();
			vbox.PackStart (textLabel, true, true, 0);
			
			Gtk.HButtonBox hButtonBox = new Gtk.HButtonBox ();
			hButtonBox.Layout = Gtk.ButtonBoxStyle.End;
			
			deleteButton = new Gtk.Button (Gtk.Stock.Delete);
			deleteButton.Clicked += OnDeleteButtonClicked;
			deleteButton.Show ();
			hButtonBox.PackStart (deleteButton, false, false, 0);
			
			editButton = new Gtk.Button (Gtk.Stock.Edit);
			editButton.Clicked += OnEditButtonClicked;
			editButton.Show ();
			hButtonBox.PackStart (editButton, false, false, 0);
			
			hButtonBox.Show ();
			vbox.PackStart (hButtonBox, false, false, 0);
			
			vbox.Show ();
			return vbox;
		}
		
		private Gtk.Widget MakeEditPage ()
		{
			Gtk.VBox vbox = new Gtk.VBox (false, 0);
			vbox.BorderWidth = 6;
			
			Gtk.ScrolledWindow sw = new Gtk.ScrolledWindow ();
			sw.ShadowType = Gtk.ShadowType.EtchedIn;
			sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
			sw.VscrollbarPolicy = Gtk.PolicyType.Automatic;
			
			textView = new Gtk.TextView ();
			textView.WrapMode = Gtk.WrapMode.Word;
			textView.Editable = true;
			textView.Buffer.Text = text;
			textView.CanFocus = true;
			textView.NoShowAll = true;
			sw.Add (textView);
			sw.Show ();
			vbox.PackStart (sw, true, true, 0);
			
			Gtk.HButtonBox hButtonBox = new Gtk.HButtonBox ();
			hButtonBox.Layout = Gtk.ButtonBoxStyle.End;
			
			cancelButton = new Gtk.Button (Gtk.Stock.Cancel);
			cancelButton.Clicked += OnEditCanceled;
			cancelButton.NoShowAll = true;
			hButtonBox.PackStart (cancelButton, false, false, 0);
			
			saveButton = new Gtk.Button (Gtk.Stock.Save);
			saveButton.Clicked += OnSaveButtonClicked;
			saveButton.NoShowAll = true;
			hButtonBox.PackStart (saveButton, false, false, 0);
			
			hButtonBox.Show ();
			vbox.PackStart (hButtonBox, false, false, 6);
			
			vbox.Show ();
			return vbox;
		}
		
		/// <summary>
		/// This is a custom method to show and hide the different notebook
		/// pages.  The reason for this is to specifically be able to hide/show
		/// each notebook's widgets properly so that the notebook can shrink
		/// and grow without taking up too much space in the window.
		/// </summary>
		/// <param name="pageNum">
		/// A <see cref="System.Int32"/>
		/// </param>
		void ShowPage (int pageNum)
		{
			if (Page == pageNum) {
				// We're already on that page, so do nothing
				return;
			}
			
			if (pageNum == viewPageId) {
				ShowViewPage ();
			} else {
				ShowEditPage ();
			}
		}
		
		void ShowViewPage ()
		{
			// Hide all the widgets on the edit page
			textView.Hide ();
			cancelButton.Hide ();
			saveButton.Hide ();

			// Show all the widgets on the view page
			textLabel.Show ();
			deleteButton.Show ();
			editButton.Show ();
			
			// Switch back to the View Page
			Page = viewPageId;
		}
		
		void ShowEditPage ()
		{
			// Set the initial text
			textView.Buffer.Text = text;
			
			// Hide all the widgets on the view page
			textLabel.Hide ();
			deleteButton.Hide ();
			editButton.Hide ();

			// Show all the widgets on the edit page
			textView.Show ();
			cancelButton.Show ();
			saveButton.Show ();
			
			// TODO: Grab the keyboard focus so the cursor is in the textView.
//			Gtk.Widget aParent = this.Parent;
//			while (aParent != null) {
//				// Get our parent Gtk.Window
//				if (aParent is Gtk.Dialog) {
//					(aParent as Gtk.Dialog).Focus = textView;
//					break;
//				}
//				
//				aParent = aParent.Parent;
//			}
			
			// Switch to the Edit Page
			Page = editPageId;
		}
		#endregion // Private Methods
		
		#region Event Handlers
		private void OnDeleteButtonClicked (object sender, EventArgs args)
		{
			if (this.DeleteButtonClicked == null)
				return;
			
			try {
				DeleteButtonClicked (this, EventArgs.Empty);
			} catch (Exception e) {
				Logger.Warn ("Exception in NoteWidget.DeleteButtonClicked handler: {0}", e.Message);
			}
		}
		
		private void OnEditButtonClicked (object sender, EventArgs args)
		{
			ShowPage (editPageId);
			FocusTextArea();
		}
		
		void OnEditCanceled (object sender, EventArgs args)
		{
			// go back to the view page
			ShowPage (viewPageId);

			// Let the event handlers know cancel was pushed
			if (this.EditCanceled == null)
				return;
			try {
				EditCanceled (this, EventArgs.Empty);
			} catch (Exception e) {
				Logger.Warn ("Exception in NoteWidget.DeleteButtonClicked handler: {0}", e.Message);
			}
		}
		
		private void OnSaveButtonClicked (object sender, EventArgs args)
		{
			// Update the text
			text = textView.Buffer.Text.Trim ();
			textLabel.Text = GLib.Markup.EscapeText (text);
			if(note != null)
				note.Text = text;
			
			ShowPage (viewPageId);
			
			// Let the event handlers know the note's been changed
			if (TextChanged != null) {
				try {
					TextChanged (this, EventArgs.Empty);
				} catch (Exception e) {
					Logger.Debug ("Exception in NoteWidget.TextChanged handler: {0}", e.Message);
				}
			}
		}

		void OnTextViewChanged (object sender, EventArgs args)
		{
			if (this.textView.Buffer.Text == null || this.textView.Buffer.Text.Trim() == String.Empty)
				this.saveButton.Sensitive = false;
			else
				this.saveButton.Sensitive = true;
                }

		private void OnNoteWidgetKeyPressed(object o, KeyPressEventArgs args)
		{
			switch (args.Event.Key) {
			case Gdk.Key.Escape:
				// fire the cancel event if the note is 
				// in edit mode.
				if (Page == editPageId) {
					OnEditCanceled(this, EventArgs.Empty);
				}
				break;
			default:
				break;
			}
		}
		#endregion // Event Handlers
	}
}
