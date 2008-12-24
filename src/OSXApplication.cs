

using System;
using System.Runtime.InteropServices;
using Gtk;
using IgeMacIntegration;
using Mono.Unix;


namespace Tasque
{
	
	
	public class OSXApplication : GtkApplication
	{
		private const string osxMenuXml =@"
<ui>

  <menubar name=""MainMenu"">
    <menu name=""FileMenu"" action=""FileMenuAction"">


      <menuitem action=""RefreshAction""/>

    </menu>
    <menu name=""WindowMenu"" action=""WindowMenuAction"">
      <menuitem action=""ShowTasksAction""/>
    </menu>
  </menubar>
</ui>
";
		public override void InitializeIdle ()
		{
			ActionGroup mainMenuActionGroup = new ActionGroup ("Main");
			mainMenuActionGroup.Add (new ActionEntry [] {
				new ActionEntry ("FileMenuAction",
				                 null,
				                 Catalog.GetString ("_File"),
				                 null,
				                 null,
				                 null),
				new ActionEntry ("WindowMenuAction",
				                 null,
				                 Catalog.GetString ("_Window"),
				                 null,
				                 null,
				                 null)
			});
			
			UIManager uiManager = Application.Instance.UIManager;
			
			uiManager.AddUiFromString (osxMenuXml);
			uiManager.InsertActionGroup (mainMenuActionGroup, 1);
			
			// This totally doesn't work...is my lib too old?
			IgeMacDock dock = new IgeMacDock();
			dock.Clicked += delegate (object sender, EventArgs args) {TaskWindow.ShowWindow ();};
			dock.QuitActivate += delegate (object sender, EventArgs args) { Application.Instance.Quit (); };
			
			MenuShell mainMenu = uiManager.GetWidget ("/MainMenu") as MenuShell;
			mainMenu.Show ();
			IgeMacMenu.MenuBar = mainMenu;
			

			MenuItem about_item = uiManager.GetWidget ("/TrayIconMenu/AboutAction") as MenuItem;
			MenuItem prefs_item = uiManager.GetWidget ("/TrayIconMenu/PreferencesAction") as MenuItem;
			MenuItem quit_item  = uiManager.GetWidget ("/TrayIconMenu/QuitAction") as MenuItem;
			

			IgeMacMenuGroup about_group = IgeMacMenu.AddAppMenuGroup ();
			IgeMacMenuGroup prefs_group = IgeMacMenu.AddAppMenuGroup ();

			about_group.AddMenuItem (about_item, null);
			prefs_group.AddMenuItem (prefs_item, null);
			
			IgeMacMenu.QuitMenuItem = quit_item;
			
			// Hide StatusIcon
			Application.Instance.Tray.Visible = false;
		}
			
			[DllImport ("libc", EntryPoint="system")]
			public static extern int system (string command);
			
			public override void OpenUrl (string url)
			{
				system ("open \"" + url + "\"");
			}

	}
}
