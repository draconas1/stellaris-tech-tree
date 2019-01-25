using System;
using System.Collections.Generic;
using System.Linq;
using CWTools.Process;

namespace TechTree.DTO
{
    // These were all taken from the example CSTests for CWTools modified by me to be easier to work with.
    public class CWKeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class CWNode
    {
        public string Key { get; set; }
        // would really like to use a dictionary here, but duplicate node keys are entirely possible, 
        // as keys are oftne things like logical operators
        public List<CWNode> Nodes { get; set; }
        // ditto here
        public List<CWKeyValue> KeyValues { get; set; }

        public List<string> Values { get; set; }

        /// <summary>
        /// use with caution, will get the first node if there are multiple with the same key in the same context!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CWNode GetNode(string key)
        {
            return Nodes.FirstOrDefault(x => x.Key == key);
        }

        
        /// <summary>
        /// If there is a node with the given key, performs the specified Action on it.
        /// use with caution, will get the first node if there are multiple with the same key in the same context!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="perform"></param>
        public void ActOnNode(string key, Action<CWNode> perform) {
            var node = GetNode(key);
            if (node != null) {
                perform(node);
            }
        }

        /// <summary>
        /// use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            return KeyValues.FirstOrDefault(x => x.Key == key)?.Value;
        }

        /// <summary>
        /// Gets the first key value as <see cref="GetKeyValue(string)"/>, if that value exists and is a variable in the variables dictionary returns the variable value, otherwise returns the keyvalue.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public string GetKeyValue(string key, IDictionary<string, string> variables)
        {
            var value = KeyValues.FirstOrDefault(x => x.Key == key)?.Value;
            if (value != null && value.StartsWith("@") && variables.ContainsKey(value))
            {
                return variables[value];
            }
            return value;
        }
    }

    public static class CWParsedFileMapper
    {
        public static CWNode ToMyNode(Node n)
        {
            var nodes = n.AllChildren.Where(x => x.IsNodeC).Select(x => ToMyNode(x.node)).ToList();
            var leaves = n.AllChildren.Where(x => x.IsLeafC).Select(x => ToMyKeyValue(x.leaf)).ToList();
            var values = n.AllChildren.Where(x => x.IsLeafValueC).Select(x => x.lefavalue.Key).ToList();
            return new CWNode { Key = n.Key, Nodes = nodes, Values = values, KeyValues = leaves };
        }

        private static CWKeyValue ToMyKeyValue(Leaf l)
        {
            return new CWKeyValue { Key = l.Key, Value = l.Value.ToRawString() };
        }

        // keeping this here as it works for my purposes
       
    }
}
