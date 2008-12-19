/* SelectedSelectedTextItem.cs
 *
 * GNOME Do is the legal property of its developers. Please refer to the
 * COPYRIGHT file distributed with this
 * source distribution.
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
using Mono.Unix;

using Do.Platform;

namespace Do.Universe
{

	internal class SelectedTextItem : ProxyItem
	{
		
		static Item TextItem { get; set; }

		static SelectedTextItem ()
		{
			TextItem = Services.UniverseFactory.NewTextItem ("") as Item;
		}

		public static void UpdateText ()
		{
			string text;
			Gtk.Clipboard primary;
			
			primary = Gtk.Clipboard.Get (Gdk.Selection.Primary);
			text = primary.WaitIsTextAvailable () ? primary.WaitForText () : "";
			TextItem = Services.UniverseFactory.NewTextItem (text) as Item;
		}
		
		public override Item Item {
			get { return TextItem; }
		}

		public override string Name {
			get { return Catalog.GetString ("Selected text"); }
		}

		public override string Description {
			get { return Catalog.GetString ("Currently selected text."); }
		}

		public override string Icon {
			get { return "gtk-select-all"; }
		}

	}
}
