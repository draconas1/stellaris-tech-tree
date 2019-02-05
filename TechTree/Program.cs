using System;
using System.IO;
using System.Linq;
using System.Text;
using CWTools.Common;
using CWTools.Localisation;
using TechTree.CWParser;
using TechTree.DTO;
using TechTree.FileIO;
using TechTree.Output;

namespace TechTree
{
    class Program {
        public const bool MAC = true;
        
        public const string STELLARIS_ROOT_WINDOWS = "C:/Games/SteamLibrary/steamapps/common/Stellaris";
        public const string STELLARIS_ROOT_MAC = "/Users/christian/Library/Application Support/Steam/steamapps/common/Stellaris";
        public const string ROOT_IN_USE = MAC ? STELLARIS_ROOT_MAC : STELLARIS_ROOT_WINDOWS;
        
        
        public const string OUTPUT_WINDOWS = "C:/Users/Draconas/source/repos/stellaris-tech-tree/www";
        public const string OUTPUT_MAC = "/Users/christian/dev/graph/stellaris-tech-tree/www";
        public const string OUTPUT_IN_USE = MAC ? OUTPUT_MAC : OUTPUT_WINDOWS;
        static void Main(string[] args)
        {
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //setup localisation 
            ILocalisationAPI localisation = Localisation.GetLocalisationAPI(ROOT_IN_USE, STLLang.English);

            // setup parser
            var dirHelper = new StellarisDirectoryHelper(ROOT_IN_USE);
            var scriptedVariablesHelper = new ScriptedVariableParser(dirHelper.ScriptedVariables);
            var parser = new TechTreeParser(localisation, scriptedVariablesHelper.ParseScriptedVariables(), dirHelper.Technology);
            parser.IgnoreFiles.AddRange(new string[] { "00_tier.txt", "00_category.txt" });
           // parser.Areas.Add("physics");
            //parser.ParseFileMask = "00_eng_tech.txt";
           
            // get the results parsed into nice tech tree format
            var model = parser.ParseTechFiles();

            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateVisData(model, "images/technologies");

            //save
            ImageOutput.transformAndOutputImages(Path.Combine(dirHelper.Icons, "technologies"), Path.Combine(OUTPUT_IN_USE, "images", "technologies"), model.Techs.Values);

            var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
            foreach (var techArea in techAreas)
            {
                ImageOutput.transformAndOutputImage(
                    Path.Combine(dirHelper.Icons, "resources", techArea.ToString().ToLowerInvariant() + "_research.dds"), 
                    Path.Combine(OUTPUT_IN_USE, "images", "technologies", techArea + "-root.png"));
            }

            // update model with image status
            visResults.nodes.ForEach(node => {
                    if (model.Techs.ContainsKey(node.id))
                    {
                        node.hasImage = model.Techs[node.id].IconFound;
                    }
                });
            visResults.WriteVisDataToOneJSFile(OUTPUT_IN_USE);

            Console.WriteLine("done.  Nodes: " + visResults.nodes.Count() + " Edges: " + visResults.edges.Count());
        }
    }
}
