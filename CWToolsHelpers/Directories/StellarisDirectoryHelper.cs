using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using NetExtensions.Collection;

namespace CWToolsHelpers.Directories {
    /// <summary>
    /// Set of helpers for navigating to specific stellaris/mod directories.
    /// </summary>
    public class StellarisDirectoryHelper {
        /// <summary>
        /// Default for the file extension we are usually interested in.
        /// </summary>
        public const string TextMask = "*.txt";

        /// <summary>
        /// The name of the Stellaris root directory for locating the helper that is for the core game.
        /// </summary>
        public const string StellarisCoreRootDirectory = "Stellaris";

        /// <summary>
        /// The name of the root directory for this helper (will be the stellaris directory for the main game, or the mods directory for a mod)
        /// </summary>
        public string ModName { get; }
        /// <summary>
        /// A free text name to allow directories for multiple different mods to be grouped.  E.g. EAC, Dadinator, Ascended Fallen Empire
        /// </summary>
        public string ModGroup { get; set; }
        
        /// <summary>
        /// <c>true</c> if this helper is for the core game, <c>false</c> if its for a mod.
        /// </summary>
        public bool IsCoreGameHelper => ModName == StellarisCoreRootDirectory && Root.EndsWith(StellarisCoreRootDirectory);
        
        /// <summary>
        /// The path to the root of the mod directory
        /// </summary>
        public string Root { get; }

        public string Common => GetCommonDirectory(Root);

        public string Technology => GetTechnologyDirectory(Root);

        public string ScriptedVariables => GetScriptedVariablesDirectory(Root);

        public string Icons => GetIconsDirectory(Root);

        public string Localisation => GetLocalisationDirectory(Root);
        public string Buildings => GetBuildingsDirectory(Root);

        public string Decisions => GetDecisionsDirectory(Root);

        public string ComponentTemplates => GetComponentTemplatesDirectory(Root);
        
        public string ComponentSets => GetComponentSetsDirectory(Root);

        public StellarisDirectoryHelper(string rootDirectory, string grouping = null) {
            Root = rootDirectory;
            ModName = new DirectoryInfo(rootDirectory).Name;
            ModGroup = grouping ?? ModName;
        }

        public static string GetCommonDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "common");
        }

        public static string GetTechnologyDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "technology");
        }
        
        private string GetBuildingsDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "buildings");
        }
        
        private string GetDecisionsDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "decisions");
        }

        public static string GetLocalisationDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "localisation");
        }

        public static string GetScriptedVariablesDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "scripted_variables");
        }

        public static string GetIconsDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "gfx", "interface", "icons");
        }

        public static string GetComponentTemplatesDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "component_templates");
        }
        
        public static string GetComponentSetsDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "component_sets");
        }
        
        /// <summary>
        /// Helper as a lot of the API's want the main game directory and mod directories as separate items, but will process them the same, just with the order changing depending on what is to be overrriden.
        /// </summary>
        /// <param name="stellarisDirectoryHelper">The main game directoryHelper</param>
        /// <param name="modDirectoryHelpers">Directory helpers for the game mod, may be <c>null</c>.  This should be in the order they appear in the game loader (usually alphabetical) where conflicts will be resolved by a "first in wins" strategy.  E.g. Mods that are earlier on this list will overwrite mods that are later in this list.</param>
        /// <param name="position">Where the main game helper should be inserted compared to the source list of mods.  Defaults to "last" as the main game is usually overriden by all the mods, switching it to first will mean the game will override any conflicting items from mods</param>
        /// <returns>A list containing the game directory and the mod directories, with the game inserted in the specified location.  The list will be the reverse of what was passed in, because the easiest way to process things that need to override other things is to use a Dictionary and a "last in wins" strategy, which si the opposite of how the stellaris listing works.</returns>
        public static IList<StellarisDirectoryHelper> CreateCombinedList(
            StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            StellarisDirectoryPositionModList position = StellarisDirectoryPositionModList.Last) {
            var stellarisDirectoryHelpers = modDirectoryHelpers.NullToEmpty().Reverse().ToList();
            // Because the source list has been reversed, we do the opposite of the insertion position to make it line up with the now reversed list
            // (because it is much easier to override thigns in a map by using a "last in wins" strategy.
            switch (position) {
                case StellarisDirectoryPositionModList.First:
                    stellarisDirectoryHelpers.Add(stellarisDirectoryHelper);
                    break;
                case StellarisDirectoryPositionModList.Last:
                    stellarisDirectoryHelpers.Insert(0, stellarisDirectoryHelper);
                    break;
                default: throw new Exception("Unknown StellarisDirectoryPositionModList " + position);
            }

            return stellarisDirectoryHelpers;
        }
        
        public enum StellarisDirectoryPositionModList {
            First,
            Last
        }
    }
}