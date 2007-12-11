/* SymbolDisplayLabel.cs
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

using Gtk;
using Gdk;

using Do.Universe;
using Do.Addins;

namespace Do.UI
{
	public class SymbolDisplayLabel : Label
	{
		// const string displayFormat = " <big>{0}</big> \n {1} ";
		
		// Description only:
		const string displayFormat = "<span size=\"medium\"> {1} </span>";
		
		string highlight;
		string name, description;
		
		public SymbolDisplayLabel ():
			base ("")
		{
			highlight = name = description = "";
			Build ();
		}
		
		void Build ()
		{
			UseMarkup = true;
			Ellipsize = Pango.EllipsizeMode.End;
			Justify = Justification.Center;
			ModifyFg (StateType.Normal, Style.White);
		}
		
		public IObject DisplayObject
		{
			set {
				IObject displayObject;
				
				displayObject = value;
				name = description = highlight = "";
				if (displayObject != null) {
					name = displayObject.Name;
					description = displayObject.Description;
				}
				SetDisplayLabel (name, description);
			}
		}
		
		public void SetDisplayLabel (string name, string description)
		{
			this.name = name ?? "";
			this.description = description ?? "";
			highlight = "";
			UpdateText ();
		}
		
		public string Highlight
		{
			get { return highlight; }
			set {
				highlight = value ?? "";
				UpdateText ();
			}
		}
		
		void UpdateText ()
		{
			string highlighted, safe_name, safe_description;

			safe_name = Util.Appearance.MarkupSafeString (name);
			safe_description = Util.Appearance.MarkupSafeString (description);
			highlighted = Util.FormatCommonSubstrings(safe_name, highlight, "<u>{0}</u>");
			Markup = string.Format (displayFormat, highlighted, safe_description);
		}
		
	}
}
