/* PreferencesWindow.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this source distribution.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq;
using System.Collections.Generic;

using Gtk;
using Mono.Addins.Gui;

using Do;
using Do.Addins;

using Do.Platform;

namespace Do.UI
{	
	public partial class PreferencesWindow : Window
	{

		const string HelpUrl = "http://do.davebsd.com/wiki/index.php?title=Using_Do";

		public PreferencesWindow () : 
			base (WindowType.Toplevel)
		{
			Build ();

			TargetEntry[] targets = {
				new TargetEntry ("text/uri-list", 0, 0), 
			};
			
			Gtk.Drag.DestSet (this, DestDefaults.All, targets, Gdk.DragAction.Copy);
			
			btn_close.IsFocus = true;
			// Add notebook pages.
			foreach (IConfigurable page in Pages) {
				notebook.AppendPage (page.GetConfiguration (), new Label (page.Name));
			}
			
			//Sets default page to the plugins tab, since this is the most common reason to
			//open the prefs UI for most users.
			notebook.CurrentPage = Pages.FindIndex (p => p.Name == "Plugins");
		}

		IConfigurable[] pages;
		IConfigurable[] Pages {
			get {
				return pages ?? pages = new IConfigurable[] {
					new GeneralPreferencesWidget (),
					new KeybindingsPreferencesWidget (),
					new ManagePluginsPreferencesWidget (),
					new ColorConfigurationWidget (),
				};
			}
		}

		protected virtual void OnBtnCloseClicked (object sender, EventArgs e)
		{
			Destroy ();
		}

		protected virtual void OnBtnHelpClicked (object sender, EventArgs e)
		{
			Services.Environment.OpenURL (HelpUrl);
		}
	}
}
