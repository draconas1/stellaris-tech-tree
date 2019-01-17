using CWTools.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// use with caution, will get the first keyvalue if there are multiple with the same key in the same context!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            return KeyValues.FirstOrDefault(x => x.Key == key)?.Value;
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
