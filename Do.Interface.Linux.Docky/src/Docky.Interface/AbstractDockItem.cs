// AbstractDockItem.cs
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
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

using Do.Interface;
using Do.Platform;

using Docky.Utilities;

namespace Docky.Interface
{

	
	public abstract class AbstractDockItem : IDockItem
	{
		Surface text_surface, icon_surface;
		#region IDockItem implementation 
		
		public virtual bool IsAcceptingDrops { 
			get { return false; } 
		}
		
		public virtual bool ReceiveItem (string item) 
		{
			return false;
		}
		
		public virtual Surface GetIconSurface (Surface similar)
		{
			return (icon_surface != null) ? icon_surface : icon_surface = MakeIconSurface (similar);
		}
		
		protected virtual Surface MakeIconSurface (Surface similar)
		{
			Surface tmp_surface = similar.CreateSimilar (similar.Content, DockPreferences.FullIconSize, DockPreferences.FullIconSize);
			Context cr = new Context (tmp_surface);
			
			Gdk.Pixbuf pbuf = GetSurfacePixbuf ();
			if (pbuf.Width != DockPreferences.FullIconSize || pbuf.Height != DockPreferences.FullIconSize) {
				double scale = (double)DockPreferences.FullIconSize / Math.Max (pbuf.Width, pbuf.Height);
				Gdk.Pixbuf temp = pbuf.ScaleSimple ((int) (pbuf.Width * scale), (int) (pbuf.Height * scale), Gdk.InterpType.Bilinear);
				pbuf.Dispose ();
				pbuf = temp;
			}
			
			Gdk.CairoHelper.SetSourcePixbuf (cr, 
			                                 pbuf, 
			                                 (DockPreferences.FullIconSize - pbuf.Width) / 2,
			                                 (DockPreferences.FullIconSize - pbuf.Height) / 2);
			cr.Paint ();
			
			pbuf.Dispose ();
			(cr as IDisposable).Dispose ();
			
			return tmp_surface;
		}
		
		protected void RedrawIcon ()
		{
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
		}
		
		protected abstract Pixbuf GetSurfacePixbuf ();
		
		/// <summary>
		/// Gets a surface that is useful for display by the Dock based on the Description
		/// </summary>
		/// <param name="similar">
		/// A <see cref="Surface"/>
		/// </param>
		/// <returns>
		/// A <see cref="Surface"/>
		/// </returns>
		public virtual Surface GetTextSurface (Surface similar)
		{
			if (text_surface == null) {
				text_surface = Util.GetBorderedTextSurface (GLib.Markup.EscapeText (Description), DockPreferences.TextWidth, similar);
			}
			return text_surface;
		}
		
		public virtual Pixbuf GetDragPixbuf ()
		{
			return null;
		}
		
		/// <summary>
		/// Called whenever the icon receives a click event
		/// </summary>
		/// <param name="button">
		/// A <see cref="System.UInt32"/>
		/// </param>
		/// <param name="controller">
		/// A <see cref="IDoController"/>
		/// </param>
		public virtual void Clicked (uint button)
		{
		}
		
		/// <summary>
		/// Called whenever an icon get repositioned, so it can update its child applications icon regions
		/// </summary>
		public virtual void SetIconRegion (Gdk.Rectangle region)
		{
		}
		
		//// <value>
		/// The value used to for the text surface
		/// </value>
		public abstract string Description { get; }
		
		/// <value>
		/// The Widget of the icon.
		/// </value>
		public virtual int Width {
			get { return DockPreferences.IconSize; }
		}
		
		/// <value>
		/// The Height of the icon.
		/// </value>
		public virtual int Height {
			get { return DockPreferences.IconSize; }
		}
		
		/// <value>
		/// If the icon is scalable or not (provides FullIconSize sized surface)
		/// </value>
		public virtual bool Scalable {
			get { return true; }
		}
		
		/// <value>
		/// Whether or not to draw an application present indicator
		/// </value>
		public virtual int WindowCount {
			get { return 0; }
		}
		
		/// <value>
		/// The last time this icon was "clicked" that required an animation
		/// </value>
		public virtual DateTime LastClick {
			get;
			protected set;
		}
		
		/// <value>
		/// When this item was added to the Dock
		/// </value>
		public virtual DateTime DockAddItem { get; set; }
		
		#endregion 
		

		
		public AbstractDockItem ()
		{
			LastClick = DateTime.UtcNow - new TimeSpan (0, 10, 0);
			
			DockPreferences.IconSizeChanged += OnIconSizeChanged;
		}
		
		protected virtual void OnIconSizeChanged ()
		{
			if (text_surface != null) {
				text_surface.Destroy ();
				text_surface = null;
			}
			
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
		}

		#region IDisposable implementation 
		
		public virtual void Dispose ()
		{
			DockPreferences.IconSizeChanged -= OnIconSizeChanged;
			
			if (text_surface != null) {
				text_surface.Destroy ();
				text_surface = null;
			}
			
			if (icon_surface != null) {
				icon_surface.Destroy ();
				icon_surface = null;
			}
		}
		
		#endregion 
		
		public virtual bool Equals (IDockItem other)
		{
			return other == this;
		}
	}
}
