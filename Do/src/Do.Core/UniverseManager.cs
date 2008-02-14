/* UniverseManager.cs
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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Mono.Addins;
using Mono.Addins.Setup;

using Do;
using Do.Universe;

namespace Do.Core
{
	public class UniverseManager
	{
		/// <summary>
		/// How long between update events (seconds).
		/// </summary>
		const int UpdateInterval = 60;
		
		/// <summary>
		/// Maximum amount of time to spend updating (millseconds).
		/// </summary>
		const int MaxUpdateTime = 125;
		
		Dictionary<string, List<IObject>> firstResults;
		Dictionary<IObject, IObject> universe;

		List<DoItemSource> doItemSources;
		List<DoAction> doActions;

		// Keep track of next data structures to update.
		int itemSourceCursor;
		int firstResultsCursor;

		public UniverseManager()
		{
			universe = new Dictionary<IObject, IObject> ();
			doItemSources = new List<DoItemSource> ();
			doActions = new List<DoAction> ();
			firstResults = new Dictionary<string, List<IObject>> ();
			itemSourceCursor = firstResultsCursor = 0;
			
		}

		internal void Initialize ()
		{
			this.initAddinManager();
			AddinManager.AddExtensionNodeHandler ("/Do/ItemSource",OnItemSourcesChange);
			AddinManager.AddExtensionNodeHandler ("/Do/Action", OnActionsChange);
			
			//LoadBuiltins ();
			//LoadPlugins ();
			BuildUniverse ();
			BuildFirstResults ();

			GLib.Timeout.Add (UpdateInterval * 1000,
				new GLib.TimeoutHandler (OnTimeoutUpdate));
		}

		bool OnTimeoutUpdate ()
		{
			DateTime then;
			int t_update;
			
			if (Do.Controller.IsSummoned) return true;

			// Keep track of the total time (in ms) we have spend updating.
			// We spend half of MaxUpdateTime updating item sources, then
			// another half of MaxUpdateTime updating first results lists.
			t_update = 0;
			while (t_update < MaxUpdateTime / 2) {
				DoItemSource itemSource;
				ICollection<IItem> oldItems;
				Dictionary<IObject, DoItem> newItems;

				then = DateTime.Now;
				itemSourceCursor = (itemSourceCursor + 1) % doItemSources.Count;
				itemSource = doItemSources[itemSourceCursor];
				newItems = new Dictionary<IObject, DoItem> ();
				// Remember old items.
				oldItems = itemSource.Items;	
				// Update the item source.
				itemSource.UpdateItems ();
				// Create a map of the new items.
				foreach (DoItem newItem in itemSource.Items) {
					newItems[newItem] = newItem;
				}
				// Update the universe by either updating items, adding new items,
				// or removing items.
				foreach (DoItem newItem in itemSource.Items) {
					if (universe.ContainsKey (newItem)) {
						// We're updating an item. This updates the item across all
						// first results lists.
						(universe[newItem] as DoItem).Inner = newItem.Inner;
					} else {
						// We're adding a new item. It might take a few minutes to show
						// up in all results lists.
						universe[newItem] = newItem;
					}
				}
				// See if there are any old items that didn't make it into the
				// set of new items. These items need to be removed from the universe.
				foreach (DoItem oldItem in oldItems) {
					if (!newItems.ContainsKey (oldItem) &&
							universe.ContainsKey (oldItem)) {
						universe.Remove (oldItem);
					}
				}
				Log.Info ("Updated \"{0}\" Item Source.", itemSource.Name);
				t_update += (DateTime.Now - then).Milliseconds;
			}

			// Updating a first results list takes about 50ms at most, so we can afford
			// to update a couple of them.
			t_update = 0;
			while (t_update < MaxUpdateTime / 2) {
				DoObjectRelevanceSorter sorter;
				string firstResultKey = null;
				int currentFirstResultsList = 0;

				then = DateTime.Now;
				firstResultsCursor = (firstResultsCursor + 1) % firstResults.Count;
				// Now pick a first results list to update.
				foreach (KeyValuePair<string, List<IObject>> keyval in firstResults) {
					if (currentFirstResultsList == firstResultsCursor) {
						firstResultKey = keyval.Key;
						break;
					}
					currentFirstResultsList++;
				}
				sorter = new DoObjectRelevanceSorter (firstResultKey);
				if (firstResults.ContainsKey (firstResultKey)) {
					firstResults.Remove (firstResultKey);
				}
				firstResults[firstResultKey] = sorter.SortAndNarrowResults (universe.Values);
				Log.Info ("Updated first results for '{0}'.", firstResultKey);
				t_update += (DateTime.Now - then).Milliseconds;
			}
			return true;
		}

		internal ICollection<DoItemSource> ItemSources
		{
			get { return doItemSources.AsReadOnly (); }
		}

		private void BuildFirstResults ()
		{
			DoObjectRelevanceSorter sorter;

			// For each starting character, add every matching object from the universe to
			// the firstResults list corresponding to that character.
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				sorter = new DoObjectRelevanceSorter (keypress.ToString ());
				firstResults[keypress.ToString ()] = sorter.SortAndNarrowResults (universe.Values);
			}
		}

		private void BuildUniverse ()
		{
			// Hash actions.
			foreach (DoAction action in doActions) {
				universe[action] = action;
			}

			// Hash items.
			foreach (DoItemSource source in doItemSources) {
				ICollection<IItem> items;

				items = source.Items;
				if (items.Count == 0) {
					source.UpdateItems ();
					items = source.Items;
				}
				foreach (DoItem item in items) {
					universe[item] = item;
				}
			}
		}

		public void Search (ref SearchContext context)
		{
			List<IObject> results = new List<IObject> ();

			// First check to see if the search context is equivalent to the
			// lastContext or parentContext. Just return that context if it is.
			SearchContext oldContext = context.EquivalentPreviousContextIfExists ();
			if (oldContext != null) {
				context = oldContext.GetContinuedContext ();
				return;
			}
			else if (context.ParentSearch) {
				context = ParentContext (context);
				return;
			}
			else if (context.ChildrenSearch) {
				// TODO: Children are not filtered at all. This needs to be fixed.
				context = ChildContext (context);
				return;
			}

			if (context.Independent) {
				results = IndependentResults (context);
			} else {
				results = DependentResults (context);
			}
			results.AddRange (SpecialItemsForContext (context));

			context.Results = results.ToArray ();
			// Keep a stack of incremental results.
			context = context.GetContinuedContext ();
		}

		private SearchContext ParentContext (SearchContext context)
		{
			//Since we are dealing with the parent, turn off the finding parent
			//flag
			context.ParentSearch = false;
			//Check to see if parent context exists first
			if (context.ParentContext == null)
				return context;

			context = context.ParentContext;
			context.ChildrenSearch = false;
			return context;
		}

		private SearchContext ChildContext (SearchContext context)
		{
			List<IItem> children;
			SearchContext newContext;

			// Check to if the current object has children first
			if (context.Selection is IItem) {
				children = ChildrenOfItem (context.Selection as IItem);
			} else {
				children = new List<IItem> ();
			}
			if (children.Count == 0) {
				context.ChildrenSearch = false;
				return context;
			}

			newContext = context.Clone () as SearchContext;
			newContext.ParentContext = context;
			newContext.Query = string.Empty;
			newContext.Results = children.ToArray ();
			newContext.ChildrenSearch = false;
			newContext.LastContext = new SearchContext (false);

			// We need to do something like this (filter the children), but DR's work
			// is intractable.
			// if (!context.Independent) {
			// 	newContext.Results = DependentResults (newContext).ToArray ();
			// }
			context.ChildrenSearch = true;
			return newContext.GetContinuedContext ();
		}

		private List<IObject> DependentResults (SearchContext context)
		{
			List<IObject> results = null;
			string query = context.Query.ToLower ();

			if (context.ActionSearch && context.Query.Length == 0) {
				return InitialActionResults (context);
			}
			else if (context.LastContext.LastContext != null) {
				return FilterPreviousSearchResultsWithContinuedContext (context);
			}

			// Or else this is a brand new search:
			if (firstResults.ContainsKey (query))
				results = new List<IObject> (firstResults[query]);
			else
				results = new List<IObject> (universe.Values);

			// Filter the results appropriately.
			if (context.ModifierItemsSearch) {
				// Use a dictionary to get the intersection of the dynamic modifier
				// items for all items in the context.
				Dictionary <IItem, IItem> dynamicModItems = new Dictionary<IItem, IItem> ();
				foreach (IItem item in context.Items) {
					foreach (IItem modItem in
							context.Action.DynamicModifierItemsForItem (item)) {
						dynamicModItems[modItem] = modItem;
					}
				}
				// Add the intersected set to the results list.
				foreach (IItem modItem in dynamicModItems.Values) {
					results.Insert (0, modItem);
				}
				results = GetModItemsFromList (context, results);
			} else {
			 	// These are items:
				results = GetItemsFromList (context, results);
			}

			return results;
		}

		private List<IObject> SpecialItemsForContext (SearchContext context)
		{
			List<IObject> results = new List<IObject> ();
			IItem textItem = new DoTextItem (context.Query);
			
			// If we're on modifier items, add a text item if it's supported.
			if (context.ModifierItemsSearch) {
				if (context.Action.SupportsModifierItemForItems (context.Items.ToArray (), textItem)) {
					results.Add (textItem);
				}
			}
			// Same if we're on items.
			else if (context.ItemsSearch) {
				if (context.Action.SupportsItem (textItem)) {
					results.Add (textItem);
				}
			}
			// If independent, always add.
			else if (context.Independent) {
				results.Add (textItem);
			}

			return results;
		}

		// This generates a list of modifier items supported by the context in a given initial list.
		public List<IObject> GetModItemsFromList (SearchContext context, List<IObject> initialList)
		{
			List<IObject> results = new List<IObject> ();
			IItem[] items = context.Items.ToArray ();

			foreach (IObject iobject in initialList) {
				if (iobject is IItem) {
					// If the item is supported add it
					if (context.Action.SupportsModifierItemForItems (items, iobject as IItem)) {
						results.Add (iobject);
					}
				}
			}
			return results;
		}

		// Same as GetModItemsFrom list but for items
		public List<IObject> GetItemsFromList (SearchContext context, List<IObject> initialList)
		{
			List<IObject> results = new List<IObject> ();
			foreach (IObject iobject in initialList) {
				if (iobject is IItem) {
					if (context.Action.SupportsItem (iobject as IItem)) {
						results.Add (iobject);
					}
				}
			}
			return results;
		}

		// This will filter out the results in the previous context that match the current query
		private List<IObject> FilterPreviousSearchResultsWithContinuedContext (SearchContext context)
		{
			DoObjectRelevanceSorter sorter;
		 
			sorter = new DoObjectRelevanceSorter (context.Query.ToLower ());
			return sorter.SortResults (context.LastContext.Results);
		}


		private List<IObject> IndependentResults (SearchContext context)
		{
			string query;
			List<IObject> results;

			query = context.Query.ToLower ();
			// We can build on the last results.
			// example: searched for "f" then "fi"
			if (context.LastContext.LastContext != null) {
				results = FilterPreviousSearchResultsWithContinuedContext (context);
			}

			// If someone typed a single key, BOOM we're done.
			else if (firstResults.ContainsKey (query)) {
				results = new List<IObject> (firstResults[query]);
			}

			// Or we just have to do an expensive search...
			else {
				DoObjectRelevanceSorter sorter;
				sorter = new DoObjectRelevanceSorter (query);
				results = sorter.SortAndNarrowResults (universe.Values);
			}
			return results;
		}

		// This method gives us all the actions that are supported by all the items in the list
		private List<IObject> InitialActionResults (SearchContext context)
		{
			List<IObject> actions = new List<IObject> ();
			List<IObject> actions_to_remove = new List<IObject> ();
			bool initial = true;

			foreach (IItem item in context.Items) {
				List<IObject> item_actions = ActionsForItem (item);

				// If this is the first item in the list, add all of its supported actions.
				if (initial) {
					actions.AddRange (item_actions);
					initial = false;
				}
				//For every subsequent item, check every action in the pre-existing list
				//if its not supported by this item, remove it from the list
				else {
					foreach (IAction action in actions) {
						if (!item_actions.Contains (action)) {
							actions_to_remove.Add (action);
						}
					}
				}
			}
			foreach (IObject rm in actions_to_remove)
				actions.Remove (rm);
			return actions;
		}

		public List<IObject> ActionsForItem (IItem item)
		{
			List<IObject> item_actions;

			item_actions = new List<IObject> ();
			foreach (IAction action in doActions) {
				if (action.SupportsItem (item)) {
					item_actions.Add (action);
				}
			}
			return item_actions;
		}

		public List<IItem> ChildrenOfItem (IItem parent)
		{
			List<IItem> children;

			children = new List<IItem> ();
			foreach (DoItemSource source in doItemSources) {
				children.AddRange (source.ChildrenOfItem (parent));
			}
			return children;
		}
		
		/// <summary>
		/// Called when a node is added or removed
		/// </summary>
		/// <param name="s">
		/// A <see cref="System.Object"/>
		/// </param>
		/// <param name="args">
		/// A <see cref="ExtensionNodeEventArgs"/>
		/// </param>
		protected void OnItemSourcesChange (object s, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node = args.ExtensionNode as TypeExtensionNode;
			IItemSource source = (IItemSource) node.CreateInstance ();
			if (args.Change == ExtensionChange.Add)
			{
				try{
					doItemSources.Add (new DoItemSource (source));
					Log.Info ("Successfully loaded \"{0}\" itemsource.", source.Name);
				}catch (Exception e){
					Log.Info ("ItemSource \"{0}\" threw an exception while trying to load it." +e, source.Name);
				}
			}
			else{
				foreach (DoItemSource dis in doItemSources)
				{
					if (dis.Inner.Equals(source))
					{
						try{
							doItemSources.Remove (dis);
							Log.Info ("Successfully unloaded \"{0}\" itemsource.", source.Name);
						}catch (Exception e){
							Log.Info ("ItemSource \"{0}\" threw an exeption while trying to unload it." + e, source.Name);
						}
					}
				}
			}
		}
		protected void OnActionsChange (object s, ExtensionNodeEventArgs args)
		{
			TypeExtensionNode node = args.ExtensionNode as TypeExtensionNode;
			IAction action = (IAction) node.CreateInstance ();
			if (args.Change == ExtensionChange.Add)
			{
				try{
					doActions.Add (new DoAction(action));
					Log.Info ("Successfully loaded \"{0}\" action", action.Name);
				}catch (Exception e){
					Log.Info ("Action \"{0}\" threw an exception while trying to load it." + e, action.Name);
				}
			}
			else{
				foreach (DoAction da in doActions)
				{
					if (da.Inner.Equals (action))
					{
						try{
							doActions.Remove (da);
							Log.Info ("Successfully removed \"{0}\" action", action.Name);
						}catch (Exception e){
							Log.Info ("Action \"{0}\" threw an exeption while trying to unload it." + e, action.Name);
						}
					}
				}
			}	
		}
		
		private void initAddinManager ()
		{
			//Initialize the registry
			string userAddinDir = System.IO.Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".config");
			userAddinDir = System.IO.Path.Combine (userAddinDir, "gnome-do");
			AddinManager.Initialize (userAddinDir);
			
			//Check that  http://do.davebsd.com/repository/dev is registered in the repository
			SetupService setupService = new SetupService (AddinManager.Registry);
			if (!setupService.Repositories.ContainsRepository ("http://do.davebsd.com/repository/dev"))
			{
				setupService.Repositories.RegisterRepository (null, "http://do.davebsd.com/repository/dev", true); 
			}
			
		}
			
	}
}
