// UniverseManager.cs created with MonoDevelop
// User: dave at 4:01 PM 10/30/2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Collections.Generic;

using Do;
using Do.Universe;

namespace Do.Core
{	
	
	public class UniverseManager
	{
		//Add GTK dialog box, while indexing
		
		Dictionary<string, IObject[]> firstCharacterResults;
	
		// Change to univariate hash
		Dictionary<int, IObject> universe;
		
		Dictionary<Command, List<IObject>> commandToItemMap;
		
		private List<ItemSource> itemSources;
		private List<Command> all_commands;
		
		public UniverseManager()
		{
			
			itemSources = new List<ItemSource> ();
			universe = new Dictionary<int, IObject> ();
			all_commands = new List<Command> ();
			commandToItemMap = new Dictionary<Command, List<IObject>> ();

			IndexCommands ();
			IndexItems ();
			BuildFirstKeyCache ();
		}
		
		private void BuildFirstKeyCache () 
		{			
			List<IObject> keypress_matches;
			RelevanceSorter comparer;
			
			//Do this: Load/save results to XML

			firstCharacterResults = new Dictionary<string, IObject[]> ();
			//For each starting character add every matching object from the universe to
			//the firstCharacterResults dictionary with the key of the character
			for (char keypress = 'a'; keypress < 'z'; keypress++) {
				List <IObject> universeList = new List<IObject> ();
				universeList.AddRange (universe.Values);
				comparer = new RelevanceSorter (keypress.ToString ());
				firstCharacterResults[keypress.ToString ()] = comparer.NarrowResults (universeList).ToArray ();
			}
		}
		
		private void IndexItems () {
			
			foreach (ItemSource source in BuiltinItemSources) {
				itemSources.Add (source);
			}
			
			//For each item add to universe, then check against all commands
			//to see if the item should be added to the list in the command to item map
			foreach (ItemSource source in itemSources) {
				foreach (Item item in source.Items) {
					foreach (Command command in all_commands) {
						List<IObject> commandResults = commandToItemMap[command];
						List<Type> supportedItemTypes = new List<Type>
							(command.SupportedItemTypes);
						List<Type> implementedItemTypes = new List<Type>
							(GCObject.GetAllImplementedTypes (item.IItem));
						foreach (Type type in supportedItemTypes) {
							if (implementedItemTypes.Contains (type)) {
								commandResults.Add (item);
								break;
							}
						}
					}
					
					if (universe.ContainsKey (item.GetHashCode ())) {
					}
					else {
						universe.Add (item.GetHashCode (), item);
					}
				}
			}
		}
		
		private void IndexCommands () {
			//For each command addit to the universe, to the list of commands and
			//initialize the list for the command to item map
			foreach (Command command in BuiltinCommands) {
				if (!(universe.ContainsKey (command.GetHashCode ()))) {
					universe[command.GetHashCode ()] = command;
				}
				all_commands.Add (command);
				commandToItemMap.Add (command, new List<IObject> ());
			}
		}
		
		
		public SearchContext Search (SearchContext newSearchContext)
		{
			string keypress = newSearchContext.SearchString;
			List<IObject> filtered_results;
			SearchContext lastContext = newSearchContext.LastContext;
		
			//Get the results based on the search string
			filtered_results = GenerateUnfilteredList (newSearchContext);
			//Filter results based on the required type
			filtered_results = filterResultsByType (filtered_results, newSearchContext.SearchTypes, keypress);
			//Filter results based on object dependencies
			filtered_results = filterResultsByDependency(filtered_results, newSearchContext.FirstObject);

			newSearchContext.Results = filtered_results.ToArray ();
			// This is a clever way to keep
			// a stack of incremental results.
			// NOTE: Clone should return a deep (enough) copy.
			// Also note - tricky pointer magic.
			SearchContext temp;
			temp = newSearchContext;
			newSearchContext = newSearchContext.Clone ();
			lastContext = temp;
			newSearchContext.LastContext = lastContext;
			
			return newSearchContext;
		}
		
		private List<IObject> GenerateUnfilteredList (SearchContext newSearchContext) 
		{
			string keypress;
			RelevanceSorter comparer;
			Dictionary<string, IObject[]> firstResults;
			SearchContext lastContext;
			List<IObject> filtered_results;
			
			keypress = newSearchContext.SearchString.ToLower ();
			lastContext = newSearchContext.LastContext;
		
			//If this is the initial search for the all the corresponding items/commands for the first object
			/// we don't need to filter based on search string
			if (newSearchContext.SearchString == "" && newSearchContext.FirstObject != null) {
				filtered_results = new List<IObject> ();
				//If command, just grab the commands for the item
				if (ContainsType (newSearchContext.SearchTypes, typeof (ICommand))) {
					foreach (Command command in CommandsForItem (newSearchContext.FirstObject as Item)) {
						filtered_results.Add (command);
					}
				}
				//If item, use the command to item map
				else if (ContainsType (newSearchContext.SearchTypes, typeof (IItem))) {
					commandToItemMap.TryGetValue (newSearchContext.FirstObject as Command, out filtered_results);
				}
			}
			else {
				// We can build on the last results.
				// example: searched for "f" then "fi"
				if (lastContext != null) {
					comparer = new RelevanceSorter (keypress);
					filtered_results = new List<IObject> (lastContext.Results);
					// Sort results based on new keypress string
					filtered_results = comparer.NarrowResults (filtered_results);
				}

				// If someone typed a single key, BOOM we're done.
				else if (firstCharacterResults.ContainsKey (keypress)) {
					filtered_results = new List<IObject> 
						(firstCharacterResults[keypress]);
				}

				// Or we just have to do an expensive search...
				// This is the current behavior on first keypress.
				else {
					filtered_results = new List<IObject> ();
					filtered_results.AddRange (universe.Values);
					comparer = new RelevanceSorter (keypress);
					filtered_results.Sort (comparer);
					// Sort results based on keypress
				}
			}
			return filtered_results;
		}
			
		
		private List<IObject> filterResultsByType (List<IObject> results, Type[] acceptableTypes, string keypress) 
		{
			List<IObject> filtered_results = new List<IObject> ();
			//Add a text item based on the key entered
			if (keypress != "") 
				results.Add (new Item (new TextItem (keypress)));
			else
				results.Add (new Item (new TextItem ("Enter Word Definition")));
			
			//Now we look through the list and add an object when it's type belongs in acceptableTypes
			foreach (IObject iobject in results) {
				List<Type> implementedTypes = GCObject.GetAllImplementedTypes (iobject);
				foreach (Type type in implementedTypes) {
				}
				foreach (Type type in acceptableTypes) {
					if (implementedTypes.Contains (type)) {
						filtered_results.Add (iobject);
						break;
					}
				}
			}
			return filtered_results;
		}
		
		private List<IObject> filterResultsByDependency (List<IObject> results, IObject independentObject)
		{
			if (independentObject == null)
				return results;
			List <IObject> filtered_results = new List<IObject> ();
			
			
			if (independentObject is Command) {
				foreach (IObject iobject in results) {
					//If the independent object is a command, add the result if its item type is supported
					List<Type> supportedItemTypes = new List<Type>
						(((Command) independentObject).SupportedItemTypes);
					List<Type> implementedItemTypes = new List<Type>
						(GCObject.GetAllImplementedTypes ((iobject as Item).IItem));
					foreach (Type type in supportedItemTypes) {
						if (implementedItemTypes.Contains (type)) {
							filtered_results.Add (iobject);
							break;
						}
					}
				}
			}
			else if (independentObject is Item) {
				foreach (IObject iobject in results) {
					//If the ind. object is an item, run the function commands for items to see if the result is in it
					List<Command> supportedCommands = CommandsForItem (independentObject as Item);
					if (supportedCommands.Contains (iobject as Command)) {
						filtered_results.Add (iobject);
					}
				}
			}
			return filtered_results;
		}
				
		
		//Function to determine whether a type array contains a type
		private bool ContainsType (Type[] typeArray, Type checkType) {
			foreach (Type type in typeArray) {
				if (type.Equals (checkType))
					return true;
			}
			return false;
		}
		
		
		public static ItemSource [] BuiltinItemSources {
			get {
				return new ItemSource [] {
					new ItemSource (new ApplicationItemSource ()),
					new ItemSource (new FirefoxBookmarkItemSource ()),
					new ItemSource (new DirectoryFileItemSource ()),
					new ItemSource (new GNOMESpecialLocationsItemSource ()),	
				};
			}
		}
		
		public static Command [] BuiltinCommands {
			get {
				return new Command [] {
					new Command (new RunCommand ()),
					new Command (new OpenCommand ()),
					new Command (new OpenURLCommand ()),
					new Command (new RunInShellCommand ()),
					new Command (new DefineWordCommand ()),
				};
			}
		}
		
		List<Command> CommandsForItem (Item item)
		{
			List<Command> commands;

			commands = new List<Command> ();
			foreach (Command command in all_commands) {
				foreach (Type item_type in command.SupportedItemTypes) {
					if (item_type.IsAssignableFrom (item.IItem.GetType ()) 
					    && command.SupportsItem (item.IItem)) {
						commands.Add (command);
					}
				}
			}
			return commands;
		}
		
	}
}
