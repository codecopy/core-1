// IDoInteropService.cs
// 
// Copyright (C) 2009 GNOME Do
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

namespace Docky.Core
{
	
	
	public interface IDoInteropService : IDockService
	{
		event EventHandler Summoned;
		
		event EventHandler Vanished;
		
		event EventHandler Reset;
		
		event EventHandler ResultsGrow;
		
		event EventHandler ResultsShrink;

		void RequestClickOff ();
	}
}
