using System.Collections.Generic;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using NetExtensions.Collection;

namespace CWToolsHelpers.ScriptedVariables {
    
    /// <summary>
    /// Manages scripted variables from the core game and mods.
    /// </summary>
    public class ScriptedVariableAccessor : IScriptedVariablesAccessor{
        private IDirectoryWalker DirectoryWalker { get; }
        private ICWParserHelper CWParserHelper { get; }

        private readonly IDictionary<string, string> variables;

        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper) : 
            this(stellarisDirectoryHelper, new StellarisDirectoryHelper[] { }) {
        }

        public ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers) :
            this(stellarisDirectoryHelper, modDirectoryHelpers, new DirectoryWalker(), new CWParserHelper()) {
        }

        internal ScriptedVariableAccessor(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            IDirectoryWalker directoryWalker,
            ICWParserHelper cwParserHelper) {
            DirectoryWalker = directoryWalker;
            CWParserHelper = cwParserHelper;

            variables = new Dictionary<string, string>();
            foreach (var directoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                var modVariables = ParseScriptedVariables(directoryHelper.ScriptedVariables);
                variables.PutAll(modVariables);
            }
        } 
        
        /// <inheritdoc />
        public string GetPotentialValue(string rawValue) {
            if (rawValue != null && rawValue.StartsWith("@") && variables.ContainsKey(rawValue))
            {
                return variables[rawValue];
            }

            return rawValue;
        }
        
        private Dictionary<string, string> ParseScriptedVariables(string scriptedVariableDir)
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(scriptedVariableDir, StellarisDirectoryHelper.TextMask);
            var parsedTechFiles = CWParserHelper.ParseParadoxFiles(techFiles);
            var result = new Dictionary<string, string>();
            foreach(var file in parsedTechFiles.Values)
            {
                // top level nodes are files, so we process the immediate children of each file, which is the individual variables.
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
        /// <summary>
        /// Always returns the passed in value.
        /// </summary>
        public string GetPotentialValue(string rawValue) {
            return rawValue;
        }
    }
}