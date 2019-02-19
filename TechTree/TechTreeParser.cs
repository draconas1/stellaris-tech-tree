using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CWTools.Common;
using CWToolsHelpers;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using CWToolsHelpers.ScriptedVariables;
using TechTree.CWParser;
using TechTree.DTO;
using TechTree.Output;

namespace TechTree
{
    internal static class Program {
        private const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";
        private const string STELLARIS_ROOT_MAC = "/Users/christian/Library/Application Support/Steam/steamapps/common/Stellaris";
        
        private const string OUTPUT_WINDOWS = "C:/Users/Draconas/source/repos/stellaris-tech-tree/www";
        private const string OUTPUT_MAC = "/Users/christian/dev/graph/stellaris-tech-tree/www";
        static void Main(string[] args)
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string rootDir = STELLARIS_ROOT_WINDOWS;
            string outputDir = OUTPUT_WINDOWS;
            if (args.Length > 0 && args[0].Equals("mac", StringComparison.InvariantCultureIgnoreCase)) {
                rootDir = STELLARIS_ROOT_MAC;
                outputDir = OUTPUT_MAC;
            }
            
            // setup parser
            var dirHelper = new StellarisDirectoryHelper(rootDir);
            var scriptedVariablesHelper = new ScriptedVariableAccessor(dirHelper);

            
            //setup localisation 
            // Include extra pie! Especially for Piebadger.
            var localisation = new LocalisationApiHelper(dirHelper, STLLang.English);
            var cwParser = new CWParserHelper(scriptedVariablesHelper);
            
            
            var parser = new TechTreeParser(localisation, cwParser, dirHelper, new StellarisDirectoryHelper[]{});
           
            // get the results parsed into nice tech tree format
            var model = parser.ParseTechFiles();

            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateVisData(model, "images/technologies");

            //save
            ImageOutput.TransformAndOutputImages(Path.Combine(dirHelper.Icons, "technologies"), Path.Combine(outputDir, "images", "technologies"), model.Techs.Values);

            var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
            var areaDir = Path.Combine(outputDir, "images", "technologies", "areas");
            Directory.CreateDirectory(areaDir);
            foreach (var techArea in techAreas) {
                var inputPath = Path.Combine(dirHelper.Icons, "resources", techArea.ToString().ToLowerInvariant() + "_research.dds");
                ImageOutput.TransformAndOutputImage(
                    inputPath, 
                    Path.Combine(outputDir, "images", "technologies", techArea + "-root.png"));
                
                ImageOutput.TransformAndOutputImage(
                    inputPath, 
                    Path.Combine(outputDir, "images", "technologies", "areas", techArea.ToString().ToLowerInvariant() + ".png"));
            }

            var categoryDir = Path.Combine(outputDir, "images", "technologies", "categories");
            Directory.CreateDirectory(categoryDir);
            TechCategoryImageOutput.OutputCategoryImages(dirHelper.Root, categoryDir);

            // update model with image status
            visResults.nodes.ForEach(node => {
                    if (model.Techs.ContainsKey(node.id))
                    {
                        node.hasImage = model.Techs[node.id].IconFound;
                    }
                });
            visResults.WriteVisDataToOneJSFile(outputDir);

            Console.WriteLine("Done.  Nodes: " + visResults.nodes.Count() + " Edges: " + visResults.edges.Count());
        }
    }
}
