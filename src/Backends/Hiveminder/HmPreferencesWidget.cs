/* -*- Mode: java; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */
// HmPreferencesWidget.cs
//
// Johnny Jacob <johnnyjacob@gmail.com>
//
// Copyright (c) 2008 Novell Inc.
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
using System.IO;
using System.Text;
using Gtk;

namespace Tasque.Backends.HmBackend
{
	public class HmPreferencesWidget : Gtk.EventBox
	{
		private LinkButton authButton;
		private Label statusLabel;

		// NOTE: This is temporary, we need to use OAuth.
		Gtk.Entry passwordEntry;
		Gtk.Entry emailEntry;
		
		private string username;
		private string password;
		
 		public HmPreferencesWidget () : base()
		{
			HmBackend.LoadCredentials (out this.username, out this.password);
			
			//Fixme Please ! I look UGLY!
			VBox mainVBox = new VBox(false, 12);
			mainVBox.BorderWidth = 10;
			mainVBox.Show();
			Add(mainVBox);

			HBox usernameBox = new HBox(false, 12);
			Gtk.Label emailLabel = new Label ("Email Address");
			usernameBox.Add (emailLabel);
			emailEntry = new Entry(username);
			usernameBox.Add (emailEntry);
			usernameBox.ShowAll();
			mainVBox.PackStart (usernameBox, false, false, 0);

			HBox passwordBox = new HBox(false, 12);
			Gtk.Label passwordLabel = new Label ("Password");
			passwordBox.Add (passwordLabel);
			passwordEntry = new Entry(password);
			passwordBox.Add (passwordEntry);
			passwordBox.ShowAll();
			mainVBox.PackStart (passwordBox, false, false, 0);
			
			// Status message label
			statusLabel = new Label();
			statusLabel.Justify = Gtk.Justification.Center;
			statusLabel.Wrap = true;
			statusLabel.LineWrap = true;
			statusLabel.Show();
			statusLabel.UseMarkup = true;
			statusLabel.UseUnderline = false;

			mainVBox.PackStart(statusLabel, false, false, 0);
			
			authButton = new LinkButton("Click Here to Connect");
			authButton.Show();
			mainVBox.PackStart(authButton, false, false, 0);
			mainVBox.ShowAll();			

			authButton.Clicked += OnAuthButtonClicked;
		}
		
		private void OnAuthButtonClicked (object sender, EventArgs args)
		{
			username = this.emailEntry.Text;
			password = this.passwordEntry.Text;
			HmBackend.PreserveCredentials(username, password);
			
			try {
				Hiveminder.Hiveminder hm = new Hiveminder.Hiveminder(username, password);
			} catch (Exception e) {
				authButton.Label = "Try Again";
			}
		}
	}
}
