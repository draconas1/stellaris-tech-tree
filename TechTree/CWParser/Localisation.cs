using CWTools.Common;
using CWTools.Localisation;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using static CWTools.Localisation.STLLocalisation;
using TechTree.FileIO;

namespace TechTree.CWParser
{
    /// <summary>
    /// Helper for creating ILocalisationAPI classes
    /// </summary>
    class Localisation
    {

        /// <summary>
        /// When using the ILocalisationAp for a specific language, the key thing being used is the Values proeprty, which is a raw dictionary of keys to text values.
        /// </summary>
        /// <param name="stellarisRootdirectory"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public static ILocalisationAPI GetLocalisationAPI(string stellarisRootdirectory, STLLang language)
        {
            string localisationPath = StellarisDirectoryHelper.GetLocalisationDirectory(stellarisRootdirectory);
            var localisationService = new STLLocalisationService(new LocalisationSettings(localisationPath));
            ILocalisationAPI api = localisationService.Api(Lang.NewSTL(language));
            return api;
        }
    }
}
