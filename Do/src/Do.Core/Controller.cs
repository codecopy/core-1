/* Controller.cs
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
using System.Reflection;
using System.Collections.Generic;

using Gdk;
using Mono.Unix;

using Do;
using Do.UI;
using Do.Addins;
using Do.Universe;
using Do.DBusLib;

namespace Do.Core {

	public class Controller : IController, IDoController {

		protected IDoWindow window;
		protected Gtk.Window addin_window;
		protected Gtk.AboutDialog about_window;
		protected PreferencesWindow prefs_window;
		protected ISearchController[] controllers;
		
		const int SearchDelay = 250;
		
		uint[] searchTimeout;
		IAction action;
		List<IItem> items;
		List<IItem> modItems;
		bool thirdPaneVisible;
		bool resultsGrown;
		bool shiftPressed = false;
		
		public Controller ()
		{
			about_window = null;
			items = new List<IItem> ();
			modItems = new List<IItem> ();
			searchTimeout = new uint[3];
			resultsGrown = false;
			
			controllers = new SimpleSearchController[3];
			
			// Each controller needs to be aware of the controllers before it.
			// Going down the line however is not needed at current.
			controllers[0] = new FirstSearchController  ();
			controllers[1] = new SecondSearchController (controllers[0]);
			controllers[2] = new ThirdSearchController  (controllers[0], controllers[1]);
			
			// Set up our callbacks here.  If we ever reconstruct these controllers, 
			// and we shouldn't be, we will need to reset these too.  However controllers
			// provide a resetting mechanism.
			controllers[0].SelectionChanged += OnFirstSelectionChanged;
			controllers[1].SelectionChanged += OnSecondSelectionQueryChanged;
			controllers[2].SelectionChanged += OnThirdSelectionQueryChanged;
			
			controllers[0].QueryChanged += OnFirstQueryChanged;
			controllers[1].QueryChanged += OnSecondSelectionQueryChanged;
			controllers[2].QueryChanged += OnThirdSelectionQueryChanged;
			
			controllers[0].SearchStarted += OnFirstSearchStarted;
			controllers[1].SearchStarted += OnSecondSearchStarted;
			controllers[2].SearchStarted += OnThirdSearchStarted;
		}
		
		public void Initialize ()
		{
			ThemeChanged ();
			Do.Preferences.PreferenceChanged += OnPreferenceChanged;
		}
		
		private void OnPreferenceChanged (object sender,
										  PreferenceChangedEventArgs args)
		{
			if (args.Key == "Theme")
				ThemeChanged ();
		}
		
		void ThemeChanged ()
		{
			if (null != window) Vanish ();
			switch (Do.Preferences.Theme) {
				case "Mini":
					window = new MiniWindow (this);
					break;
				case "Glass Frame":
					window = new GlassWindow (this);
					break;
				default:
					window = new ClassicWindow (this);
					break;
			}
			
			// Get key press events from window since we want to control that
			// here.
			window.KeyPressEvent += KeyPressWrap;
			Reset ();
		}

		bool IsSummonable {
			get {
				return prefs_window == null && about_window == null;
			}
		}

		/// <value>
		/// Convenience Method
		/// </value>
		ISearchController CurrentContext {
			get {
				return controllers[(int) CurrentPane];
			}
		}
		
		/// <value>
		/// The currently active pane, setting does not imply searching
		/// currently
		/// </value>
		public Pane CurrentPane {
			set {
				if (window.CurrentPane == Pane.First &&
					CurrentContext.Results.Length == 0)
					return;
				
				switch (value) {
				case Pane.First:
					window.CurrentPane = Pane.First;
					break;
				case Pane.Second:
					window.CurrentPane = Pane.Second;
					break;
				case Pane.Third:
					if (ThirdPaneAllowed)
						window.CurrentPane = Pane.Third;
					break;
				}

				// Determine if third pane needed
				if (!ThirdPaneAllowed || 
				    (!ThirdPaneRequired &&
					 controllers[2].Query.Length == 0 &&
					 controllers[2].Cursor == 0))
					ThirdPaneVisible = false;
				
				if ((ThirdPaneAllowed && window.CurrentPane == Pane.Third) ||
						ThirdPaneRequired)
					ThirdPaneVisible = true;
			}
			
			get {
				return window.CurrentPane;
			}
		}
		
		bool ThirdPaneVisible {
			set {
				if (value == thirdPaneVisible)
					return;
				
				if (value == true)
					window.Grow ();
				else
					window.Shrink ();
				thirdPaneVisible = value;
			}
			
			get {
				return thirdPaneVisible;
			}
		}
		
		bool ThirdPaneAllowed {
			get {
				IObject first, second;
				IAction action;

				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					controllers[1].Results.Length > 0;
			}
		}

		bool ThirdPaneRequired {
			get {
				IObject first, second;
				IAction action;

				first = GetSelection (Pane.First);
				second = GetSelection (Pane.Second);
				action = (first as IAction) ?? (second as IAction);
				return action != null &&
					action.SupportedModifierItemTypes.Length > 0 &&
					!action.ModifierItemsOptional &&
					controllers[1].Results.Length > 0;
			}
		}

		public bool IsSummoned {
			get {
				return null != window && window.Visible;
			}
		}

		internal PreferencesWindow PreferencesWindow {
			get { return prefs_window; }
		}
		
		/// <summary>
		/// Summons a window with objects in it... seems to work
		/// </summary>
		/// <param name="objects">
		/// A <see cref="IObject"/>
		/// </param>
		public void SummonWithObjects (IObject[] objects)
		{
			if (!IsSummonable) return;
			
			Reset ();
			
			//Someone is going to need to explain this to me -- Now with less stupid!
			controllers[0].Results = objects;
			
			Summon ();

			// If there are multiple results, show results window after a short
			// delay.
			if (objects.Length > 1) {
				GLib.Timeout.Add (50,
					delegate {
						Gdk.Threads.Enter ();
						GrowResults ();
						Gdk.Threads.Leave ();
						return false;
					}
				);
			}
		}
		
		/////////////////////////
		/// Key Handling ////////
		/////////////////////////

		private void KeyPressWrap (Gdk.EventKey evnt)
		{
			//why are we doing this anyway?
			if ((evnt.State & ModifierType.ControlMask) != 0) {
					return;
			}

			if (evnt.Key != Key.Shift_L) shiftPressed = false;
			
			switch ((Gdk.Key) evnt.KeyValue) {
			// Throwaway keys
			
			case Gdk.Key.Control_L:
				break;
			case Gdk.Key.Escape:
				OnEscapeKeyPressEvent (evnt);
				break;
			case Gdk.Key.Return:
			case Gdk.Key.ISO_Enter:
			case Gdk.Key.KP_Enter:
				OnActivateKeyPressEvent (evnt);
				break;
			case Gdk.Key.Delete:
			case Gdk.Key.BackSpace:
				OnDeleteKeyPressEvent (evnt);
				break;
			case Gdk.Key.Tab:
			case Gdk.Key.ISO_Left_Tab:
				OnTabKeyPressEvent (evnt);
				break;
			case Gdk.Key.Up:
			case Gdk.Key.Down:
			case Gdk.Key.Home:
			case Gdk.Key.End:
				OnUpDownKeyPressEvent (evnt);
				break;
			case Gdk.Key.Shift_L:
				OnTextModePressEvent (evnt);
				break;
			case Gdk.Key.Right:
			case Gdk.Key.Left:
				OnRightLeftKeyPressEvent (evnt);
				break;
			case Gdk.Key.comma:
				OnCommaKeyPressEvent (evnt);
				break;
			default:
				OnInputKeyPressEvent (evnt);
				break;
			}
			return;
		}
		
		void OnActivateKeyPressEvent (EventKey evnt)
		{
			bool shift_pressed = (evnt.State & ModifierType.ShiftMask) != 0;
			PerformAction (!shift_pressed);
		}
		
		/// <summary>
		/// This will set a secondary cursor unless we are operating on a text item, in which case we
		/// pass the event to the input key handler
		/// </summary>
		/// <param name="evnt">
		/// A <see cref="EventKey"/>
		/// </param>
		void OnCommaKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Selection is ITextItem || !resultsGrown)
				OnInputKeyPressEvent (evnt);
			else if (CurrentContext.AddSecondaryCursor (CurrentContext.Cursor))
				UpdatePane (CurrentPane);
		}
		
		void OnDeleteKeyPressEvent (EventKey evnt)
		{
			CurrentContext.DeleteChar ();
		}
		
		void OnEscapeKeyPressEvent (EventKey evnt)
		{
			bool results, something_typed;

			something_typed = CurrentContext.Query.Length > 0;
			results = CurrentContext.Results.Length > 0;
			
			ClearSearchResults ();
			
			if (!ThirdPaneAllowed)
				ThirdPaneVisible = false;
			
			ShrinkResults ();
			
			if (CurrentPane == Pane.First && !results) Vanish ();
			else if (!something_typed) Reset ();
		}
		
		void OnInputKeyPressEvent (EventKey evnt)
		{
			char c;
			//Do.PrintPerf ("InputKeyPress Start");
			c = (char) Gdk.Keyval.ToUnicode (evnt.KeyValue);
			if (char.IsLetterOrDigit (c)
					|| char.IsPunctuation (c)
					|| (c == ' ' && CurrentContext.Query.Length > 0)
					|| char.IsSymbol (c)) {
				CurrentContext.AddChar (c);
				//Console.WriteLine (CurrentContext);
				//QueueSearch (false);
			}
			//Do.PrintPerf ("InputKeyPress Stop");
		}
		
		void OnRightLeftKeyPressEvent (EventKey evnt)
		{
			if (CurrentContext.Results.Length > 0) {
				if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Right) {
					if (CurrentContext.ItemChildSearch ())
						GrowResults ();
				} else if ((Gdk.Key) evnt.KeyValue == Gdk.Key.Left) {
					if (CurrentContext.ItemParentSearch ())
						GrowResults ();
				}
			}
			
		}
		
		void OnTabKeyPressEvent (EventKey evnt)
		{
			ShrinkResults ();

			if (evnt.Key == Key.Tab) {
				NextPane ();
			} else if (evnt.Key == Key.ISO_Left_Tab) {
				PrevPane ();
			}
			if (!(CurrentPane == Pane.First && CurrentContext.Results.Length == 0))
				window.SetPaneContext (CurrentPane, CurrentContext.UIContext);
		}
		
		void OnTextModePressEvent (EventKey evnt)
		{
			if (shiftPressed) {
				bool tmp = CurrentContext.TextMode;
				CurrentContext.TextMode = !CurrentContext.TextMode;
				if (CurrentContext.TextMode == tmp) {
					NotificationIcon.SendNotification ("Text Mode Error", "Do could not enter text mode " +
					                      "because the current action does not support it.");
				}
				shiftPressed = false;
			} else {
				shiftPressed = true;
			}
		}
		
		void OnUpDownKeyPressEvent (EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Up) {
				if (!resultsGrown) {
                    GrowResults ();
					return;
				} else {
					if (CurrentContext.Cursor <= 0)
						return;
					CurrentContext.Cursor--;
                }
			} else if (evnt.Key == Gdk.Key.Down) {
				if (!resultsGrown) {
					GrowResults ();
					return;
				}
				CurrentContext.Cursor++;
			} else if (evnt.Key == Gdk.Key.Home) {
				CurrentContext.Cursor = 0;
			} else if (evnt.Key == Gdk.Key.End) {
				CurrentContext.Cursor = CurrentContext.Results.Length - 1;
			}
		}
		
		/// <summary>
		/// Selects the logical next pane in the UI left to right
		/// </summary>
		void NextPane ()
		{
			switch (CurrentPane) {
			case Pane.First:
				CurrentPane = Pane.Second;
				break;
			case Pane.Second:
				if (ThirdPaneAllowed)
					CurrentPane = Pane.Third;
				else
					CurrentPane = Pane.First;
				break;
			case Pane.Third:
				CurrentPane = Pane.First;
				break;
			}
		}
		
		/// <summary>
		/// Selects the logical previous pane in the UI left to right
		/// </summary>
		void PrevPane ()
		{
			switch (CurrentPane) {
			case Pane.First:
				if (ThirdPaneAllowed)
					CurrentPane = Pane.Third;
				else
					CurrentPane = Pane.Second;
				break;
			case Pane.Second:
				CurrentPane = Pane.First;
				break;
			case Pane.Third:
				CurrentPane = Pane.Second;
				break;
			}
		}
		
		void OnFirstSearchStarted ()
		{
		}
		
		void OnSecondSearchStarted ()
		{
			window.ClearPane (Pane.Second);
		}
		
		void OnThirdSearchStarted ()
		{
			window.ClearPane (Pane.Third);
		}
		
		void OnFirstSelectionChanged ()
		{
			//Do.PrintPerf ("FirstSelectionChanged");
			UpdatePane (Pane.First);
			//Do.PrintPerf ("UpdatePaneFinished");
		}
		
		void OnFirstQueryChanged ()
		{
			//Do.PrintPerf ("FirstQueryChanged");
			if (string.IsNullOrEmpty(controllers[0].Query) && controllers[0].Results.Length == 0) {
			    Reset ();
				return;
			}
			UpdatePane (Pane.First);
		}
		
		void OnSecondSelectionQueryChanged ()
		{
			if (string.IsNullOrEmpty(controllers[0].Query) && controllers[0].Results.Length == 0)
				return;
			
			UpdatePane (Pane.Second);
		}
		
		void OnThirdSelectionQueryChanged ()
		{
			if (string.IsNullOrEmpty(controllers[0].Query) && controllers[0].Results.Length == 0)
				return;
			UpdatePane (Pane.Third);
		}
		
		protected void ClearSearchResults ()
		{
			switch (CurrentPane) {
			case Pane.First:
				Reset ();
				break;
			case Pane.Second:
				controllers[1].Reset ();
				controllers[2].Reset ();
				break;
			case Pane.Third:
				controllers[2].Reset ();
				break;
			}
		}
		
		/////////////////////////
		// Pane Update Methods //
		/////////////////////////

		protected void UpdatePane (Pane pane)
		{
			//Lets see if we need to play with Third pane visibility
			if (pane == Pane.Second) {
				if (ThirdPaneRequired) {
					ThirdPaneVisible = true;
				} else if (!ThirdPaneAllowed || 
				           (controllers[2].Query.Length == 0 && CurrentPane != Pane.Third)) {
					ThirdPaneVisible = false;
				}
			}
			window.SetPaneContext (pane, controllers[(int) pane].UIContext);
		}
		
		/// <summary>
		/// Call to fully reset Do.  This should return Do to its starting state
		/// </summary>
		void Reset ()
		{
			for (int i = 0; i < 3; ++i) {
				if (searchTimeout[i] > 0) 
					GLib.Source.Remove (searchTimeout[i]);
				searchTimeout[i] = 0;
			}
			ThirdPaneVisible = false;
			
			foreach (ISearchController controller in controllers)
				controller.Reset ();
			
			CurrentPane = Pane.First;
			
			ShrinkResults ();
			window.Reset ();
		}
		
		/// <summary>
		/// Should cause UI to display more results
		/// </summary>
		void GrowResults ()
		{
			window.GrowResults ();
			resultsGrown = true;	
		}
		
		/// <summary>
		/// Should cause UI to display fewer results, 0 == no results displayed
		/// </summary>
		void ShrinkResults ()
		{
			window.ShrinkResults ();
			resultsGrown = false;
		}
		
		IObject GetSelection (Pane pane)
		{
			IObject o;

			try {
				o = controllers[(int) pane].Selection;
			} catch {
				o = null;
			}
			return o;
		}
		
		protected virtual void PerformAction (bool vanish)
		{
			IObject first, second, third;
			string actionQuery, itemQuery, modItemQuery;

			items.Clear ();
			modItems.Clear ();
			if (vanish) {
				Vanish ();
			}

			first  = GetSelection (Pane.First);
			second = GetSelection (Pane.Second);
			third  = GetSelection (Pane.Third);

			if (first != null && second != null) {
				if (first is IItem) {
					foreach (IItem item in controllers[0].FullSelection)
						items.Add (item);
					action = second as IAction;
					itemQuery = controllers[0].Query;
					actionQuery = controllers[1].Query;
				} else {
					foreach (IItem item in controllers[1].FullSelection)
						items.Add (item);
					action = first as IAction;
					itemQuery = controllers[1].Query;
					actionQuery = controllers[0].Query;
				}
				if (third != null && ThirdPaneVisible) {
					foreach (IItem item in controllers[2].FullSelection)
						modItems.Add (item);
					modItemQuery = controllers[2].Query;
					(third as DoObject).IncreaseRelevance (modItemQuery, null);
				}

				/////////////////////////////////////////////////////////////
				/// Relevance accounting
				/////////////////////////////////////////////////////////////
				
				// Increase the relevance of the item.
				foreach (DoObject item in items) {
					item.IncreaseRelevance (itemQuery, null);
				}

				// Increase the relevance of the action alone:
				(action as DoAction).IncreaseRelevance (actionQuery, null);
				// Increase the relevance of the action /for each item/:
				foreach (DoObject item in items)
					(action as DoObject).IncreaseRelevance (actionQuery, item);

				action.Perform (items.ToArray (), modItems.ToArray ());
			}

			if (vanish) {
				Reset ();
			}
		}

		///////////////////////////
		/// IController Members ///
		///////////////////////////
		
		public void Summon ()
		{
			if (!IsSummonable) return;
			window.Summon ();
		}
		
		public void Vanish ()
		{
			ShrinkResults ();
			window.Vanish ();
		}	

		public void ShowPreferences ()
		{
			Vanish ();
			Reset ();

			if (null == prefs_window) {
				prefs_window = new PreferencesWindow ();
				prefs_window.Destroyed += delegate {
					Do.UniverseManager.Reload ();
					prefs_window = null;
				};
			}
			prefs_window.Show ();
		}

		public void ShowAbout ()
		{
			string[] logos;
			string logo;

			Vanish ();
			Reset ();

			about_window = new Gtk.AboutDialog ();
			about_window.ProgramName = "GNOME Do";

			try {
				AssemblyName name = Assembly.GetEntryAssembly ().GetName ();
				about_window.Version = String.Format ("{0}.{1}.{2}",
					name.Version.Major, name.Version.Minor, name.Version.Build);
			} catch {
				about_window.Version = Catalog.GetString ("Unknown");
			}
			
			logos = new string[] {
				"/usr/share/icons/gnome/scalable/actions/search.svg",
			};

			logo = "gnome-run";
			foreach (string l in logos) {
				if (!System.IO.File.Exists (l)) continue;
				logo = l;
			}

			about_window.Logo = UI.IconProvider.PixbufFromIconName (logo, 140);
			about_window.Copyright = "Copyright \xa9 2008 GNOME Do Developers";
			about_window.Comments = "Do things as quickly as possible\n" +
				"(but no quicker) with your files, bookmarks,\n" +
				"applications, music, contacts, and more!";
			about_window.Website = "http://do.davebsd.com/";
			about_window.WebsiteLabel = "Visit Homepage";
			about_window.IconName = "gnome-run";

			if (null != about_window.Screen.RgbaColormap)
				Gtk.Widget.DefaultColormap = about_window.Screen.RgbaColormap;

			about_window.Run ();
			about_window.Destroy ();
			about_window = null;
		}
		
		/////////////////////////////
		/// IDoController Members ///
		/////////////////////////////
		
		public void NewContextSelection (Pane pane, int index)
		{
			if (controllers[(int) pane].Results.Length == 0) return;
			
			controllers[(int) pane].Cursor = index;
			window.SetPaneContext (pane, controllers[(int) pane].UIContext);
		}

		public void ButtonPressOffWindow ()
		{
			Vanish ();
			Reset ();
		}
	}
}
