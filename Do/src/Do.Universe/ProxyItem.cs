//  ProxyItem.cs
//
//  GNOME Do is the legal property of its developers, whose names are too numerous
//  to list here.  Please refer to the COPYRIGHT file distributed with this
//  source distribution.
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;

using Do.Core;

namespace Do.Universe
{
	public class ProxyItem: DoItem
	{
		string name, description, icon;
		
		public ProxyItem ():
			this (new EmptyItem ())
		{
		}
			
		public ProxyItem (IItem item):
			this (null, null, null, item)
		{
		}
		
		public ProxyItem (string name):
			this (name, new EmptyItem ())
		{
		}
		
		public ProxyItem (string name, IItem item):
			this (name, null, null, item)
		{
		}
		
		public ProxyItem (string name, string description):
			this (name, description, new EmptyItem ())
		{
		}
		
		public ProxyItem (string name, string description, IItem item):
			this (name, description, null, item)
		{
		}
		
		public ProxyItem (string name, string description, string icon):
			this (name, description, icon, new EmptyItem ())
		{
		}
		
		public ProxyItem (string name, string description, string icon, IItem item):
			base (item)
		{
			this.name = name;
			this.description = description;
			this.icon = icon;
		}

		public override string Name {
			get { return name ?? base.Name; }
		}
		
		public override string Description {
			get { return description ?? base.Description; }
		}
		
		public override string Icon {
			get { return icon ?? base.Icon; }
		}

		public override string UID {
			get { return string.Format ("{0}{1}{2}", GetType (), Name, Description); }
		}
	}
}