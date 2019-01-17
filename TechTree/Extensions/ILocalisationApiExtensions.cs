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
            return api.Values[key];
        }

        /// <summary>
        /// Get the long description for a key - explanation text, popup info etc...  
        /// </summary>
        public static string GetDescription(this ILocalisationAPI api, string key)
        {
            return api.Values[key + "_desc"];
        }
    }
}
