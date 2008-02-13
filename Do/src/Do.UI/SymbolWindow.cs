/* SymbolWindow.cs
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
using System.Collections.Generic;
using Mono.Unix;

using Gtk;
using Gdk;

using Do.Addins;
using Do.Universe;

// Do.Core dependency needs to be removed
using Do.Core;

namespace Do.UI
{
	public class SymbolWindow : Gtk.Window
	{
		class DefaultIconBoxObject : IObject
		{
			public string Icon { get { return "search"; } }
			public string Name { get { return ""; } }
			public string Description { get { return ""; } }
		}

		class NoResultsFoundObject : IObject
		{
			string query;

			public NoResultsFoundObject (string query)
			{
				this.query = query;
			}

			public string Icon { get { return "gtk-dialog-question"; } }
			public string Name { get { return Catalog.GetString ("No results found."); } }

			public string Description
			{
				get {
					return string.Format (Catalog.GetString ("No results found for \"{0}\"."), query);
				}
			}
		}

		const int IconBoxIconSize = 128;
		const int IconBoxPadding = 6;
		const int IconBoxRadius = 20;
		const double WindowTransparency = 0.91;

		protected enum Pane {
			First = 0,
			Second = 1,
			Third = 2,
		}

		const int SearchDelay = 225;
		uint[] searchTimeout;

		RoundedFrame frame;
		SymbolDisplayLabel label;
		ResultsWindow resultsWindow;
		HBox resultsHBox;
		IconBox[] iconbox;

		protected Pane currentPane;
		protected SearchContext[] context;

		IAction action;
		List<IItem> items;
		List<IItem> modItems;

		bool tabbing;

		public SymbolWindow () : base (Gtk.WindowType.Toplevel)
		{
			Build ();
			searchTimeout = new uint[3];
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			context = new SearchContext[3];
			SetDefaultState ();
		}

		IObject GetCurrentObject (Pane pane)
		{
			IObject o;

			try {
				o = context[(int) pane].Results[context[(int) pane].Cursor];
			} catch {
				o = null;
			}
			return o;
		}

		Pane CurrentPane
		{
			get {
				return currentPane;
			}
			set {
				if (currentPane == value) return;

				currentPane = value;
				iconbox[0].IsFocused = (value == Pane.First);
				iconbox[1].IsFocused = (value == Pane.Second);
				iconbox[2].IsFocused = (value == Pane.Third);

				resultsWindow.Context = CurrentContext;

				label.DisplayObject = GetCurrentObject (value) ??
					new NoResultsFoundObject (CurrentContext.Query);

				Reposition ();
			}
		}

		SearchContext CurrentContext
		{
			get {
				return context[(int) currentPane];
			}
			set {
				context[(int) currentPane] = value;
			}
		}

		int CurrentCursor
		{
			get {
				return context[(int) currentPane].Cursor;
			}
			set {
				context[(int) currentPane].Cursor = value;
			}
		}

		bool ThirdPaneAllowed
		{
			get {
				IObject first, second;
				IAction action;

				first = GetCurrentObject (Pane.First);
				second = GetCurrentObject (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					context[1].Results.Length > 0;
			}
		}

		bool ThirdPaneRequired
		{
			get {
				IObject first, second;
				IAction action;

				first = GetCurrentObject (Pane.First);
				second = GetCurrentObject (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					!action.ModifierItemsOptional &&
					context[1].Results.Length > 0;
			}
		}

		protected virtual void SetDefaultState ()
		{
			// Cancel any pending searches.
			for (int i = 0; i < 3; ++i) {
				if (searchTimeout[i] > 0) 
					GLib.Source.Remove (searchTimeout[i]);
				searchTimeout[i] = 0;
			}

			resultsWindow.Hide ();
			resultsWindow.Clear ();

			tabbing = false;

			context[0] = new SearchContext ();
			context[1] = new SearchContext ();
			context[2] = new SearchContext ();
			context[0].SearchTypes = new Type[] { typeof (IAction), typeof (IItem) };

			CurrentPane = Pane.First;
			iconbox[0].DisplayObject = new DefaultIconBoxObject ();
			iconbox[1].Clear ();
			iconbox[2].Clear ();
			HideThirdPane ();

			label.SetDisplayLabel (Catalog.GetString ("Type to begin searching"),
					Catalog.GetString ("Type to start searching."));
		}

		protected virtual void SetNoResultsFoundState (Pane pane)
		{
			NoResultsFoundObject none_found;

			if (pane == Pane.First) {
				iconbox[1].Clear ();
				iconbox[2].Clear ();
			} else if (pane == Pane.Second) {
				iconbox[2].Clear ();
			}

			none_found = new NoResultsFoundObject (context[(int) pane].Query);
			iconbox[(int) pane].DisplayObject = none_found;
			if (currentPane == pane) {
				label.DisplayObject = none_found;
				resultsWindow.Results = new IObject[0];
			}
		}

		protected void ClearSearchResults ()
		{
			switch (currentPane) {
				case Pane.First:
					// Do this once we have "" in the first results list(?)
					// context[0] = new SearchContext ();
					// SearchFirstPane ();
					SetDefaultState ();
					break;
				case Pane.Second:
					context[1] = new SearchContext ();
					SearchSecondPane ();
					break;
				case Pane.Third:
					context[2] = new SearchContext ();
					SearchThirdPane ();
					break;
			}
		}

		protected override bool OnButtonPressEvent (EventButton evnt)
		{
			int start_x, start_y, end_x, end_y;
			int click_x, click_y;
			bool click_on_window, click_near_settings_icon;

			GetPosition (out start_x, out start_y);
			GetSize (out end_x, out end_y);
			end_x += start_x;
			end_y += start_y;
			click_x = (int) evnt.XRoot;
			click_y = (int) evnt.YRoot;
			click_on_window = start_x <= click_x && click_x < end_x &&
			                  start_y <= click_y && click_y < end_y;
			click_near_settings_icon = (end_x - 27) <= click_x && click_x < end_x &&
			                            start_y <= click_y && click_y < (start_y + 27);
			if (click_near_settings_icon) {
				Addins.Util.Appearance.PopupMainMenuAtPosition (end_x - 21, start_y + 16);
				// Have to re-grab the pane from the menu.
				Addins.Util.Appearance.PresentWindow (this);
			} else if (!click_on_window) {
				Vanish ();
			}
			return base.OnButtonPressEvent (evnt);
		}

		public void Vanish ()
		{
			resultsWindow.Hide ();
			Hide ();
		}

		public void Reposition ()
		{
			int monitor;
			Gdk.Rectangle geo, main, results;
			
			GetPosition (out main.X, out main.Y);
			GetSize (out main.Width, out main.Height);
			monitor = Screen.GetMonitorAtPoint (main.X, main.Y);
			geo = Screen.GetMonitorGeometry (monitor);
			main.X = (geo.Width - main.Width) / 2;
			main.Y = (int)((geo.Height - main.Height) / 2.5);
			Move (main.X, main.Y);

			resultsWindow.GetSize (out results.Width, out results.Height);
			results.Y = main.Y + main.Height - 1;
			results.X = main.X + (IconBoxIconSize + 60) * (int) currentPane + IconBoxRadius;
			resultsWindow.Move (results.X, results.Y);
		}

		void OnControlKeyPressEvent (EventKey evnt)
		{
			switch ((Gdk.Key) evnt.KeyValue) {
				case Gdk.Key.q:
					Application.Quit ();
					break;
				default:
					break;
			}
		}

		void OnEscapeKeyPressEvent (EventKey evnt)
		{
			bool results, something_typed;

			something_typed = CurrentContext.Query.Length > 0;
			results = CurrentContext.Results.Length > 0;

			resultsWindow.Hide ();
			ClearSearchResults ();
			if (CurrentPane == Pane.First && !results) Vanish ();
			else if (!something_typed) SetDefaultState ();
		}

		void OnActivateKeyPressEvent (EventKey evnt)
		{
			bool shift_pressed = (evnt.State & ModifierType.ShiftMask) != 0;
			PerformAction (!shift_pressed);
		}

		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			string query;

			query = CurrentContext.Query;
			if (query.Length == 0) return;
			CurrentContext.Query = query.Substring (0, query.Length-1);
			QueueSearch (false);
		}

		void OnTabKeyPressEvent (EventKey evnt)
		{
			tabbing = true;
			resultsWindow.Hide ();
			if (CurrentPane == Pane.First &&
					context[0].Results.Length != 0) {
				CurrentPane = Pane.Second;
			} else if (CurrentPane == Pane.Second && ThirdPaneAllowed) {
				CurrentPane = Pane.Third;
				ShowThirdPane ();
			} else if (CurrentPane == Pane.Third && !ThirdPaneRequired) {
				// This is used for actions for which modifier items are optional.
				CurrentPane = Pane.First;
				HideThirdPane ();
			} else {
				CurrentPane = Pane.First;
			}
			tabbing = false;
		}

		void OnUpDownKeyPressEvent (EventKey evnt)
		{
			if (!resultsWindow.Visible) {
				resultsWindow.Show ();
				return;
			}
			if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Up) {
				resultsWindow.SelectPrev ();
			} else {
				resultsWindow.SelectNext ();
			}
		}

		void OnRightLeftKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Results.Length > 0) {
				if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Right) {
					CurrentContext.ChildrenSearch = true;
					QueueSearch (false);
				} else if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Left) {
					CurrentContext.ParentSearch = true;
					QueueSearch (false);
				}
				resultsWindow.Show ();
			}
		}

		void OnInputKeyPressEvent (EventKey evnt)
		{
			char c;

			c = (char) Gdk.Keyval.ToUnicode (evnt.KeyValue);
			if (char.IsLetterOrDigit (c)
					|| char.IsPunctuation (c)
					|| c == ' '
					|| char.IsSymbol (c)) {
				CurrentContext.Query += c;
				QueueSearch (false);
			}
		}

		protected override bool OnKeyPressEvent (EventKey evnt)
		{
			// Handle action keys (Quit, etc.)
			if ((evnt.State & ModifierType.ControlMask) != 0) {
					OnControlKeyPressEvent (evnt);
					return base.OnKeyPressEvent (evnt);
			}

			switch ((Gdk.Key) evnt.KeyValue) {
				// Throwaway keys
				case Gdk.Key.Shift_L:
				case Gdk.Key.Control_L:
					break;
				case Gdk.Key.Escape:
					OnEscapeKeyPressEvent (evnt);
					break;
				case Gdk.Key.Return:
				case Gdk.Key.ISO_Enter:
					OnActivateKeyPressEvent (evnt);
					break;
				case Gdk.Key.Delete:
				case Gdk.Key.BackSpace:
					OnDeleteKeyPressEvent (evnt);
					break;
				case Gdk.Key.Tab:
					OnTabKeyPressEvent (evnt);
					break;
				case Gdk.Key.Up:
				case Gdk.Key.Down:
					OnUpDownKeyPressEvent (evnt);
					break;
				case Gdk.Key.Right:
				case Gdk.Key.Left:
					OnRightLeftKeyPressEvent (evnt);
					break;
				default:
					OnInputKeyPressEvent (evnt);
					break;
			}
			return base.OnKeyPressEvent (evnt);
		}

		protected virtual void PerformAction (bool vanish)
		{
			IObject first, second, third;

			items.Clear ();
			modItems.Clear ();
			if (vanish) {
				Vanish ();
			}

			first = GetCurrentObject (Pane.First);
			second = GetCurrentObject (Pane.Second);
			third = GetCurrentObject (Pane.Third);
			// User may have pressed enter before delayed search completed.
			// We guess this is the case if there is nothing in the second pane,
			// so we immediately do a search and use the first result.
			if (first != null && second == null) {
				SearchSecondPane ();
				second = GetCurrentObject (Pane.Second);
			}

			if (first != null && second != null) {
				if (first is IItem) {
					items.Add (first as IItem);
					action = second as IAction;
				} else {
					items.Add (second as IItem);
					action = first as IAction;
				}
				if (third != null && iconbox[2].Visible) {
					modItems.Add (third as IItem);
				}
				action.Perform (items.ToArray (), modItems.ToArray ());
			}

			if (vanish) {
				SetDefaultState ();
			}
		}

		private void OnResultsWindowSelectionChanged (object sender,
				ResultsWindowSelectionEventArgs args)
		{
			CurrentCursor = args.SelectedIndex;

			UpdatePane (currentPane, false);

			// If we're just tabbing, no need to search.
			if (tabbing) return;

			switch (currentPane) {
				case Pane.First:
					context[1] = new SearchContext ();
					SearchPaneDelayed (Pane.Second);
					break;
				case Pane.Second:
					context[2] = new SearchContext ();
					SearchPaneDelayed (Pane.Third);
					break;
			}
		}

		private void OnScreenChanged (object sender, EventArgs args)
		{
			SetColormap ();
		}

		void QueueSearch (bool delayed)
		{
			if (delayed) {
				SearchPaneDelayed (currentPane);
				return;
			}

			switch (currentPane) {
				case Pane.First:
					SearchFirstPane ();
					break;
				case Pane.Second:
					SearchSecondPane ();
					break;
				case Pane.Third:
					SearchThirdPane ();
					break;
			}
		}
		
		public void DisplayObjects (IObject[] objects)
		{
			SetDefaultState ();
			context[0].Results = objects;
			// This is extremely awkward. DR....
			context[0].LastContext.LastContext = context[0].LastContext = context[0];
			SearchFirstPane ();
			SearchSecondPane ();

			// Showing the results after a bit of a delay looks a bit better.
			GLib.Timeout.Add (250, delegate {
				Gdk.Threads.Enter ();
				resultsWindow.Show ();
				Gdk.Threads.Leave ();
				return false;
			});
		}

		void SearchPaneDelayed (Pane pane)
		{
			for (int i = 0; i < 3; ++i) {
				if (searchTimeout[i] > 0) 
					GLib.Source.Remove (searchTimeout[i]);
				searchTimeout[i] = 0;
			}
			for (int i = (int) pane; i < 3; ++i) {
					iconbox[i].Clear ();
			}

			
			searchTimeout[(int) pane] = GLib.Timeout.Add (SearchDelay, delegate {
				Gdk.Threads.Enter ();
				switch (pane) {
					case Pane.First:
						SearchFirstPane ();
						break;
					case Pane.Second:
						SearchSecondPane ();
						break;
					case Pane.Third:
						SearchThirdPane ();
						break;
				}
				Gdk.Threads.Leave ();
				return false;
			});
		}

		protected void SearchFirstPane ()
		{
			IObject lastResult;

			lastResult = GetCurrentObject (Pane.First);

			// If we delete the entire query on a regular search (we are not
			// searching children) then set default state.
			if (context[0].Query == "" &&
					// DR, I could kill you right now.
					context[0].LastContext.LastContext.LastContext == null &&
					context[0].ParentContext == null) {
				SetDefaultState ();
				return;
			}

			context[0].SearchTypes = new Type[] { typeof (IItem), typeof (IAction) };
			Do.UniverseManager.Search (ref context[0]);
			UpdatePane (Pane.First, true);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetCurrentObject (Pane.First) != lastResult) {
				context[1] = new SearchContext ();
				SearchPaneDelayed (Pane.Second);
			}
		}

		protected void SearchSecondPane ()
		{
			IObject first;
			IObject lastResult;

			lastResult = GetCurrentObject (Pane.Second);

			// Set up the next pane based on what's in the first pane:
			first = GetCurrentObject (Pane.First);
			if (first is IItem) {
				// Selection is an IItem
				context[1].Items.Clear ();
				context[1].Items.Add (first as IItem);
				context[1].SearchTypes = new Type[] { typeof (IAction) };
			} else {
				// Selection is an IAction
				context[1].Action = first as IAction;
				context[1].SearchTypes = new Type[] { typeof (IItem) };
			}

			Do.UniverseManager.Search (ref context[1]);
			UpdatePane (Pane.Second, true);

			// Queue a search for the next pane unless the result of the most
			// recent search is the same as the last result - if this is the
			// case, we already have a valid search queued.
			if (GetCurrentObject (Pane.Second) != lastResult) {
				context[2] = new SearchContext ();
				SearchPaneDelayed (Pane.Third);
			}
		}

		protected void SearchThirdPane ()
		{
			IObject first, second;

			context[2].SearchTypes = new Type[] { typeof (IItem) };

			first = GetCurrentObject (Pane.First);
			second = GetCurrentObject (Pane.Second);
			if (first == null || second == null) {
				SetNoResultsFoundState (Pane.Third);
				return;
			}

			if (first is IItem) {
				context[2].Items.Clear ();
				context[2].Items.Add (first as IItem);
				context[2].Action = second as IAction;
			} else {
				context[2].Items.Clear ();
				context[2].Items.Add (second as IItem);
				context[2].Action = first as IAction;
			}

			Do.UniverseManager.Search (ref context[2]);
			UpdatePane (Pane.Third, true);

			if (ThirdPaneRequired) {
				ShowThirdPane ();
			} else if (!ThirdPaneAllowed) {
				HideThirdPane ();
			}
		}

		protected void UpdatePane (Pane pane, bool updateResults)
		{
			IObject current;

			current = GetCurrentObject (pane);
			if (current != null) {
				iconbox[(int) pane].DisplayObject = current;
				iconbox[(int) pane].Highlight = context[(int) pane].Query;
			} else {
				SetNoResultsFoundState (pane);
				return;
			}

			if (pane == currentPane) {
				label.DisplayObject = GetCurrentObject (pane);
				if (updateResults) resultsWindow.Context = CurrentContext;
			}
		}

		protected void Build ()
		{
			VBox      vbox;
			Alignment align;
			Gtk.Image settings_icon;

			AppPaintable = true;
			KeepAbove = true;
			Decorated = false;
			// This typehint gets the window to raise all the way to top.
			TypeHint = WindowTypeHint.Splashscreen;

			try {
				SetIconFromFile ("/usr/share/icons/gnome/scalable/actions/system-run.svg");
			} catch { }
			SetColormap ();

			resultsWindow = new ResultsWindow ();
			resultsWindow.SelectionChanged += OnResultsWindowSelectionChanged;

			currentPane = Pane.First;

			frame = new RoundedFrameGloss ();
			frame.DrawFill = true;
			frame.FillColor = BackgroundColor;
			frame.FillAlpha = WindowTransparency;
			frame.Radius = Screen.IsComposited ? IconBoxRadius : 0;
			Add (frame);
			frame.Show ();

			vbox = new VBox (false, 0);
			frame.Add (vbox);
			vbox.BorderWidth = IconBoxPadding;
			vbox.Show ();

			settings_icon = new Gtk.Image (GetType().Assembly, "settings-triangle.png");

			align = new Alignment (1.0F, 0.0F, 0, 0);
			align.SetPadding (3, 0, 0, IconBoxPadding);
			align.Add (settings_icon);
			vbox.PackStart (align, false, false, 0);
			settings_icon.Show ();
			align.Show ();

			resultsHBox = new HBox (false, IconBoxPadding*2);
			resultsHBox.BorderWidth = IconBoxPadding;
			vbox.PackStart (resultsHBox, false, false, 0);
			resultsHBox.Show ();

			iconbox = new IconBox[3];

			iconbox[0] = new IconBox (IconBoxIconSize);
			iconbox[0].IsFocused = true;
			iconbox[0].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[0], false, false, 0);
			iconbox[0].Show ();

			iconbox[1] = new IconBox (IconBoxIconSize);
			iconbox[1].IsFocused = false;
			iconbox[1].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[1], false, false, 0);
			iconbox[1].Show ();

			iconbox[2] = new IconBox (IconBoxIconSize);
			iconbox[2].IsFocused = false;
			iconbox[2].Radius = IconBoxRadius;
			resultsHBox.PackStart (iconbox[2], false, false, 0);
			// iconbox[2].Show ();

			align = new Alignment (0.5F, 0.5F, 1, 1);
			align.SetPadding (0, 2, 0, 0);
			label = new SymbolDisplayLabel ();
			align.Add (label);
			vbox.PackStart (align, false, false, 0);
			label.Show ();
			align.Show ();

			ScreenChanged += OnScreenChanged;
			ConfigureEvent += OnConfigureEvent;

			Reposition ();
		}
		
		private void RGB_to_HSV (ref byte r, ref byte g, ref byte b)
		{
			//CLEANUP
			//Ported from Murrine Engine
			double red, green, blue;
			double hue = 0, lum, sat;
			double max, min;
			double delta;
			
			red = (double) r;
			green = (double) g;
			blue = (double) b;
			
			max = Math.Max (red, Math.Max(blue, green));
			min = Math.Min (red, Math.Min(blue, green));
			lum = (max/255.0)*100.0;
			
			delta = max - min;
			
			if ( Math.Abs(max - min) < 0.0001 )
			{
				lum = 0;
				sat = 0;
			} else {
				sat = ( delta / max ) * 100;
				
				if ( red == max )   hue = ( green - blue) / delta;
				if ( green == max ) hue = 2 + ( blue - red ) / delta;
				if ( blue == max )  hue = 4 + ( red - green ) / delta;
				
				hue *= 60;
				if ( hue <= 0 )
					hue += 360;
				
			}
			r = (byte) hue;
			g = (byte) sat;
			b = (byte) lum;
		}
		
		private void HSV_to_RGB (ref byte hue, ref byte sat, ref byte val)
		{
			double h;
			double s;
			double v;

			double r = 0;
			double g = 0;
			double b = 0;

			h = (double) hue;
			s = ((double) sat) / 100;
			v = ((double) val) / 100;

			if ( s == 0 ) 
			{
				r = v;
				g = v;
				b = v;
			} else {
				double p;
				double q;
				double t;

				double fracSec;
				int secNum;
				
				secNum = (int)(Math.Floor(h / 60));
				fracSec = (h / 60) - secNum;

				p = v * (1 - s);
				q = v * (1 - (s * fracSec));
				t = v * (1 - (s * (1 - fracSec)));

				switch (secNum) 
				{
					case 0:
						r = v;
						g = t;
						b = p;
						break;

					case 1:
						r = q;
						g = v;
						b = p;
						break;

					case 2:
						r = p;
						g = v;
						b = t;
						break;

					case 3:
						r = p;
						g = q;
						b = v;
						break;

					case 4:
						r = t;
						g = p;
						b = v;
						break;

					case 5:
						r = v;
						g = p;
						b = q;
						break;
				}
			}

			hue = Convert.ToByte(r*255);
			sat = Convert.ToByte(g*255);
			val = Convert.ToByte(b*255);
		}

		private Gdk.Color BackgroundColor
		{
			get {
				byte r, g, b;
				byte maxLum;
				Gdk.Color bgColor;

				maxLum = 50;
				bgColor = Gtk.Rc.GetStyle (this).Backgrounds[(int) StateType.Selected];
				
				r = (byte) ((bgColor.Red) >> 8);
				g = (byte) ((bgColor.Green) >> 8);
				b = (byte) ((bgColor.Blue) >> 8);
				
				//with stupid
				//RGB_to_HSV(ref r, ref g, ref b);
				//if ( b > maxLum ) b = maxLum;
				//HSV_to_RGB(ref r, ref g, ref b);
				
				return new Gdk.Color(r, g, b);
			}
		}

		protected virtual void SetColormap ()
		{
			Gdk.Colormap  colormap;

			colormap = Screen.RgbaColormap;
			if (colormap == null) {
				colormap = Screen.RgbColormap;
				Console.Error.WriteLine ("No alpha support.");
			}
			Colormap = colormap;
		}

		void OnConfigureEvent (object sender, ConfigureEventArgs args)
		{
			Reposition ();
		}

		void ShowThirdPane ()
		{
			iconbox[2].Show ();
			Resize (1, 1);
			GLib.Timeout.Add (10, delegate {
				Gdk.Threads.Enter ();
				Reposition ();
				Gdk.Threads.Leave ();
				return false;
			});
		}

		void HideThirdPane ()
		{
			iconbox[2].Hide ();
			Resize (1, 1);
			GLib.Timeout.Add (10, delegate {
				Gdk.Threads.Enter ();
				Reposition ();
				Gdk.Threads.Leave ();
				return false;
			});
		}

		protected override bool OnExposeEvent (EventExpose evnt)
		{
			Cairo.Context cairo;
			//frame.FillColor = BackgroundColor;
			
			using (cairo = Gdk.CairoHelper.Create (GdkWindow)) {
				cairo.Rectangle (evnt.Area.X, evnt.Area.Y, evnt.Area.Width, evnt.Area.Height);
				cairo.Color = new Cairo.Color (1.0, 1.0, 1.0, 0.0);
				cairo.Operator = Cairo.Operator.Source;
				cairo.Paint ();
			}
			return base.OnExposeEvent (evnt);
		}
	}
}
