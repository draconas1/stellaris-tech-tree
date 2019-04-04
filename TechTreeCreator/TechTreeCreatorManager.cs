using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CWTools.Common;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using CWToolsHelpers.ScriptedVariables;
using NetExtensions.Collection;
using Serilog;
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
        private StellarisDirectoryHelper StellarisDirectoryHelper { get; }
        private OutputDirectoryHelper OutputDirectoryHelper { get; }
        
        public List<ModInfo> Mods { get; set; }
        public STLLang Language { get; set; }

        private IList<StellarisDirectoryHelper> modDirectoryHelpers;
        private IList<StellarisDirectoryHelper> ModDirectoryHelpers => modDirectoryHelpers ?? (modDirectoryHelpers = ModDirectoryHelper.CreateDirectoryHelpers(Mods.NullToEmpty()));


        private ILocalisationApiHelper localisation;
        private ILocalisationApiHelper Localisation => localisation ?? (localisation = new LocalisationApiHelper(StellarisDirectoryHelper, ModDirectoryHelpers, Language));
        
        private ICWParserHelper cwParser;
        private ICWParserHelper CWParser => cwParser ?? (cwParser = new CWParserHelper(new ScriptedVariableAccessor(StellarisDirectoryHelper, ModDirectoryHelpers)));


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stellarisRoot"></param>
        /// <param name="outputRoot"></param>
        /// <param name="mods">mods</param>
        public TechTreeCreatorManager(string stellarisRoot, string outputRoot, List<ModInfo> mods) {
            this.outputRoot = outputRoot;
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Language = STLLang.English;
            
            OutputDirectoryHelper = new OutputDirectoryHelper(outputRoot);

            
            // setup parser
            StellarisDirectoryHelper = new StellarisDirectoryHelper(stellarisRoot);
            modDirectoryHelpers = ModDirectoryHelper.CreateDirectoryHelpers(mods.NullToEmpty());
            
            //setup localisation 
            // Include extra pie! Especially for Piebadger.
            localisation = new LocalisationApiHelper(StellarisDirectoryHelper, modDirectoryHelpers, STLLang.English);
            
            // setup parser
            IScriptedVariablesAccessor scriptedVariablesHelper = new ScriptedVariableAccessor(StellarisDirectoryHelper, modDirectoryHelpers);
            cwParser = new CWParserHelper(scriptedVariablesHelper);
        }
        
        public void CopyMainImages(IEnumerable<Entity> entities, string inputPath, string outputPath) {
            //build the mod input list
            var inputDirectories = StellarisDirectoryHelper
                .CreateCombinedList(StellarisDirectoryHelper, modDirectoryHelpers, Last)
                .Select(x => Path.Combine(x.Icons, inputPath)).ToList();

            // tech images
            ImageOutput.TransformAndOutputImages(inputDirectories, Path.Combine(outputRoot, outputPath), entities);
        }


        private void CopyCategoryImages(string techOutputDir) {
            // tech category images no mod support here
            var categoryDir = Path.Combine(techOutputDir, "categories");
            Directory.CreateDirectory(categoryDir);
            
            var helpers = StellarisDirectoryHelper
                .CreateCombinedList(StellarisDirectoryHelper, modDirectoryHelpers);

            foreach (var helper in helpers) {
                var categoryFiles =  new[] {helper}.Select(x => Path.Combine(x.Technology, "categories"))
                    .Where(Directory.Exists)
                    .Select(x => DirectoryWalker.FindFilesInDirectoryTree(x, StellarisDirectoryHelper.TextMask))
                    .SelectMany(x => x)
                    .ToList();
                
                var categoryFileNodes = new CWParserHelper().ParseParadoxFiles(categoryFiles.Select(x => x.FullName).ToList());
                foreach (CWNode fileNode in categoryFileNodes.Values) {
                    foreach (var category in fileNode.Nodes) {
                        var catName = category.Key;
                        var imagePath = category.GetKeyValue("icon");

                        ImageOutput.TransformAndOutputImage(Path.Combine(helper.Root, imagePath),
                            Path.Combine(categoryDir, catName + ".png"));
                    };
               
                }
            }
        }

        public void Parse(IEnumerable<ParseTarget> parseTargets) {
            var techTreeGraphCreator = new TechTreeGraphCreator(Localisation, CWParser, StellarisDirectoryHelper, ModDirectoryHelpers);
            ModEntityData<Tech> techData = techTreeGraphCreator.CreateTechnologyGraph();

            var techImageOutputDir = OutputDirectoryHelper.GetImagesPath(ParseTarget.Technologies.ImagesDirectory());
            if (true) {
                CopyMainImages(techData.AllEntities, ParseTarget.Technologies.ImagesDirectory(),
                    techImageOutputDir);

                
                var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
                var areaDir = Path.Combine(techImageOutputDir, "areas"); 
                    
                // tech areas
                Directory.CreateDirectory(areaDir);
                foreach (var techArea in techAreas) {
                    var inputPath = Path.Combine(StellarisDirectoryHelper.Icons, "resources", techArea.ToString().ToLowerInvariant() + "_research.dds");
                    // icon to be displayed on the 3 root nodes
                    ImageOutput.TransformAndOutputImage(
                        inputPath, 
                        Path.Combine(techImageOutputDir, techArea + "-root.png"));
                
                    // area icon
                    ImageOutput.TransformAndOutputImage(
                        inputPath, 
                        Path.Combine(areaDir, techArea.ToString().ToLowerInvariant() + ".png"));
                }

                // tech categories
                CopyCategoryImages(techImageOutputDir);
            }

            ObjectsDependantOnTechs dependants = null;
            if (parseTargets.Any()) {
                var dependantsGraphCreator = new DependantsGraphCreator(Localisation, CWParser, StellarisDirectoryHelper, ModDirectoryHelpers, techData);
                dependants = dependantsGraphCreator.CreateDependantGraph();

                if (true) {
                    foreach (var parseTarget in parseTargets) {
                        var imageOutputDir = OutputDirectoryHelper.GetImagesPath(ParseTarget.Technologies.ImagesDirectory());
                        ModEntityData<Entity> entityData = dependants.Get(parseTarget);
                        CopyMainImages(entityData.AllEntities, ParseTarget.Technologies.ImagesDirectory(),
                            imageOutputDir);
                    }
                }
            }
            
            
            
            var visDataMarshaler = new VisDataMarshaler(localisation, OutputDirectoryHelper);
            var techVisResults = visDataMarshaler.CreateVisData(techData, techImageOutputDir);
            List<JSModInfo> modInfos = new List<JSModInfo>();
            foreach (var (modGroup, visResult) in techVisResults) {
                var (jsFileName, jsVariable) = visResult.WriteVisDataToOneJSFile(OutputDirectoryHelper.Data, modGroup + "-tech.js", modGroup + "GraphDataTech");
                modInfos.Add(new JSModInfo() {name = modGroup, jsVarable = jsVariable, fileName = jsFileName});
            }

            VisData.WriteJavascriptObject(modInfos, OutputDirectoryHelper.Data, "TechFiles.js", "techDataFiles");
            
            var importListing = modInfos.Select(x => x.fileName).Select(x => $"<script type=\"text/javascript\" src=\"data/{x}?v=2\"></script>").ToArray();
            File.WriteAllLines(Path.Combine(OutputDirectoryHelper.Data, "JS-Tech-Imports.txt"), importListing);


            if (dependants != null) {
                var dependantVisResults = visDataMarshaler.CreateGroupedVisDependantData(techVisResults, dependants, parseTargets);
                List<JSModInfo> modInfos = new List<JSModInfo>();
                foreach (var (modGroup, visResult) in dependantVisResults) {
                    var (jsFileName, jsVariable) = visResult.WriteVisDataToOneJSFile(Path.Combine(outputRoot, "data"), modGroup + "-dependants.js", modGroup + "GraphDataDependants");
                    modInfos.Add(new JSModInfo() {name = modGroup, jsVarable = jsVariable, fileName = jsFileName});
                } 

                VisData.WriteJavascriptObject(modInfos, Path.Combine(outputRoot, "data"), "DependantFiles.js", "dependantDataFiles");

                var importListing = modInfos.Select(x => x.fileName).Select(x => $"<script type=\"text/javascript\" src=\"data/{x}?v=2\"></script>").ToArray();
                File.WriteAllLines(Path.Combine(outputRoot, "data", "JS-Dependants-Imports.txt"), importListing);
            }
            
        }
        
        
        
        
        
        public ModEntityData<Tech> ParseStellarisFiles() {
            var parser = new TechTreeGraphCreator(localisation, cwParser, StellarisDirectoryHelper, modDirectoryHelpers);
            return parser.CreateTechnologyGraph();
        }

        public void CopyImages(ModEntityData<Tech> techsAndDependencies) {
            CopyMainImages(techsAndDependencies.AllEntities, "technologies", Path.Combine("images", "technologies"));

         

            // tech category images no mod support here
            var categoryDir = Path.Combine(outputRoot, "images", "technologies", "categories");
            Directory.CreateDirectory(categoryDir);
            
            
            
            
            var categoryFile = DirectoryWalker.FindFilesInDirectoryTree(StellarisDirectoryHelper.GetTechnologyDirectory(stellarisRootDirectory), "00_category.txt");
            var catcatFile = new CWParserHelper().ParseParadoxFiles(categoryFile.Select(x => x.FullName).ToList());

            if (!catcatFile.Any()) {
                Log.Logger.Warning("Could not find 00_category.txt to get all the category icons");
                return;
            }

            var catNode = catcatFile.First();

            foreach (var category in catNode.Value.Nodes) {
                var catName = category.Key;
                var imagePath = category.GetKeyValue("icon");

                ImageOutput.TransformAndOutputImage(Path.Combine(stellarisRootDirectory, imagePath),
                    Path.Combine(outputDir, catName + ".png"));
            }
            
            
            
            
            
            
            
            
            
            TechCategoryImageOutput.OutputCategoryImages(StellarisDirectoryHelper.Root, categoryDir);
        }

        public IDictionary<string, VisData> GenerateJsGraph(TechsAndDependencies techsAndDependencies) {
            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateVisData(techsAndDependencies, Path.Combine("images", "technologies"));
            List<JSModInfo> modInfos = new List<JSModInfo>();
            foreach (var (modGroup, visResult) in visResults) {
                var (jsFileName, jsVariable) = visResult.WriteVisDataToOneJSFile(Path.Combine(outputRoot, "data"), modGroup + "-tech.js", modGroup + "GraphDataTech");
                modInfos.Add(new JSModInfo() {name = modGroup, jsVarable = jsVariable, fileName = jsFileName});
            }

            VisData.WriteJavascriptObject(modInfos, Path.Combine(outputRoot, "data"), "TechFiles.js", "techDataFiles");
            
            var importListing = modInfos.Select(x => x.fileName).Select(x => $"<script type=\"text/javascript\" src=\"data/{x}?v=2\"></script>").ToArray();
            File.WriteAllLines(Path.Combine(outputRoot, "data", "JS-Tech-Imports.txt"), importListing);
            
            return visResults;
        }

  
        public IDictionary<string, VisData> ParseObjectsDependantOnTechs(ModEntityData<Tech> techsAndDependencies, IDictionary<string, VisData> techVisData) {
            var parser = new DependantsGraphCreator(localisation, cwParser, StellarisDirectoryHelper, modDirectoryHelpers, techsAndDependencies);
            var dependantGraph = parser.CreateDependantGraph();
            CopyMainImages(dependantGraph.Buildings.AllEntities, "buildings", Path.Combine("images", "buildings"));
            var visDataMarshaler = new VisDataMarshaler(localisation);
            var visResults = visDataMarshaler.CreateGroupedVisDependantData(techVisData, dependantGraph, Path.Combine("images", "buildings"));
            List<JSModInfo> modInfos = new List<JSModInfo>();
            foreach (var (modGroup, visResult) in visResults) {
                var (jsFileName, jsVariable) = visResult.WriteVisDataToOneJSFile(Path.Combine(outputRoot, "data"), modGroup + "-dependants.js", modGroup + "GraphDataDependants");
                modInfos.Add(new JSModInfo() {name = modGroup, jsVarable = jsVariable, fileName = jsFileName});
            } 

            VisData.WriteJavascriptObject(modInfos, Path.Combine(outputRoot, "data"), "DependantFiles.js", "dependantDataFiles");

            var importListing = modInfos.Select(x => x.fileName).Select(x => $"<script type=\"text/javascript\" src=\"data/{x}?v=2\"></script>").ToArray();
            File.WriteAllLines(Path.Combine(outputRoot, "data", "JS-Dependants-Imports.txt"), importListing);

            return visResults;
        }
        
    }
}
