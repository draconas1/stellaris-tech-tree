using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using NetExtensions.Collection;

namespace CWToolsHelpers.ScriptedVariables {
    
    /// <summary>
    /// Manages scripted variables from the core game and mods.
    /// </summary>
    public class ScriptedVariableAccessor : IScriptedVariablesAccessor{
        private readonly IDictionary<string, string> variables;

        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper) : this(
            stellarisDirectoryHelper, new StellarisDirectoryHelper[] { }) {
        }
        
        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers) {
            var scriptedVars = ParseScriptedVariables(stellarisDirectoryHelper.ScriptedVariables);
            foreach (var directoryHelper in modDirectoryHelpers) {
                var modVariables = ParseScriptedVariables(directoryHelper.ScriptedVariables);
                scriptedVars.PutAll(modVariables);
            }

            variables = scriptedVars;
        }
        
        public string GetPotentialValue(string rawValue) {
            if (rawValue != null && rawValue.StartsWith("@") && variables.ContainsKey(rawValue))
            {
                return variables[rawValue];
            }

            return rawValue;
        }
        
        private Dictionary<string, string> ParseScriptedVariables(string scriptedVariableDir)
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(scriptedVariableDir, DirectoryWalker.TextMask);
            var parsedTechFiles = new CWParserHelper().ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());
            var result = new Dictionary<string, string>();
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immiediate children of each file, which is the individual variables.
                foreach (var keyValue in file.KeyValues)
                {
                    result[keyValue.Key] = keyValue.Value;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Implementation of <see cref="IScriptedVariablesAccessor"/> that always returns the <c>rawValue</c>
    /// </summary>
    public class DummyScriptedVariablesAccessor : IScriptedVariablesAccessor {
        public string GetPotentialValue(string rawValue) {
            return rawValue;
        }
    }
}