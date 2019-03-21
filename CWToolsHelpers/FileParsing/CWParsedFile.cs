using System;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.ScriptedVariables;

namespace CWToolsHelpers.FileParsing
{
    /// <summary>
    /// A complex object in the paradox file that has many children.  Most top level items are Nodes.  
    /// </summary>
    public class CWNode
    {
        public CWNode(string key) {
            Key = key;
        }
        
        /// <summary>
        /// The node key
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// All child nodes of this one.
        /// </summary>
        /// <remarks>
        /// Would really like to use a dictionary here, but duplicate node keys are entirely possible, as keys are often things like logical operators
        /// </remarks>
        public IList<CWNode> Nodes { get; set; }
        
        /// <summary>
        /// All key value pairs.
        /// </summary>
        /// <remarks>
        /// Would really like to use a dictionary here, but duplicate keys are entirely possible, as keys are often things like logical operators
        /// </remarks>
        public IList<CWKeyValue> KeyValues { get; set; }

        /// <summary>
        /// Straight values within the node, these are almost always comments.
        /// </summary>
        public IList<string> Values { get; set; }

        private IScriptedVariablesAccessor scriptedVariablesAccessor;

        public IScriptedVariablesAccessor ScriptedVariablesAccessor {
            get => scriptedVariablesAccessor ?? (scriptedVariablesAccessor = new DummyScriptedVariablesAccessor());
            set => scriptedVariablesAccessor = value;
        }

        /// <summary>
        /// Gets the first child node with the specified key.
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first node if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">They key</param>
        /// <returns></returns>
        public CWNode GetNode(string key)
        {
            return Nodes.FirstOrDefault(x => x.Key == key);
        }

        /// <summary>
        /// Get all child nodes with the specified key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns></returns>
        public IEnumerable<CWNode> GetNodes(string key) {
            return Nodes.Where(x => x.Key == key);
        }
        
        /// <summary>
        /// If there are any child nodes with the given key, performs the specified Action on them.
        /// </summary>
        /// <param name="key">The key of the child nodes</param>
        /// <param name="perform">The Action to perform if any are found</param>
        public void ActOnNodes(string key, Action<CWNode> perform) {
            ActOnNodes(key, perform, () => { });
        }
        
        /// <summary>
        /// If there are any child nodes with the given key, performs the specified Action on them, otherwise perform the no match action.
        /// </summary>
        /// <param name="key">The key of the child nodes</param>
        /// <param name="perform">The Action to perform if any are found</param>
        /// <param name="performIfNoMatch">The Action to perform if there is no nodes with the specified key</param>
        public void ActOnNodes(string key, Action<CWNode> perform, Action performIfNoMatch) {
            var nodes = GetNodes(key);
            if (nodes.Any()) {
                foreach (var cwNode in nodes) {
                    perform(cwNode);
                }
            }
            else {
                performIfNoMatch();
            }
        }

        /// <summary>
        /// If there is a child key-value with the specified key, returns it.  Attempting to convert any variables using <see cref="ScriptedVariablesAccessor"/>
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <returns>See above.</returns>
        public string GetKeyValue(string key) {
            var value = GetRawKeyValue(key);
            return ScriptedVariablesAccessor.GetPotentialValue(value);
        }

        /// <summary>
        /// If there is a child key-value with the specified key, returns it. Does not perform variable conversion.
        /// </summary>
        /// <remarks>
        /// Use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </remarks>
        /// <param name="key">The Key of the Keyvalue item within the node</param>
        /// <returns>See above.</returns>
        public string GetRawKeyValue(string key) {
            return KeyValues.FirstOrDefault(x => x.Key == key)?.Value;
        }
    }
    
    /// <summary>
    /// A straight key = value entry in the file/node: e.g: tier = 1
    /// </summary>
    public class CWKeyValue 
    {
        public string Key { get; set; }
        public string Value { get; set; }
        
        public KeyValuePair<string, string> ToKeyValue() { return new KeyValuePair<string, string>(Key, Value);}
    }

    
}
