using System;
using System.Collections.Generic;
using System.Text;
using CWToolsHelpers.Directories;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TechTreeCreator;
using TechTreeCreator.Logger;

namespace TechTreeConsole {
    class Program {
        private const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";
        private const string STELLARIS_ROOT_MAC = "/Users/christian/Library/Application Support/Steam/steamapps/common/Stellaris";
        
        private const string OUTPUT_WINDOWS = "C:/Users/Draconas/source/repos/stellaris-tech-tree/www";
        private const string OUTPUT_MAC = "/Users/christian/dev/graph/stellaris-tech-tree/www";
        
        
        static void Main(string[] args) {
            string rootDir = STELLARIS_ROOT_WINDOWS;
            string outputDir = OUTPUT_WINDOWS;
            if (args.Length > 0 && args[0].Equals("mac", StringComparison.InvariantCultureIgnoreCase)) {
                rootDir = STELLARIS_ROOT_MAC;
                outputDir = OUTPUT_MAC;
            }
            
            var outputTemplate = "[{Timestamp:HH:mm:ss} {Level}]{ShortCaller}: {Message}{NewLine}{Exception}"; 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithCaller()
                .WriteTo.Console(LogEventLevel.Information, outputTemplate, theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            IList<ModDirectoryHelper.ModFile> modFiles = ModDirectoryHelper.GetMods(@"/Users/christian/Documents/Paradox Interactive/Stellaris/");
            foreach (var modFile in modFiles) {
                Log.Logger.Information("Mod {name} is defined in {modFile} with data located in {location}", modFile.Name, modFile.ModDefinitionFilePath, modFile.ModDirectoryPath ?? modFile.ArchiveFilePath);
                
            }


//            var techTreeCreatorManager = new TechTreeCreatorManager(rootDir, outputDir);
//            var techsAndDependencies = techTreeCreatorManager.ParseStellarisFiles();
//            techTreeCreatorManager.CopyImages(techsAndDependencies);
//            var techsData = techTreeCreatorManager.GenerateJsGraph(techsAndDependencies);
//
//            var objectsDependantOnTechs = techTreeCreatorManager.ParseObjectsDependantOnTechs(techsAndDependencies, techsData);
//
//            foreach (var (modGoup, visData) in techsData) {
//                Log.Logger.Information("{modGroup} Techs: {techCount} Edges: {edgeCount}",modGoup, visData.nodes.Count, visData.edges.Count);
//            }
//            
//            foreach (var (modGoup, visData) in objectsDependantOnTechs) {
//                Log.Logger.Information("{modGroup} Buildings: {techCount} Edges: {edgeCount}",modGoup, visData.nodes.Count, visData.edges.Count);
//            }
        }
    }
}