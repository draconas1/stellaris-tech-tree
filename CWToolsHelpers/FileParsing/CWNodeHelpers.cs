using System;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.FileParsing;

namespace CWToolsHelpers.Helpers {
    public static class CWNodeHelpers {

        public static bool? ResolveBooleanValue(string value) {
            if (value.Equals("yes", StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }
            
            if (value.Equals("no", StringComparison.InvariantCultureIgnoreCase)) {
                return false;
            }

            return null;
        }
        
        public static bool ResolveBoolean(bool startingValue, CWNode node) {
            bool endingValue;
            switch (node.Key.ToLowerInvariant()) {
                case "and":
                case "or":
                    endingValue = startingValue;
                    break;
                case "nor":
                case "not":
                    endingValue = !startingValue;
                    break;
                default: endingValue = startingValue;
                    break;
            }

            if (node.Parent != null) {
                return ResolveBoolean(endingValue, node.Parent);
            }

            return endingValue;
        }

        public static CWNode SearchNodes(this IEnumerable<CWNode> nodes, NodeSearchCriteria nodeSearchCriteria) {
            foreach (var cwNode in nodes) {
                var searchNodeResult = SearchNodes(cwNode, nodeSearchCriteria);
                if (searchNodeResult != null) {
                    return searchNodeResult;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds a node in this CWNode and all of its child CWNodes that matches the specified <see cref="NodeSearchCriteria"/>.  It will return the first Node that matches, and what consitutes "first" is undetermined.
        /// </summary>
        /// <param name="nodeSearchCriteria">The criteria.  Null fields are ignored in searching.  If multiple criteria are specified then the match returns the first node that matches any of them</param>
        /// <returns>The found node, or <c>null</c> if none could be found</returns>
        public static CWNode SearchNodes(this CWNode node, NodeSearchCriteria nodeSearchCriteria) {
            // test if our node matches the criteria
            if (nodeSearchCriteria.NodeKey != null) {
                if (node.Key.Equals(nodeSearchCriteria.NodeKey, StringComparison.InvariantCultureIgnoreCase)) {
                    return node;
                }
            }

            if (nodeSearchCriteria.Value != null) {
                if (node.Values.Contains(nodeSearchCriteria.Value, StringComparer.InvariantCultureIgnoreCase)) {
                    return node;
                }
            }

            if (nodeSearchCriteria.KeyValue != null) {
                var criteriaKeyValue = nodeSearchCriteria.KeyValue.Value;
                if (criteriaKeyValue.Key == null) {
                    if (nodeSearchCriteria.SearchForKVPAgainstRawValues) {
                        if (node.RawKeyValues.Any(kvp => kvp.Value.Equals(criteriaKeyValue.Value, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }

                    if (nodeSearchCriteria.SearchForKVPAgainstSubstitutedValues) {
                        if (node.KeyValues.Any(kvp => kvp.Value.Equals(criteriaKeyValue.Value, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }
                }
                else if (criteriaKeyValue.Value == null) {
                    if (nodeSearchCriteria.SearchForKVPAgainstRawValues) {
                        if (node.RawKeyValues.Any(kvp => kvp.Key.Equals(criteriaKeyValue.Key, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }

                    if (nodeSearchCriteria.SearchForKVPAgainstSubstitutedValues) {
                        if (node.KeyValues.Any(kvp => kvp.Key.Equals(criteriaKeyValue.Key, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }
                }
                else {
                    if (nodeSearchCriteria.SearchForKVPAgainstRawValues) {
                        if (node.RawKeyValues.Any(kvp => kvp.Equals(criteriaKeyValue, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }

                    if (nodeSearchCriteria.SearchForKVPAgainstSubstitutedValues) {
                        if (node.KeyValues.Cast<CWNodeContextedKeyValue>().Any(kvp => kvp.Equals(criteriaKeyValue, StringComparison.InvariantCultureIgnoreCase))) {
                            return node;
                        }
                    }
                }
            }

            // otherwise DFS down the children
            return node.Nodes.Select(cwNode => cwNode.SearchNodes(nodeSearchCriteria)).FirstOrDefault(childResult => childResult != null);
        }
    }
    
    /// <summary>
    /// Criteria to search for a CWNode
    /// </summary>
    public class NodeSearchCriteria {
        /// <summary>
        /// The Node key that must match.   Checked case-insensitively.
        /// </summary>
        public string NodeKey { get; set; }
        
        /// <summary>
        /// A keyvalue pair that the node must contain.  This KVP can either be just the value (where any KVP that has that value satisfies), just the key (where any KVP that has that key satisfies) or both (KVP must match both key and value).  The comparison is case-insensitive.
        /// </summary>
        /// <remarks>
        /// These are checked against the raw values by default, this is controlled with <see cref="SearchForKVPAgainstRawValues"/> and <see cref="SearchForKVPAgainstSubstitutedValues"/>
        /// </remarks>
        public KeyValuePair<string, string>? KeyValue { get; set; }
        
        /// <summary>
        /// KVP searches should be against raw values.  This is <c>true</c> by default.
        /// </summary>
        public bool SearchForKVPAgainstRawValues { get; set; }
        
        /// <summary>
        /// KVP searches should be against substituted values.  This is <c>false</c> by default.
        /// </summary>
        public bool SearchForKVPAgainstSubstitutedValues { get; set; }
        
        /// <summary>
        /// A value that the node must contain.  Checked case-insensitively.
        /// </summary>
        public string Value { get; set; }

        public NodeSearchCriteria() {
            SearchForKVPAgainstRawValues = true;
            SearchForKVPAgainstSubstitutedValues = false;
        }
    }
}