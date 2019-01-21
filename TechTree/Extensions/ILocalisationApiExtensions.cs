using CWTools.Localisation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TechTree.Extensions
{
    /// <summary>
    /// Extension methods for working with ILocalisationAPI
    /// </summary>
    public static class ILocalisationApiExtensions {
        private static readonly Regex VariableRegex = new Regex(@"(?<var>\$(?<varname>.+?)\$)");
        /// <summary>
        /// Get the primary text for a key, it's main name.  
        /// </summary>
        public static string GetName(this ILocalisationAPI api, string key)
        {
            return getValue(api, key);
        }

        /// <summary>
        /// Get the long description for a key - explanation text, popup info etc...  
        /// </summary>
        public static string GetDescription(this ILocalisationAPI api, string key)
        {
            return getValue(api, key + "_desc");
        }

        private static string getValue(ILocalisationAPI api, string key)
        {
            string result = api.Values[key];

            // first strip out comments
            if (result.IndexOf("#") > 0)
            {
                result = result.Substring(0, result.IndexOf("#") - 1);
                result = result.Trim();
            }

            // follow variables in the localisation text, they are denoted by $varName$ 
            Match match = VariableRegex.Match(result);
            while (match.Success) {
                // get the name of the variable to find in localisation
                var variableName = match.Groups["varname"].Value;
                // go find the value of that variable, note recurision
                string replacement = getValue(api, variableName);
                // Replace is equivalent of javas ReplaceAll
                result = result.Replace(match.Groups["var"].Value, replacement);
                // look for other variables
                match = VariableRegex.Match(result);
            }

            // remove the surrounding quotation marks
            result = removeQuotes(result);
            result = result.Trim();
            return result;
        }


        private static string removeQuotes(string data) {
            if (data.StartsWith("\"")) {
                data = data.Substring(1, data.Length - 1);
            }
            if (data.EndsWith("\""))
            {
                data = data.Substring(0, data.Length - 1);
            }

            return data;
        }
    }
}
