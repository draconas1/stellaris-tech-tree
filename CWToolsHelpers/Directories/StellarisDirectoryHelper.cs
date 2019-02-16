using System.IO;

namespace CWToolsHelpers.Directories {
    /// <summary>
    /// Set of helpers for navigating to specific stellaris/mod directories.
    /// </summary>
    public class StellarisDirectoryHelper {
        public string Root { get; }

        public string Common => GetCommonDirectory(Root);

        public string Technology => GetTechnologyDirectory(Root);

        public string ScriptedVariables => GetScriptedVariablesDirectory(Root);

        public string Icons => GetIconsDirectory(Root);

        public string Localisation => GetLocalisationDirectory(Root);

        public StellarisDirectoryHelper(string rootDirectory) {
            Root = rootDirectory;
        }


        public static string GetCommonDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "common");
        }

        public static string GetTechnologyDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "technology");
        }

        public static string GetLocalisationDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "localisation");
        }

        public static string GetScriptedVariablesDirectory(string rootDirectory) {
            return Path.Combine(GetCommonDirectory(rootDirectory), "scripted_variables");
        }

        public static string GetIconsDirectory(string rootDirectory) {
            return Path.Combine(rootDirectory, "gfx", "interface", "icons");
        }
    }
}