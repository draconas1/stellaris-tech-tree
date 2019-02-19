using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CWTools.Common;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using CWToolsHelpers.ScriptedVariables;
using NetExtensions.Collection;

namespace CWToolsHelpers {
    
    /// <summary>
    /// A documented example in how to use the helpers.
    /// </summary>
    public static class ExampleUse {
        /// <summary>
        /// Example of using the helpers to parse tech files.
        /// </summary>
        /// <param name="stellarisRoot">The root directory to Stellaris</param>
        /// <param name="modRoots">The root directory of any mods you also wish to use.  The mods will be loaded in order, with later mods overriding earlier ones (as per Stellaris), thus suggest you use an ordered collection!</param>
        public static void Example(string stellarisRoot, IEnumerable<string> modRoots) {
            // First create DirectoryHelpers for stellaris and all the mods.
            // While at the moment I could get away with changing the API to simply take an IEnumerable that includes the main stellaris directory
            // I want to keep the door open for the time when the utilities have differing behaviour for the game directory and any mods

            // For ease there are overloaded constructors on all utilities that just take the root stellaris directory when you are not loading mods,
            // but this example will always use the full ones.

            var stellarisDirectoryHelper = new StellarisDirectoryHelper(stellarisRoot);
            var modDirectoryHelpers = new List<StellarisDirectoryHelper>();
            modDirectoryHelpers.AddRange(modRoots.Select(root => new StellarisDirectoryHelper(root)));
            
            // create a localisation helper, need to specify the Language for this
            // this gives easy way to look up localised strings from ids
            // 2 methods: GetName and GetDescription 
            // GetName: gets the direct value for a key
            // GetDescription: gets the value for key + "_desc" as its such a common operation it gets a helper.
            // both of these methods will follow any $OtherLocalisationString$ variables in the localisation file (and strip out comments) to generate a complete
            // non-variabled string but will not process formatting codes
            var localisationApiHelper = new LocalisationApiHelper(stellarisDirectoryHelper, modDirectoryHelpers, STLLang.English);
            
            // Numerous values in the game files make use of variables in the scriptedVariables folder.  e.g. @tier1cost1 for level 1 tech costs
            // The scripted variables accessor handles the converting of these into their actual values
            // If it cannot look up a value for a given request it returns the requesting key
            var scriptedVariableAccessor = new ScriptedVariableAccessor(stellarisDirectoryHelper, modDirectoryHelpers);


            // Create the main parser helper
            // You do not need to assign a scripted variables helper if you don't want to resolve variables.
            var cwParserHelper = new CWParserHelper(scriptedVariableAccessor);

            // parse over all the tech files and convert each teach into a basic descriptive string
            // am storing each tech in a dictionary to allow the mods (which process after main) to override values in the core game.
            var allDirectoryHelpers = new List<StellarisDirectoryHelper> {stellarisDirectoryHelper};
            allDirectoryHelpers.AddRange(modDirectoryHelpers);
            var result = new Dictionary<string, string>();
            foreach (var directoryHelper in allDirectoryHelpers) {
                
                // use DirectoryWalker to find all tech files to process.  Do not want to process the tier or category files.
                List<FileInfo> techFiles = DirectoryWalker.FindFilesInDirectoryTree(directoryHelper.Technology, StellarisDirectoryHelper.TextMask, new [] { "00_tier.txt", "00_category.txt" });
                
                // each node is the contents of an individual file, keyed by file name
                IDictionary<string,CWNode> parsedTechFiles = cwParserHelper.ParseParadoxFiles(techFiles.Select(x => x.FullName).ToList());
                foreach(var fileAndContents in parsedTechFiles)
                {
                    // top level nodes are files, so we process the immediate children of each file, which is the individual techs.
                    foreach (var techNode in fileAndContents.Value.Nodes) {
                        // extract some info about the tech and put it in a string
                        string techString = "[" + techNode.GetKeyValue("area") + "]" +
                                            "(" + (techNode.GetKeyValue("tier") ?? "0") + ") " +
                                            localisationApiHelper.GetName(techNode.Key) + ": " +
                                            localisationApiHelper.GetDescription(techNode.Key) + 
                                            " (from: " + fileAndContents.Key + ")";
                        result[techNode.Key] = techString;
                    }
                }
            }
            
            // loop over and output
            result.Values.ForEach(Console.WriteLine); 
        }
    }
}