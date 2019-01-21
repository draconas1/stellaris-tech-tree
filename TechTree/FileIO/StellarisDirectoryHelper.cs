using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TechTree.FileIO
{
    /// <summary>
    /// Set of helpers for navigating to specific stellaris/mod directories.
    /// </summary>
    public class StellarisDirectoryHelper
    {
        public string Root { get; private set; }
        public string Common
        {
            get
            {
                return GetCommonDirectory(Root);
            }
        }

        public string Technology
        {
            get
            {
                return GetTechnologyDirectory(Root);
            }
        }

        public string ScriptedVariables
        {
            get
            {
                return GetScriptedVariablesDirectory(Root);
            }
        }   

        public StellarisDirectoryHelper(string rootDirectory)
        {
            Root = rootDirectory;
        }


        public static string GetCommonDirectory(string rootDirectory)
        {
            return Path.Combine(rootDirectory, "common");
        }

        public static string GetTechnologyDirectory(string rootDirectory)
        {
            return Path.Combine(GetCommonDirectory(rootDirectory), "technology");
        }

        public static string GetLocalisationDirectory(string rootDirectory)
        {
            return Path.Combine(rootDirectory, "localisation");
        }

        public static string GetScriptedVariablesDirectory(string rootDirectory)
        {
            return Path.Combine(GetCommonDirectory(rootDirectory), "scripted_variables");
        }
    }
}
