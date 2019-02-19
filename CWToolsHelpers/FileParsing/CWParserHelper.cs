using System.Collections.Generic;
using System.Linq;
using CWTools.CSharp;
using CWTools.Parser;
using CWTools.Process;
using CWToolsHelpers.ScriptedVariables;

namespace CWToolsHelpers.FileParsing
{
    /// <summary>
    /// Main Helper class for using the CWTools library to parse general PDX files into a (raw) DTO.
    /// </summary>
    public class CWParserHelper : ICWParserHelper {
        private readonly IScriptedVariablesAccessor scriptedVariablesAccessor;

        /// <summary>
        /// Create a CWParserHelper that will not attempt to resolve in files.
        /// </summary>
        public CWParserHelper() : this(new DummyScriptedVariablesAccessor()) {
        }
        
        /// <summary>
        /// Create a CWParserHelper where the nodes will attempt to resolve variables using the specified <see cref="IScriptedVariablesAccessor"/>.
        /// </summary>
        public CWParserHelper(IScriptedVariablesAccessor scriptedVariablesAccessor) {
            this.scriptedVariablesAccessor = scriptedVariablesAccessor;
        }
        
        /// <inheritdoc />
        public IDictionary<string, CWNode> ParseParadoxFiles(IEnumerable<string> filePaths)
        {
            var result = new Dictionary<string, CWNode>();
            foreach (string paradoxFile in filePaths)
            {
                result[paradoxFile] = (ParseParadoxFile(paradoxFile));
            }
            return result;
        }

        /// <inheritdoc />
        public CWNode ParseParadoxFile(string filePath)
        {
            // raw parsing
            var parsed = CKParser.parseEventFile(filePath);

            // this is an extension method in CWTools.CSharp
            var eventFile = parsed.GetResult();

            //"Process" result into nicer format
            CK2Process.EventRoot processed = CK2Process.processEventFile(eventFile);      

            // marshall this into a more c# fieldy type using the CWTools example
            CWNode marshaled = ToMyNode(processed);

            return marshaled;
        }

        
        private CWNode ToMyNode(Node n)
        {
            var nodes = n.AllChildren.Where(x => x.IsNodeC).Select(x => ToMyNode(x.node)).ToList();
            var leaves = n.AllChildren.Where(x => x.IsLeafC).Select(x => ToMyKeyValue(x.leaf)).ToList();
            var values = n.AllChildren.Where(x => x.IsLeafValueC).Select(x => x.lefavalue.Key).ToList();
            return new CWNode(n.Key) { Nodes = nodes, Values = values, KeyValues = leaves, ScriptedVariablesAccessor = scriptedVariablesAccessor};
        }

        private static CWKeyValue ToMyKeyValue(Leaf l)
        {
            return new CWKeyValue { Key = l.Key, Value = l.Value.ToRawString() };
        }
    }
}
