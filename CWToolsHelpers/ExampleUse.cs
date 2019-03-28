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
        /// <param name="modRoots">The root directory of any mods you also wish to use.  The mod list will be processed in the same way Stellaris would, with earlier mods in the list overriding later ones.  (Conflict resolution: first-in wins), thus suggest you use an ordered collection!</param>
        public static void Example(string stellarisRoot, string stellarisUserDirectory, string modConfigFilePath) {
            
            // Setting up mods
            // if you don't want mods, you can skip all this to the "ending mods" comment
            // These Steps will show you the steps you need to take to work with mods that you have installed in stellaris with a minimum amount of boilerplate.
            // There are several steps, where you will do different ones on different runs of the code
            
            // First: Get all mods you are using in Stellaris and write out to a file that can be easily edited.
            
            // Open that file in your favourite text editor and review the mods it has found
            // Set the "include" property to true for all the mods you want to parse
            // For mods that are sensible to group together (E.g. the various mods in the At War series or Zenith of Fallen Empires) set a mod group so they will get processed together.
            
            // Last step: run the program again, loading that file from disk and making the mod definitions.
            
            
            // Locate your mods.
            // These are all the mods configured in the Stellaris Launcher
            IList<ModDefinitionFile> stellarisModDefinitions = ModDirectoryHelper.LoadModDefinitions(stellarisUserDirectory);
            // Write a config file that you can edit.
            ModDirectoryHelper.WriteModInfoFile(modConfigFilePath, stellarisModDefinitions);
            
            // FIRST RUN: STOP HERE!  GO EDIT THE FILE YOU JUST CREATED.  Then remember to not run the load mods and write mod file on your next run, or you are going to overwrite your edits.
            
            // TODO write a "merge" tool that will take an existing config and a stellaris directory and add any mods missing to the config but not overwrite changes.
            
            // Load the mod configs
            // This will be all the mods, including ones with include : false as they get filtered later.
            List<ModInfo> modInfos = ModDirectoryHelper.ReadModInfoFile(modConfigFilePath);

            // now create StellarisDirectoryHelpers for accessing your mods.
            // this will handling extracting zip files for you
            // and will only work over mods you ahve set the include parameter on.
            // The force override flag determines whether a mod will be extracted from steam workshop event if it already exists in the temp directory
            // For the first run of a "session" as you are parsing i alwayus recommend setting to true, to make sure you get the latest mod data
            // If you are running multiple times in succession (e.g. debugging your app), then set to false to speed things up.
            IList<StellarisDirectoryHelper> modDirectoryHelpers = ModDirectoryHelper.CreateDirectoryHelpers(modInfos, true);

            
            
            // Done with all the preprocessing for mods now.
            
            
            // First job is to get a Directory Helper for Stellaris Root.  
            // While at the moment I could get away with changing the API to simply take an IEnumerable that includes the main stellaris directory
            // I want to keep the door open for the time when the utilities have differing behaviour for the game directory and any mods

            // For ease there are overloaded constructors on all utilities that just take the root stellaris directory when you are not loading mods,
            // but this example will always use the full ones.
            // the utilities will also cope happily with null or empty mod collections.

            var stellarisDirectoryHelper = new StellarisDirectoryHelper(stellarisRoot);
            
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
            var allDirectoryHelpers = StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers);
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