// IDockItem.cs
// 
// Copyright (C) 2008 GNOME Do
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

using Cairo;

using Do.Interface;
using Do.Universe;

namespace Docky.Interface
{
	
	
	public interface IDockItem : IEquatable<IDockItem>, IDisposable
	{
		string Description { get; }
		int Width { get; }
		int Height { get; }
		bool Scalable { get; }
		bool DrawIndicator { get; }
		
		DateTime LastClick { get; }
		DateTime DockAddItem { get; set; }
		
		Surface GetIconSurface (Surface similar);
		Surface GetTextSurface ();
		
		void Clicked (uint button, IDoController controller);
		void SetIconRegion (Gdk.Rectangle region);
	}
	
	public interface IDoDockItem
	{
		Element Element { get; }
	}
}
