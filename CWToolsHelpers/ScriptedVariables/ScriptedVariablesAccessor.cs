using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using NetExtensions.Collection;
using Serilog;

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
                if (Directory.Exists(directoryHelper.ScriptedVariables)) {
                    var modVariables = ParseScriptedVariables(directoryHelper.ScriptedVariables);
                    variables.PutAll(modVariables);
                }
                else {
                    Log.Logger.Debug("{0} does not contain scripted variables", directoryHelper.ModName);
                }
                
            }
        }

        private ScriptedVariableAccessor(IEnumerable<CWKeyValue> keyValues, ICWParserHelper cwParserHelper) {
            DirectoryWalker = null;
            CWParserHelper = cwParserHelper;
            variables = new Dictionary<string, string>();
            keyValues.Where(kv => IsVariable(kv.Key)).ForEach(kv => variables[kv.Key] = kv.Value);
        }
        
        /// <inheritdoc />
        public string GetPotentialValue(string rawValue) {
            if (rawValue != null && IsVariable(rawValue) && variables.ContainsKey(rawValue))
            {
                var value = variables[rawValue];
                return IsVariable(value) ? GetPotentialValue(value) : value;
            }

            return rawValue;
        }

        private bool Contains(string key) {
            return variables.ContainsKey(key);
        }

        public static bool IsVariable(string key) {
            return key.StartsWith('@');
        }
        
        public void AddAdditionalFileVariables(CWNode node) {
            node.RawKeyValues.Where(kv => IsVariable(kv.Key)).ForEach(kv => variables[kv.Key] = kv.Value);
        }

        public IScriptedVariablesAccessor CreateNew(IEnumerable<CWKeyValue> keyValues) {
            return new DelegatingScriptedVariablesAccessor(new ScriptedVariableAccessor(keyValues, CWParserHelper), this);
        }


        private class DelegatingScriptedVariablesAccessor : IScriptedVariablesAccessor {
            private readonly IScriptedVariablesAccessor primary;
            private readonly IScriptedVariablesAccessor fallback;

            internal DelegatingScriptedVariablesAccessor(IScriptedVariablesAccessor primary, IScriptedVariablesAccessor fallback) {
                this.primary = primary;
                this.fallback = fallback;
            }
            public void Dispose() {
            }

            public string GetPotentialValue(string rawValue) {
                var potentialValue = primary.GetPotentialValue(rawValue);
                return potentialValue == rawValue ? fallback.GetPotentialValue(rawValue) : potentialValue;
            }

            public IScriptedVariablesAccessor CreateNew(IEnumerable<CWKeyValue> node) {
                return new DelegatingScriptedVariablesAccessor(primary.CreateNew(node), fallback);
            }

            public void AddAdditionalFileVariables(CWNode node) {
                primary.AddAdditionalFileVariables(node);
            }
        }
        
        private Dictionary<string, string> ParseScriptedVariables(string scriptedVariableDir)
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(scriptedVariableDir, StellarisDirectoryHelper.TextMask);
            var parsedTechFiles = CWParserHelper.ParseParadoxFiles(techFiles);
            var result = new Dictionary<string, string>();
            foreach(var file in parsedTechFiles.Values)
            {
                // top level nodes are files, so we process the immediate children of each file, which is the individual variables.
                AddAdditionalFileVariables(file);
            }
            return result;
        }

        public void Dispose() {
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

        public IScriptedVariablesAccessor CreateNew(IEnumerable<CWKeyValue> node) {
            return this;
        }

        public void AddAdditionalFileVariables(CWNode node) {
        }

        public void Dispose() {
        }
    }
}