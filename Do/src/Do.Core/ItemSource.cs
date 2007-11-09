/* ItemSource.cs
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
using System.Collections.Generic;

using Do.Universe;

namespace Do.Core
{
	
	public class ItemSource : GCObject 
	{
		
		private bool enabled;
		protected IItemSource source;
		
		public ItemSource (IItemSource source):
			base (source)
		{
			this.source = source;
			enabled = true;
		}
		
		public void UpdateItems () {
			source.UpdateItems ();
		}
		
		public ICollection<IItem> Items {
			get {
				List<IItem> items;
				
				items = new List<IItem> ();
				if (source.Items != null) {
					items.Capacity = source.Items.Count;
					foreach (IItem item in source.Items) {
						items.Add (new Item (item));
					}
				}
				return items;
			}
		}
		
		public ICollection<IItem> ChildrenOfItem (IItem item) {
			return new List<IItem> ();
		}
		
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; }
		}
		
	}
}