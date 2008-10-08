// BezelGlassResults.cs
// 
// Copyright (C) 2008 GNOME-Do
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
using System.Threading;

using Cairo;
using Gdk;
using Gtk;

using Do.Addins;
using Do.Universe;

namespace Do.UI
{
	
	
	public class BezelGlassResults : Gtk.DrawingArea
	{
		HUDStyle style;
		
		const int SurfaceHeight = 20;
		const int IconSize = 16;
		const int FadeTime = 100;
		
		int num_results;
		int width, height;
		int border_width, top_border_width;
		Dictionary <IObject, Surface> surface_buffer;
		Surface highlight_surface, backbuffer, child_inout_surface, triplebuffer, background;
		
		DateTime delta_time;
		double scroll_offset, highlight_offset, child_scroll_offset, slide_offset;
		int cursor, prev_cursor, delta;
		uint timer, delta_reset;
		bool visible;
		
		IUIContext context = null;
		
		IObject[] results;

		public int X { get; set; }
		
		int BottomBorderWidth {
			get {
				switch (style) {
				case HUDStyle.HUD:
					return 25;
				case HUDStyle.Classic:
					return 20;
				default:
					throw new NotImplementedException ();
				}
			}
		}

		private Cairo.Color BackgroundColor
		{
			get {
				switch (style) {
				case HUDStyle.HUD:
					return new Cairo.Color (0, 0, 0, .8);
				case HUDStyle.Classic:
					return new Cairo.Color (1, 1, 1);
				default:
					throw new NotImplementedException ();
				}
			}
		}				
		
		string ItemTextColor {
			get {
				switch (style) {
				case HUDStyle.HUD:
					return "ffffff";
				case HUDStyle.Classic:
					return "333333";
				default:
					throw new NotImplementedException ();
				}
			}
		}
		
		string QueryColor {
			get {
				switch (style) {
				case HUDStyle.HUD:
					return "dddddd";
				case HUDStyle.Classic:
					return "777777";
				default:
					throw new NotImplementedException ();
				}
			}
		}
		
		public IObject[] Results {
			get {
				return results;
			}
			set {
				if (results != null && value != null && results.GetHashCode () == value.GetHashCode ())
					return;
				results = value;
				scroll_offset = 0;
				highlight_offset = 0;
			}
		}
		
		public int Cursor {
			get {
				return cursor;
			}
			set {
				if (cursor == value)
					return;
				
				int oldStart = StartResult;
				prev_cursor = cursor;
				cursor = value;
				if (oldStart == StartResult) {
					highlight_offset -= value-prev_cursor;
				} else {
					scroll_offset += value-prev_cursor;
				}
				delta++;
			}
		}
		
		private int StartResult {
			get {
				int result = Math.Max (Cursor - (num_results / 2), 0);
				if (results == null)
					return 0;
				while (result+num_results > results.Length && result > 1)
					result--;
				return result;
			}
		}
		
		private bool AnimationNeeded {
			get {
				return CursorMoveNeeded || ScrollNeeded || ChildScrollNeeded || SlideNeeded;
			}
		}
		
		private bool CursorMoveNeeded {
			get {
				return highlight_offset != 0;
			}
		}
		
		private bool ScrollNeeded {
			get {
				return scroll_offset != 0;
			}
		}
		
		private bool ChildScrollNeeded {
			get {
				return child_scroll_offset != 0;
			}
		}
		
		private bool SlideNeeded {
			get {
				return (visible && slide_offset != 1 || !visible && slide_offset != 0);
			}
		}
		
		public IUIContext Context
		{
			set {
				IUIContext tmp = context;
				context = value;
				
				if (!Visible)
					return;

				if (value == null || value.Results.Length == 0) {
					Clear ();
					return;
				}
				
				if (tmp != null) {
					if (context.ParentContext != null && tmp != null &&
					    context.ParentContext.Query == tmp.Query && 
					    tmp.Results.GetHashCode () == context.ParentContext.Results.GetHashCode ()) {
						InitChildInAnimation ();
					} else if (tmp.ParentContext != null && context != null &&
					           tmp.ParentContext.Query == context.Query && 
					           tmp.ParentContext.Results.GetHashCode () == context.Results.GetHashCode ()) {
						InitChildOutAnimation ();
					}
				}
				
				Cursor = value.Cursor;
				Results = value.Results;
//				secondary = value.SecondaryCursors;
				if (visible)
					Draw ();
			}
		}
		
		private int InternalWidth {
			get {
				return width - 2*border_width;
			}
		}
		
		public BezelGlassResults(int numberResults, int width, HUDStyle style) : base ()
		{
			this.style = style;
			
			num_results = numberResults;
			surface_buffer = new Dictionary <IObject,Surface> ();
			
			X=105;
			border_width = 12;
			top_border_width = 20;
			this.width = width;
			height = num_results * SurfaceHeight + top_border_width + BottomBorderWidth;
			SetSizeRequest (width, height);
			
			DoubleBuffered = false;
			
			this.Shown += delegate {
				Context = context;
				Draw ();
			};
		}
		
		private void AnimatedDraw ()
		{
			if (!IsDrawable || timer > 0) return;
			
			Paint ();
			
			if (!AnimationNeeded)
				return;
			
			delta_time = DateTime.Now;
			timer = GLib.Timeout.Add (17, delegate {
				double change = DateTime.Now.Subtract (delta_time).TotalMilliseconds / FadeTime;
				delta_time = DateTime.Now;
				
				double move = Math.Max (change*delta, change);
				
				if (ScrollNeeded) {
					if (scroll_offset > 0)
						scroll_offset = Math.Max (0, scroll_offset - move);
					else
						scroll_offset = Math.Min (0, scroll_offset + move);
				}
				
				if (CursorMoveNeeded) {
					if (highlight_offset > 0)
						highlight_offset = Math.Max (0, highlight_offset - move);
					else
						highlight_offset = Math.Min (0, highlight_offset + move);
				}
				
				if (ChildScrollNeeded) {
					if (child_scroll_offset > 0)
						child_scroll_offset = Math.Max (0, child_scroll_offset - change*0.7);
					else
						child_scroll_offset = Math.Min (0, child_scroll_offset + change*0.7);
				}
				
				if (SlideNeeded) {
					if (visible)
						slide_offset = Math.Min (1, slide_offset + change*0.7);
					else
						slide_offset = Math.Max (0, slide_offset - change*0.7);
				}
				
				Paint ();
				
				if (!AnimationNeeded) {
					timer = 0;
					if (delta_reset > 0)
						GLib.Source.Remove (delta_reset);
					delta_reset = GLib.Timeout.Add (50, delegate {
						if (timer == 0)
							delta = 0;
						delta_reset = 0;
						return false;
					});
					return false;
				}
				return true;
			});
		}
		
		public void Clear ()
		{
			Results = null;
			Cursor = 0;
			foreach (Surface s in surface_buffer.Values)
				(s as IDisposable).Dispose ();
			
			surface_buffer = new Dictionary<IObject,Surface> ();
		}
		
		private void Paint ()
		{
			if (!IsDrawable) return;
			Context cr = Gdk.CairoHelper.Create (GdkWindow);
			
			if (backbuffer == null)
				backbuffer = cr.Target.CreateSimilar (cr.Target.Content, width, height);
			
			
			DrawContextOnSurface (backbuffer);
			if (child_scroll_offset == 0) {
				cr.SetSource (backbuffer, X, -(height*(1-slide_offset)));
				cr.Operator = Operator.Source;
				cr.Paint ();
			} else {
				if (triplebuffer == null) {
					triplebuffer = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				}
				
				int old_x, new_x;
				if (child_scroll_offset > 0) {
					old_x = (int)(-width*(1-child_scroll_offset));
					new_x = old_x+width;
				} else {
					old_x = (int)(-width*(-1-child_scroll_offset));
					new_x = old_x-width;
				}
				
				DrawSlideContexts (child_inout_surface, backbuffer, triplebuffer, old_x, new_x);

				cr.SetSource (triplebuffer, X, -(height*(1-slide_offset)));
				cr.Operator = Operator.Source;
				cr.Paint ();
			}
			
			(cr as IDisposable).Dispose ();
		}
		
		/// <summary>
		/// Draws two surfaces offset onto a singel surface.  Useful for making left/right slide animations
		/// </summary>
		/// <param name="old_surface">
		/// A <see cref="Surface"/>
		/// </param>
		/// <param name="new_surface">
		/// A <see cref="Surface"/>
		/// </param>
		/// <param name="target_surface">
		/// A <see cref="Surface"/>
		/// </param>
		/// <param name="old_x">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="new_x">
		/// A <see cref="System.Int32"/>
		/// </param>
		private void DrawSlideContexts (Surface old_surface, Surface new_surface, Surface target_surface,
		                                int old_x, int new_x)
		{
			Context cr = new Context (target_surface);
			cr.Operator = Operator.Source;

			// redraw our top and bottom border separately.  This makes the slide only appear to affect
			// the center.
			cr.Rectangle (0, 0, width, top_border_width);
			cr.Rectangle (0, height-BottomBorderWidth, width, BottomBorderWidth);
			cr.SetSource (new_surface, 0, 0);
			cr.Fill ();
			
			cr.Rectangle (0, top_border_width, width, height-top_border_width-BottomBorderWidth);
			cr.SetSource (old_surface, old_x, 0);
			cr.FillPreserve ();
			
			cr.Operator = Operator.Over;
			cr.SetSource (new_surface, new_x, 0);
			cr.FillPreserve ();
			
			(cr as IDisposable).Dispose ();
		}
		
		/// <summary>
		/// Draws a header.  Currently this relies on being drown on top of the background surface to
		/// look correct
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="radius">
		/// A <see cref="System.Int32"/>
		/// </param>
		private void DrawHeaderOnContext (Context cr, int radius)
		{
			switch (style) {
			case HUDStyle.HUD:
				cr.MoveTo (0 + radius, 0);
				cr.Arc (0 + width - radius, 0 + radius, radius, Math.PI*1.5, Math.PI*2);
				cr.LineTo (0 + width, 0 + top_border_width);
				cr.LineTo (0, 0 + top_border_width);
				cr.Arc (0 + radius, 0 + radius, radius, Math.PI, Math.PI*1.5);
				LinearGradient title_grad = new LinearGradient (0, 0, 0, top_border_width);
				title_grad.AddColorStop (0.0, new Cairo.Color (0.45, 0.45, 0.45));
				title_grad.AddColorStop (0.5, new Cairo.Color (0.33, 0.33, 0.33));
				title_grad.AddColorStop (0.5, new Cairo.Color (0.28, 0.28, 0.28));
				cr.Pattern = title_grad;
				cr.Fill ();
				break;
			case HUDStyle.Classic:
				cr.Rectangle (0.5, -0.5, width-1, top_border_width);
				LinearGradient title_grad1 = new LinearGradient (0, 0, 0, top_border_width);
				title_grad1.AddColorStop (0, new Cairo.Color (0.95, 0.95, 0.95));
				title_grad1.AddColorStop (1, new Cairo.Color (0.75, 0.75, 0.75));
				cr.Pattern = title_grad1;
				cr.FillPreserve ();
				
				cr.LineWidth = 1;
				cr.Color = new Cairo.Color (0.4, 0.4, 0.4);
				cr.Stroke ();
				break;
			}
		}
		
		/// <summary>
		/// Draws a footer.  Currently this relies on being drawn on top of the background surface to
		/// look correct
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		/// <param name="radius">
		/// A <see cref="System.Int32"/>
		/// </param>
		private void DrawFooterOnContext (Context cr, int radius)
		{
			switch (style) {
			case HUDStyle.HUD:
				cr.MoveTo (.5, height-BottomBorderWidth+.5);
				cr.LineTo (width-1, height-BottomBorderWidth+.5);
				cr.Arc (width-radius-.5, height-radius-.5, radius, 0, Math.PI*.5);
				cr.Arc (radius+.5, height-radius-.5, radius, Math.PI*.5, Math.PI);
				cr.ClosePath ();
				cr.Color = new Cairo.Color (.22, .22, .22, 1);
				cr.FillPreserve ();
				cr.LineWidth=1;
				cr.Color = new Cairo.Color (.6, .6, .6, .4);
				cr.Stroke ();
				break;
			case HUDStyle.Classic:
				cr.Rectangle (.5, height-BottomBorderWidth+.5, width-1, BottomBorderWidth-1);
				LinearGradient title_grad1 = new LinearGradient (0, height-BottomBorderWidth, 0, height);
				title_grad1.AddColorStop (0, new Cairo.Color (0.95, 0.95, 0.95));
				title_grad1.AddColorStop (1, new Cairo.Color (0.75, 0.75, 0.75));
				cr.Pattern = title_grad1;
				cr.FillPreserve ();
				
				cr.LineWidth = 1;
				cr.Color = new Cairo.Color (0.4, 0.4, 0.4);
				cr.Stroke ();
				break;
			}
		}
		
		/// <summary>
		/// Draws the background theme on the passed context
		/// </summary>
		/// <param name="cr">
		/// A <see cref="Context"/>
		/// </param>
		private void DrawBackgroundOnContext (Context cr)
		{
			cr.Operator = Operator.Source;
			cr.Rectangle (0, 0, width, height);
			cr.Color = new Cairo.Color (0, 0, 0, 0);
			cr.Fill ();
			cr.Operator = Operator.Over;
				
			int c_size = border_width - 2;
			
			//Draw rounded rectange around whole border
			cr.MoveTo (0.5+c_size, -1);
			cr.Arc (width-c_size-0.5, c_size-1, c_size, Math.PI*1.5, Math.PI*2);
			cr.Arc (width-0.5-c_size, height-c_size-0.5, c_size, 0, Math.PI*.5);
			cr.Arc (0.5+c_size, height-c_size-0.5, c_size, Math.PI*.5, Math.PI);
			cr.Arc (0.5+c_size, c_size-1, c_size, Math.PI, Math.PI*1.5);
			cr.ClosePath ();
			cr.Color = BackgroundColor;
			cr.FillPreserve ();
			
			cr.LineWidth = 1;
			cr.Color = new Cairo.Color (.3, .3, .3, 1);
			cr.Stroke ();

			DrawHeaderOnContext (cr, c_size);
			
			cr.Rectangle (border_width, top_border_width, InternalWidth, height-top_border_width);
			cr.Color = new Cairo.Color (.9, .9, .9, .05);
			cr.Fill ();
			
			DrawFooterOnContext (cr, c_size);
			
			cr.MoveTo (border_width + .5, top_border_width);
			cr.LineTo (border_width + .5, height-BottomBorderWidth);
			cr.MoveTo (width - border_width - .5, top_border_width);
			cr.LineTo (width - border_width - .5, height-BottomBorderWidth);
			cr.MoveTo (0, height-BottomBorderWidth-.5);
			cr.LineTo (width, height-BottomBorderWidth-.5);
			
			cr.LineWidth = 1;
			cr.Color = new Cairo.Color (.6, .6, .6, .15);
			cr.Stroke ();
		}
		
		/// <summary>
		/// Draws the entire view of the results window now on the surface passed in
		/// </summary>
		/// <param name="sr">
		/// A <see cref="Surface"/>
		/// </param>
		private void DrawContextOnSurface (Surface sr)
		{
			Context cr = new Context (sr);
			if (background == null) {
				background = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				Context cr2 = new Context (background);
				DrawBackgroundOnContext (cr2);
				(cr2 as IDisposable).Dispose ();
			}
			
			cr.Operator = Operator.Source;
			cr.SetSource (background);
			cr.Paint ();
			cr.Operator = Operator.Over;
			
			if (context != null && !string.IsNullOrEmpty (context.Query))
				RenderText (cr, new Gdk.Rectangle (10, 3, width-60, 20), 12, context.Query, QueryColor);
			
			if (Results != null) {
				string render_string = context.Cursor+1 + " of " + Results.Length + "  ▸  ";
				if (context.ParentContext != null && context.ParentContext.Selection != null) {
					if (context.ParentContext.ParentContext != null && context.ParentContext.ParentContext.Selection != null) {
						render_string += context.ParentContext.ParentContext.Selection.Name + " ▸ ";
					}
					render_string += context.ParentContext.Selection.Name + " ▸ ";
				}
				
				RenderText (cr, new Gdk.Rectangle (10, height-BottomBorderWidth+3, width-20, 20), 11, render_string);
				int start_result = StartResult-(int) Math.Ceiling (scroll_offset);
				RenderHighlight (cr);
				for (int i = start_result; i < start_result+num_results+1 && i < Results.Length; i++) {
					RenderItem (cr, i);
				}
			}
			
			(cr as IDisposable).Dispose ();
		}
		
		public void Draw ()
		{
			AnimatedDraw ();
		}
		
		public void InitChildInAnimation ()
		{
			if (child_inout_surface == null) {
				Context cr = Gdk.CairoHelper.Create (GdkWindow);
				child_inout_surface = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				(cr as IDisposable).Dispose ();
			}
			DrawContextOnSurface (child_inout_surface);
			child_scroll_offset = 1;
		}
		
		public void InitChildOutAnimation ()
		{
			if (child_inout_surface == null) {
				Context cr = Gdk.CairoHelper.Create (GdkWindow);
				child_inout_surface = cr.Target.CreateSimilar (cr.Target.Content, width, height);
				(cr as IDisposable).Dispose ();
			}
			DrawContextOnSurface (child_inout_surface);
			child_scroll_offset = -1;
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Draw ();
			return base.OnExposeEvent (evnt);
		}

		void BufferItem (IObject item) 
		{
			if (!IsDrawable)
				return;
			Context cr = Gdk.CairoHelper.Create (GdkWindow);
			Surface surface = cr.Target.CreateSimilar (cr.Target.Content, InternalWidth, SurfaceHeight);
			Context cr2 = new Context (surface);
			cr2.Rectangle (border_width, 0, InternalWidth, SurfaceHeight);
			cr2.Color = new Cairo.Color (0, 0, 0, 0);
			cr2.Operator = Operator.Source;
			cr2.Fill ();
			cr2.Operator = Operator.Over;
			
			Gdk.Pixbuf pixbuf = IconProvider.PixbufFromIconName (item.Icon, IconSize);
			Gdk.CairoHelper.SetSourcePixbuf (cr2, pixbuf, 2, 2);
			cr2.Paint ();
				
			Pango.Layout layout = new Pango.Layout (this.PangoContext);
			layout.Width = Pango.Units.FromPixels (InternalWidth - IconSize - 10);
			layout.Ellipsize = Pango.EllipsizeMode.End;
			layout.SetMarkup ("<span foreground=\"#" + ItemTextColor + "\">"+item.Name+"</span>");
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (10);
				
			cr2.MoveTo (IconSize + 6, 4);
			Pango.CairoHelper.ShowLayout (cr2, layout);
			
			surface_buffer[item] = surface;
			
			layout.FontDescription.Dispose ();
			(cr2 as IDisposable).Dispose ();
			(cr as IDisposable).Dispose ();
		}
		
		void RenderText (Context cr, Gdk.Rectangle region, int size, string text)
		{
			switch (style) {
			case HUDStyle.HUD:
				RenderText (cr, region, size, text, "ffffff");
				break;
			case HUDStyle.Classic:
				RenderText (cr, region, size, text, "333333");
				break;
			default:
				throw new NotImplementedException ();
			}
		}
		
		void RenderText (Context cr, Gdk.Rectangle region, int size, string text, string color_string)
		{
			Pango.Layout layout = new Pango.Layout (this.PangoContext);
			layout.Width = Pango.Units.FromPixels (region.Width);
			layout.Ellipsize = Pango.EllipsizeMode.End;
			layout.SetMarkup ("<span foreground=\"#" + color_string + "\">"+text+"</span>");
			layout.FontDescription = Pango.FontDescription.FromString ("normal bold");
			layout.FontDescription.AbsoluteSize = Pango.Units.FromPixels (size);
			cr.MoveTo (region.X, region.Y);
			Pango.CairoHelper.ShowLayout (cr, layout);
		}
		
		Surface GetHighlightSource () 
		{
			if (highlight_surface == null) {
				Context cr = Gdk.CairoHelper.Create (GdkWindow);
				highlight_surface = cr.Target.CreateSimilar (cr.Target.Content, width, SurfaceHeight);
				
				Context cr2 = new Context (highlight_surface);
				LinearGradient grad = new LinearGradient (0, 0, 0, SurfaceHeight);
				
				grad.AddColorStop (0, new Cairo.Color (.85, .85, .85, .2));
				grad.AddColorStop (1, new Cairo.Color (.95, .95, .95, .2));
				
				cr2.Pattern = grad;
				double radius=(SurfaceHeight-2)/2;
				double x=4.5, y=1.5;
				int r_width = width-9;
				int r_height = SurfaceHeight-3;
				
				cr2.MoveTo (x, y + radius);
				cr2.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
				cr2.LineTo (x + r_width - radius, y);
				cr2.Arc (x + r_width - radius, y + radius, radius, -Math.PI / 2, 0);
				cr2.LineTo (x + r_width, y + r_height - radius);
				cr2.Arc (x + r_width - radius, y + r_height - radius, radius, 0, Math.PI / 2);
				cr2.LineTo (x + radius, y + r_height);
				cr2.Arc (x + radius, y + r_height - radius, radius, Math.PI / 2, Math.PI);
				cr2.ClosePath ();

				cr2.FillPreserve ();
				
				cr2.LineWidth = 1;
				cr2.Color = new Cairo.Color (0.9, 0.9, 0.9, 1);
				cr2.Stroke ();
				
				(cr as IDisposable).Dispose ();
				(cr2 as IDisposable).Dispose ();
			}
			return highlight_surface;
		}
		
		void RenderItem (Context cr, int item)
		{
			if (item >= Results.Length || item < 0)
				return;
			int offset = (int) (SurfaceHeight*scroll_offset) + top_border_width;
			if (!surface_buffer.ContainsKey (Results[item])) {
				BufferItem (Results[item]);
			}
			
			cr.Rectangle (border_width, top_border_width, InternalWidth, height-top_border_width-BottomBorderWidth);
			cr.Clip ();
			
			cr.Rectangle (border_width, offset+(item-StartResult)*SurfaceHeight, InternalWidth, SurfaceHeight);
			if (item%2 == 1) {
				cr.Color = new Cairo.Color (.2, .2, .2, .2);
				cr.Operator = Operator.DestOver;
				cr.FillPreserve ();
			}
			
			cr.Operator = Operator.Over;
			cr.SetSource (surface_buffer[Results[item]], border_width, offset+(item-StartResult)*SurfaceHeight);
			cr.Fill ();
		}
		
		void RenderHighlight (Context cr)
		{
			int offset = (int) (SurfaceHeight*highlight_offset) + top_border_width;
			cr.Rectangle (0, offset+(Cursor-StartResult)*SurfaceHeight, width, SurfaceHeight);
			cr.SetSource (GetHighlightSource (), 0, offset+(Cursor-StartResult)*SurfaceHeight);
			cr.FillPreserve ();
		}
		
		public void SlideIn ()
		{
			if (visible)
				return;
			visible = true;
			slide_offset = 0;
			Draw ();
		}
		
		public void SlideOut ()
		{
			if (!visible)
				return;
			visible = false;
			slide_offset = 1;
			Draw ();
		}
	}
}
