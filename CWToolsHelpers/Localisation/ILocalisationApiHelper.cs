using System;
using CWTools.Localisation;

namespace CWToolsHelpers.Localisation {
    /// <summary>
    /// Helper for working with the <see cref="ILocalisationAPI"/>
    /// </summary>
    public interface ILocalisationApiHelper {
        /// <summary>
        /// Get the primary text for a key, it's main name. 
        /// </summary>
        /// <remarks>
        /// This can also be used to get an arbitrary value by adjusting the key.
        /// </remarks>
        string GetName(string key);

        /// <summary>
        /// Returns <c>true</c> If there is a value for the given key.
        /// </summary>
        bool HasValueForKey(string key);

        /// <summary>
        /// Get the long description for a key - explanation text, popup info etc...  
        /// </summary>
        /// <remarks>
        /// Gets <c>key + "_desc"</c>
        /// </remarks>
        string GetDescription(string key);
    }
}