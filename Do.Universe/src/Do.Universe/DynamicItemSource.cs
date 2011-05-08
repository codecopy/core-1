// 
//  DynamicItemSource.cs
//  
//  Author:
//       Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  Copyright © 2011 Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Linq;
using System.Collections.Generic;

namespace Do.Universe
{
	public class ItemsAvailableEventArgs : EventArgs
	{
		public IEnumerable<Item> newItems;
	}

	public class ItemsUnavailableEventArgs : EventArgs
	{
		public IEnumerable<Item> unavailableItems;
	}

	/// <summary>
	/// An item source with items which may appear or disappear at any time.
	/// Unlike the standard ItemSource, this does not get periodically polled.
	/// </summary>
	public abstract class DynamicItemSource : Item, IChildItemSource
	{
		private EventHandler<ItemsAvailableEventArgs> itemsAvailable;
		private Dictionary<string, Item> itemBuffer;
		private object event_lock;

		public DynamicItemSource ()
		{
			itemBuffer = new Dictionary<string, Item> ();
			event_lock = new object ();
			ItemsAvailable += (object sender, ItemsAvailableEventArgs e) =>
			{
				lock (itemBuffer) {
					foreach (Item item in e.newItems)
						itemBuffer.Add (item.UniqueId, item);
				}
			};
			ItemsUnavailable += (object sender, ItemsUnavailableEventArgs e) =>
			{
				lock (itemBuffer) {
					foreach (Item item in e.unavailableItems)
						itemBuffer.Remove (item.UniqueId);
				}
			};
		}

		/// <summary>
		/// The <typeparamref>DynamicItemSource</typeparamref> raises this event when
		/// new Items are available and should be added to the Universe.
		/// </summary>
		public event EventHandler<ItemsAvailableEventArgs> ItemsAvailable {
			add {
				// If there have been any items previously created fire the callback off immediately
				// to get the subscriber up to speed.
				// Copy out itemBuffer so that we can modify it at will.  We don't know how long the callback
				// will be holding on to it.
				if (itemBuffer.Any ()) {
					var args = new ItemsAvailableEventArgs ();
					lock (itemBuffer) {
						args.newItems = itemBuffer.Values.ToArray ();
					}
					value (this, args);
				}
				lock (event_lock)
					itemsAvailable += value;
			}
			remove {
				lock (event_lock)
					itemsAvailable -= value;
			}
		}

		private EventHandler<ItemsUnavailableEventArgs> itemsUnavailable;
		/// <summary>
		/// The <typeparamref>DynamicItemSource</typeparamref> raises this event when
		/// one or more items are no longer available and should be removed from the
		/// Universe.
		/// </summary>
		public event EventHandler<ItemsUnavailableEventArgs> ItemsUnavailable {
			add {
				lock (event_lock)
					itemsUnavailable += value;
			}
			remove {
				lock (event_lock)
					itemsUnavailable -= value;
			}
		}

		/// <value>
		/// Item sub-types provided/supported by this source. These include any
		/// types of items provided through ItemsAvailable, and the types of items
		/// that this source will provide children for.  Please provide types as
		/// close as possible in ancestry to the static types of items this source
		/// provides/supports (e.g.  FirefoxBookmarkItem instead of Item or
		/// BookmarkItem).
		/// </value>
		public abstract IEnumerable<Type> SupportedItemTypes { get; }

		/// <summary>
		/// Provides a collection of children of an item. Item is guaranteed to be a
		/// subtype of a type in SupportedItemTypes.
		/// An empty enumerable is ok---it signifies that no children are provided for
		/// the Item argument.
		/// </summary>
		public virtual IEnumerable<Item> ChildrenOfItem (Item item)
		{
			yield break;
		}

	}
}

