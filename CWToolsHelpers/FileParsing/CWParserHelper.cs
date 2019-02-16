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
    public class CWParserHelper
    {
        public IScriptedVariablesAccessor ScriptedVariablesAccessor { get; set; }
        
        /// <summary>
        /// Loops over collection of files and parses them using <see cref="ParseParadoxFile(System.String)"/>
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public List<CWNode> ParseParadoxFile(IEnumerable<string> filePaths)
        {
            var result = new List<CWNode>();
            foreach (string paradoxFile in filePaths)
            {
                result.Add(ParseParadoxFile(paradoxFile));
            }
            return result;
        }

        /// <summary>
        /// Main method for using the CWTools library to parse an individual file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A CWNode representing the contents of the file.</returns>
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
            return new CWNode(n.Key) { Nodes = nodes, Values = values, KeyValues = leaves, ScriptedVariablesAccessor = ScriptedVariablesAccessor};
        }

        private static CWKeyValue ToMyKeyValue(Leaf l)
        {
            return new CWKeyValue { Key = l.Key, Value = l.Value.ToRawString() };
        }
    }
}
