// UniverseFactory.cs
//
// GNOME Do is the legal property of its developers. Please refer to the
// COPYRIGHT file distributed with this source distribution.
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

using Do.Universe;

namespace Do.Platform
{
	
	public static class UniverseFactory
	{

		public interface Implementation
		{
			IFileItem NewFileItem (string path);
			IApplicationItem NewApplicationItem (string path);
		}

		public static Implementation Imp { get; private set; }

		public static void Initialize (Implementation imp)
		{
			if (Imp != null)
				throw new Exception ("Already has Implementation");
			if (imp == null)
				throw new ArgumentNullException ("Implementation may not be null");
			
			Imp = imp;
		}

		public static IFileItem NewFileItem (string path)
		{
			return Imp.NewFileItem (path);
		}
		
		public static IApplicationItem NewApplicationItem (string path)
		{
			return Imp.NewApplicationItem (path);
		}

		public static ITextItem NewTextItem (string text)
		{
			return new TextItem (text);
		}
	}
}
