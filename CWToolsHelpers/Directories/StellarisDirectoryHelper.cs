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
        public const string TextMask = "*.txt";
        
        public string ModName { get; }
        
        public string Root { get; }

        public string Common => GetCommonDirectory(Root);

        public string Technology => GetTechnologyDirectory(Root);

        public string ScriptedVariables => GetScriptedVariablesDirectory(Root);

        public string Icons => GetIconsDirectory(Root);

        public string Localisation => GetLocalisationDirectory(Root);
        public string Buildings => GetBuildingsDirectory(Root);

        public StellarisDirectoryHelper(string rootDirectory) {
            Root = rootDirectory;
            ModName = new DirectoryInfo(rootDirectory).Name;
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

        public static string GetLocalisationDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "localisation");
        }

        public static string GetScriptedVariablesDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "scripted_variables");
        }

        public static string GetIconsDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "gfx", "interface", "icons");
        }

        /// <summary>
        /// Helper as a lot of the API's want the main game directory and mod directories as separate items, but will process them the same, just with the order changing depending on what is to be overrriden.
        /// </summary>
        /// <param name="stellarisDirectoryHelper">The main game directoryHelper</param>
        /// <param name="modDirectoryHelpers">Directory helpers for the game mod, may be <c>null</c>.  This should be in the order they appear in the game loader (usually alphabetal) where conflicts will be resolved by a "first in wins" strategy.  E.g. Mods that are earlier on this list will overwrite mods that are later in this list.</param>
        /// <param name="position">Where the main game helper should be inserted into the result list, defaults to the first position, as the most often scenario is for it to be processed first then overriden by mods</param>
        /// <returns>A list contianing the game directory and the mod directories, with the game inserted in the specified location</returns>
        public static IList<StellarisDirectoryHelper> CreateCombinedList(
            StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            StellarisDirectoryPositionModList position = StellarisDirectoryPositionModList.First) {
            var stellarisDirectoryHelpers = modDirectoryHelpers.NullToEmpty().Reverse().ToList();
            switch (position) {
                case StellarisDirectoryPositionModList.First:
                    stellarisDirectoryHelpers.Insert(0, stellarisDirectoryHelper);
                    break;
                case StellarisDirectoryPositionModList.Last:
                    stellarisDirectoryHelpers.Add(stellarisDirectoryHelper);
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