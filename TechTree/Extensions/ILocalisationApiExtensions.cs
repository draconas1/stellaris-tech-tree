using CWTools.Localisation;
using System;
using System.Collections.Generic;
using System.Text;

namespace TechTree.Extensions
{
    /// <summary>
    /// Extension methods for working with ILocalisationAPI
    /// </summary>
    public static class ILocalisationApiExtensions
    {
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
