using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CWToolsHelpers.FileParsing;
using Ionic.Zip;
using NetExtensions.Object;
using Newtonsoft.Json;

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

        public class Mod {
            public string Name { get; set; }
            public string NameRaw { get; set; }
            public string ModGroup { get; set; }
            public string ArchiveFilePath { get; set; }
            public string ModDirectoryPath { get; set; }

            public bool Include { get; set; }
        }
        
        public static void WriteModListFile(string filePath, IEnumerable<ModFile> modFiles) {
            var list = modFiles.Select(x => new Mod() {
                Name = string.Join("_", x.Name.Split(Path.GetInvalidFileNameChars())),
                NameRaw = x.Name,
                ArchiveFilePath = x.ArchiveFilePath,
                ModDirectoryPath = x.ModDirectoryPath
            }).ToList();
            list.Sort(IComparerExtensions.Create<Mod>(x => x.Name));
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(filePath)))
            {
                serializer.Serialize(writer, list);
            }
        }

        public static List<Mod> ReadModListFromFile(string filePath) {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader file = File.OpenText(filePath))
            {
                return (List<Mod>)serializer.Deserialize(file, typeof(List<Mod>));
            }
        }

        public static List<StellarisDirectoryHelper> CreateDirectoryHelpers(IEnumerable<Mod> mods, bool forceOverride = false) {
            return mods.Where(x => x.Include).Select(x => CreateDirectoryHelper(x.ArchiveFilePath ?? x.ModDirectoryPath, x.Name, x.ModGroup ?? x.Name, forceOverride)).ToList();
        }

        public static StellarisDirectoryHelper CreateDirectoryHelper(string path, string modName, string modGroup = null, bool forceOverride = true) {
            if (!isArchiveFile(path) && !isSteamWorkshopModDirectory(path)) {
                return new StellarisDirectoryHelper(path, modGroup);
            }

            FileInfo zipInfo;
            string workshopNumber;
            if (isArchiveFile(path)) {
                zipInfo = new FileInfo(path);
                workshopNumber = zipInfo.Directory.Name;
            }
            else {
                var directoryInfo = new DirectoryInfo(path);
                var zipFiles = directoryInfo.GetFiles("*.zip");
                if (zipFiles.Length > 1) {
                    throw new Exception("Path " + path + " was determined to be a steam workshop file, but contained multiple zip files " + zipFiles.Select(x => x.Name));
                }

                zipInfo = zipFiles.First();
                workshopNumber = directoryInfo.Name;
            }
           
            var tempFolder = Path.Combine(Path.GetTempPath(), workshopNumber, modName);
            if (forceOverride && Directory.Exists(tempFolder)) {
                Directory.Delete(tempFolder, true);
            }
            
            Directory.CreateDirectory(tempFolder);

            try {
                Ionic.Zip.ZipFile.Read(zipInfo.FullName).ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (Exception e) {
                throw new Exception("Unable to process: " + zipInfo.FullName, e);
            }

            return new StellarisDirectoryHelper(tempFolder);
        }
        
    }
}