using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CWTools.Common;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using CWToolsHelpers.ScriptedVariables;
using NetExtensions.Collection;
using TechTreeCreator.DTO;
using TechTreeCreator.GraphCreation;
using TechTreeCreator.Output;
using static CWToolsHelpers.Directories.StellarisDirectoryHelper.StellarisDirectoryPositionModList;

namespace TechTreeCreator
{
    /// <summary>
    /// Main control class for doing the parsing and output
    /// </summary>
    public class TechTreeCreatorManager {
        private readonly string outputRoot;
        private readonly StellarisDirectoryHelper stellarisDirectoryHelper;
        private readonly ILocalisationApiHelper localisation;
        private readonly ICWParserHelper cwParser;
        private readonly IList<StellarisDirectoryHelper> modDirectoryHelpers;
        
        public TechTreeCreatorManager(string stellarisRoot, string outputRoot, IEnumerable<string> modRoots = null) {
            this.outputRoot = outputRoot;
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            
            // setup parser
            stellarisDirectoryHelper = new StellarisDirectoryHelper(stellarisRoot);
            modDirectoryHelpers = modRoots.NullToEmpty().Select(x => new StellarisDirectoryHelper(x)).ToList();
            
            //setup localisation 
            // Include extra pie! Especially for Piebadger.
            localisation = new LocalisationApiHelper(stellarisDirectoryHelper, modDirectoryHelpers, STLLang.English);
            
            // setup parser
            IScriptedVariablesAccessor scriptedVariablesHelper = new ScriptedVariableAccessor(stellarisDirectoryHelper, modDirectoryHelpers);
            cwParser = new CWParserHelper(scriptedVariablesHelper);
        }
        
        public TechsAndDependencies ParseStellarisFiles() {
            var parser = new TechTreeGraphCreator(localisation, cwParser, stellarisDirectoryHelper, modDirectoryHelpers);
            return parser.CreateTechnologyGraph();
        }

        public void CopyImages(TechsAndDependencies techsAndDependencies) {
            CopyMainImages(techsAndDependencies.Techs.Values, "technologies", Path.Combine("images", "technologies"));

            // the 3 tech area icons no mod support here
            var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
            var areaDir = Path.Combine(outputRoot, "images", "technologies", "areas");
            Directory.CreateDirectory(areaDir);
            foreach (var techArea in techAreas) {
                var inputPath = Path.Combine(stellarisDirectoryHelper.Icons, "resources", techArea.ToString().ToLowerInvariant() + "_research.dds");
                ImageOutput.TransformAndOutputImage(
                    inputPath, 
                    Path.Combine(outputRoot, "images", "technologies", techArea + "-root.png"));
                
                ImageOutput.TransformAndOutputImage(
                    inputPath, 
                    Path.Combine(outputRoot, "images", "technologies", "areas", techArea.ToString().ToLowerInvariant() + ".png"));
            }

            // tech category images no mod support here
            var categoryDir = Path.Combine(outputRoot, "images", "technologies", "categories");
            Directory.CreateDirectory(categoryDir);
            TechCategoryImageOutput.OutputCategoryImages(stellarisDirectoryHelper.Root, categoryDir);
        }

        public VisData GenerateJsGraph(TechsAndDependencies techsAndDependencies) {
            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateVisData(techsAndDependencies, Path.Combine("images", "technologies"));
            visResults.WriteVisDataToOneJSFile(outputRoot);

            return visResults;
        }
        
        public VisData ParseObjectsDependantOnTechs(TechsAndDependencies techsAndDependencies, VisData techVisData) {
            var parser = new DependantsGraphCreator(localisation, cwParser, stellarisDirectoryHelper, modDirectoryHelpers);
            var dependantGraph = parser.CreateDependantGraph(techsAndDependencies);
            CopyMainImages(dependantGraph.Buildings.Values, "buildings", Path.Combine("images", "buildings"));
            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateVisData(techVisData, dependantGraph, Path.Combine("images", "buildings"));
            visResults.WriteVisDataToOneJSFile(outputRoot, "buildingGraph.js");

            return visResults;
        }

        public void CopyMainImages(IEnumerable<Entity> entities, string inputPath, string outputPath) {
            //build the mod input list
            var inputDirectories = StellarisDirectoryHelper
                .CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers, Last)
                .Select(x => Path.Combine(x.Icons, inputPath)).ToList();

            // tech images
            ImageOutput.TransformAndOutputImages(inputDirectories, Path.Combine(outputRoot, outputPath), entities);
        }
        
    }
}
