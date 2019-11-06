using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CWToolsHelpers.Directories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TechTreeCreator;
using TechTreeCreator.DTO;
using TechTreeCreator.Logger;

namespace TechTreeConsole {
    class Program {
        private const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";
        private const string STELLARIS_ROOT_MAC = "/Users/christian/Library/Application Support/Steam/steamapps/common/Stellaris";       

        private const string OUTPUT_WINDOWS = "C:/Users/Draconas/source/repos/stellaris-tech-tree/www";
        private const string OUTPUT_MAC = "/Users/christian/dev/graph/stellaris-tech-tree/www";

        private const string STELLARIS_USER_WINDOWS = @"D:\Users\Draconas\Documents\Paradox Interactive\Stellaris";
        private const string STELLARIS_USER_MAC = "/Users/christian/Documents/Paradox Interactive/Stellaris";

        static void Main(string[] args) {
            try {
                string rootDir = STELLARIS_ROOT_WINDOWS;
                string outputDir = OUTPUT_WINDOWS;
                string modFileSuffix = "-windows";
                string stellarisUserDir = STELLARIS_USER_WINDOWS;
                bool forceOverwrite = true;
                bool createModListFile = false;
                foreach (var arg in args) {
                    if (arg.Equals("mac", StringComparison.InvariantCultureIgnoreCase)) {
                        rootDir = STELLARIS_ROOT_MAC;
                        outputDir = OUTPUT_MAC;
                        stellarisUserDir = STELLARIS_USER_MAC;
                        modFileSuffix = "-mac";
                    }

                    if (arg.Equals("modfile", StringComparison.InvariantCultureIgnoreCase)) {
                        createModListFile = true;
                    }

                    if (arg.Equals("overwrite", StringComparison.InvariantCultureIgnoreCase)) {
                        forceOverwrite = true;
                    }
                }

                string modsFilePath = Path.Combine(outputDir, "mods" + modFileSuffix + ".json");

                var outputTemplate = "[{Timestamp:HH:mm:ss} {Level}]{ShortCaller}: {Message}{NewLine}{Exception}";
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .Enrich.FromLogContext()
                    .Enrich.WithCaller()
                    .WriteTo.Console(LogEventLevel.Debug, outputTemplate, theme: AnsiConsoleTheme.Literate)
                    .CreateLogger();

                if (createModListFile) {
                    IList<ModDefinitionFile> modFiles = ModDirectoryHelper.LoadModDefinitions(stellarisUserDir);
                    ModDirectoryHelper.WriteModInfoFile(modsFilePath, modFiles);
                    Environment.Exit(0);
                }

                var modList = ModDirectoryHelper.ReadModInfoFile(modsFilePath);
                foreach (var modFile in modList.Where(x => x.Include)) {
                    Log.Logger.Information("Mod {name} with data located in {location}", modFile.Name, modFile.ModDirectoryPath ?? modFile.ArchiveFilePath);
                }

                var techTreeCreatorManager = new TechTreeCreatorManager(rootDir, outputDir);
                techTreeCreatorManager.Mods = modList;
                techTreeCreatorManager.ForceModOverwriting = forceOverwrite;
                
                techTreeCreatorManager.Parse(new [] { ParseTarget.Buildings, ParseTarget.ShipComponents, ParseTarget.Decisions});
                //techTreeCreatorManager.Parse(new [] { ParseTarget.ShipComponents});
                
//
//                foreach (var (modGoup, visData) in techsData) {
//                    Log.Logger.Information("{modGroup} Techs: {techCount} Edges: {edgeCount}", modGoup, visData.nodes.Count, visData.edges.Count);
//                }
//
//                foreach (var (modGoup, visData) in objectsDependantOnTechs) {
//                    Log.Logger.Information("{modGroup} Buildings: {techCount} Edges: {edgeCount}", modGoup, visData.nodes.Count, visData.edges.Count);
//                }
            }
            catch (Exception e) {
                Log.Logger.Fatal(e, "Fatal Error");
                //Environment.Exit(1);
            }
            finally{Log.CloseAndFlush();}
            Console.ReadLine();
        }
    }
}