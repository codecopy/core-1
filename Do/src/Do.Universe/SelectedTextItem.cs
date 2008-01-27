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
using System.Threading;
using System.Collections.Generic;

namespace Do.Universe
{

	public class SelectedTextItem : ITextItem
	{		
		public SelectedTextItem ()
		{
		}
		
		public string Name
		{
			get { return "Selected text"; }
		}
		
		public string Description
		{
			get { return "Currently selected text."; }
		}
		
		public string Icon
		{
			get { return "gtk-select-all"; }
		}
		
		public string Text
		{
			get {
				Gtk.Clipboard primary;
				string text;
			
				primary = Gtk.Clipboard.Get (Gdk.Selection.Primary);
				if (primary.WaitIsTextAvailable ()) {
					text = primary.WaitForText ();
				} else {
					text = "";
				}
				return text;
			}
		}
	}
}