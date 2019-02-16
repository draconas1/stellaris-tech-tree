﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CWTools.Common;
using CWTools.Localisation;
using CWToolsHelpers.Directories;
using NetExtensions.Collection;

namespace CWToolsHelpers {
    /// <summary>
    /// Helper for the most common operations working with <see cref="ILocalisationAPI"/> with potentially mods involved.
    /// </summary>
    public class LocalisationApiHelper {
        private static readonly Regex VariableRegex = new Regex(@"(?<var>\$(?<varname>.+?)\$)");
        
        private readonly IDictionary<string, string> localisation;

        public LocalisationApiHelper(StellarisDirectoryHelper stellarisDirectoryHelper, STLLang language) : this(stellarisDirectoryHelper,
            new StellarisDirectoryHelper[] { }, language) {
        }
        
        public LocalisationApiHelper(StellarisDirectoryHelper stellarisDirectoryHelper,
            IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers,
            STLLang language) {
            //When using the ILocalisationAp for a specific language, the key thing being used is the Values property,
            //which is a raw dictionary of keys to text values.
            var coreLocalisationService =
                new STLLocalisation.STLLocalisationService(new LocalisationSettings(stellarisDirectoryHelper.Localisation));
            var coreLocalisation = coreLocalisationService.Api(Lang.NewSTL(language)).Values;
            foreach (var modDirectoryHelper in modDirectoryHelpers) {
                var modLocalisationService =
                    new STLLocalisation.STLLocalisationService(new LocalisationSettings(modDirectoryHelper.Localisation));
                var modLocalisation = modLocalisationService.Api(Lang.NewSTL(language)).Values;
                coreLocalisation.PutAll(modLocalisation);
            }

            localisation = coreLocalisation;
        }
        
        /// <summary>
        /// Get the primary text for a key, it's main name. 
        /// </summary>
        /// <remarks>
        /// This can also be used to get an arbitrary value by adjusting the key.
        /// </remarks>
        public string GetName(string key)
        {
            return GetValue(key);
        }

        /// <summary>
        /// Get the long description for a key - explanation text, popup info etc...  
        /// </summary>
        /// <remarks>
        /// Gets <c>key + "_desc"</c>
        /// </remarks>
        public string GetDescription(string key)
        {
            return GetValue(key + "_desc");
        }

        private string GetValue(string key)
        {
            // if no localised text, return the key
            if (!localisation.ContainsKey(key))
            {
                return key;
            }
            string result = localisation[key];

            // first strip out comments
            if (result.IndexOf("#", StringComparison.Ordinal) > 0)
            {
                result = result.Substring(0, result.IndexOf("#", StringComparison.Ordinal) - 1);
                result = result.Trim();
            }

            // follow variables in the localisation text, they are denoted by $varName$ 
            Match match = VariableRegex.Match(result);
            while (match.Success) {
                // get the name of the variable to find in localisation
                var variableName = match.Groups["varname"].Value;
                // go find the value of that variable, note recurision
                string replacement = GetValue(variableName);
                // Replace is equivalent of javas ReplaceAll
                result = result.Replace(match.Groups["var"].Value, replacement);
                // look for other variables
                match = VariableRegex.Match(result);
            }

            // remove the surrounding quotation marks
            result = RemoveQuotes(result);
            result = result.Trim();
            return result;
        }


        private static string RemoveQuotes(string data) {
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
