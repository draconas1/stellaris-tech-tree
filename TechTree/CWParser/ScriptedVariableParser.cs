using System.Collections.Generic;
using System.Linq;
using TechTree.FileIO;

namespace TechTree.CWParser
{
    class ScriptedVariableParser
    {
        private readonly string scriptedVariableDir;

        public ScriptedVariableParser(string scriptedVariableDir)
        {
            this.scriptedVariableDir = scriptedVariableDir;
        }

        public Dictionary<string, string> ParseScriptedVariables()
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(scriptedVariableDir, DirectoryWalker.TEXT_MASK);
            var parsedTechFiles = new CWParserHelper().ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());
            var result = new Dictionary<string, string>();
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immiedate children of each file, which is the individual variables.
                foreach (var keyValue in file.KeyValues)
                {
                    result[keyValue.Key] = keyValue.Value;
                }
            }
            return result;
        }
    }
}
