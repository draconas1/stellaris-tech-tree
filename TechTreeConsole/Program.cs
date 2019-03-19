using System;
using TechTreeCreator;

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

            var techTreeCreatorManager = new TechTreeCreatorManager(rootDir, outputDir);
            var techsAndDependencies = techTreeCreatorManager.ParseStellarisFiles();
            techTreeCreatorManager.CopyImages(techsAndDependencies);
            var techsData = techTreeCreatorManager.GenerateJsGraph(techsAndDependencies);

            var objectsDependantOnTechs = techTreeCreatorManager.ParseObjectsDependantOnTechs(techsAndDependencies, techsData);

            foreach (var (modGoup, visData) in techsData) {
                Console.WriteLine(modGoup + " Techs: " + visData.nodes.Count + " Edges: " + visData.edges.Count);
            }
            
            foreach (var (modGoup, visData) in objectsDependantOnTechs) {
                Console.WriteLine(modGoup + " Buildings: " + visData.nodes.Count + " Edges: " + visData.edges.Count);
            }
        }
    }
}