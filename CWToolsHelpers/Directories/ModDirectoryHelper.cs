using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CWToolsHelpers.FileParsing;

namespace CWToolsHelpers.Directories {
    public static class ModDirectoryHelper {
        
        public static bool isSteamWorkshopModDirectory(string path) {
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                return false;
            }
            var directoryInfo = new DirectoryInfo(path);
            var zipFiles = directoryInfo.GetFiles("*.zip");
            if (!zipFiles.Any()) {
                return false;
            }
            var files = directoryInfo.GetFiles();
            if (zipFiles.Length == files.Length) {
                return true;
            }

            return false;
        }
        
        public static bool isArchiveFile(string path) {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)) {
                return false;
            }

            var fileInfo = new FileInfo(path);
            return fileInfo.Extension.ToLowerInvariant() == ".zip";
        }

        public class ModFile {
            private readonly string stellarisDataPath;
            private readonly CWNode node;
            public string ModDefinitionFilePath { get; }
            public string Name => node.GetKeyValue("name");
            public string ArchiveFilePath => node.GetKeyValue("archive");
            public string ModDirectoryPath => node.GetKeyValue("path") != null ? Path.Combine(stellarisDataPath, node.GetKeyValue("path")) : null;
            //public IList<string> Tags => node.GetNode("tags")?.Values;

            internal ModFile(string modDefinitionFilePath, string stellarisDataPath, CWNode node) {
                ModDefinitionFilePath = modDefinitionFilePath;
                this.stellarisDataPath = stellarisDataPath;
                this.node = node;
            }
        }
        
        
        public static IList<ModFile> GetMods(string stellarisUserDirectory) {
            var directoryInfo = new DirectoryInfo(Path.Combine(stellarisUserDirectory, "mod"));
            var modfiles = directoryInfo.GetFiles("*.mod");
            var cwParserHelper = new CWParserHelper();
            var modFiles = cwParserHelper.ParseParadoxFiles(modfiles.Select(x => x.FullName));
            return modFiles.Select(x => new ModFile(x.Key, stellarisUserDirectory, x.Value)).ToList();

//            foreach (var fileInfo in modfiles) {
//                var allLines = File.ReadAllLines(fileInfo.FullName);
//                string name = null;
//                string archivePath = null;
//                string directoryPath = null;
//                foreach (var line in allLines) {
//                    var keyValue = line.Split("=");
//                    
//                }
//            }
        }
        
        
        

        public static StellarisDirectoryHelper CreateDirectoryHelper(string path, string modGroup = null, bool forceOverride = false) {
            if (!isArchiveFile(path) && isSteamWorkshopModDirectory(path)) {
                return new StellarisDirectoryHelper(path, modGroup);
            }

            string tempDirectoryName;
            FileInfo zipInfo;
            if (isArchiveFile(path)) {
                zipInfo = new FileInfo(path);
                tempDirectoryName = zipInfo.Directory.Name;
            }
            else {
                var directoryInfo = new DirectoryInfo(path);
                var zipFiles = directoryInfo.GetFiles("*.zip");
                if (zipFiles.Length > 1) {
                    throw new Exception("Path " + path + " was determined to be a steam workshop file, but contained multiple zip files " + zipFiles.Select(x => x.Name));
                }

                zipInfo = zipFiles.First();
                tempDirectoryName = directoryInfo.Name;
            }
           
            var tempFolder = Path.Combine(Path.GetTempPath(), tempDirectoryName, zipInfo.Name);
            if (forceOverride && Directory.Exists(tempFolder)) {
                Directory.Delete(tempFolder, true);
            }
            
            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(zipInfo.FullName, tempFolder);                
            
            return new StellarisDirectoryHelper(tempFolder);
        }
    }
}