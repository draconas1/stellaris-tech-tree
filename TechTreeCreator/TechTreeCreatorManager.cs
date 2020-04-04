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
        public bool ForceModOverwriting { get; set; }

        public bool CopyImages { get; set; }

        private IList<StellarisDirectoryHelper> modDirectoryHelpers;
        private IList<StellarisDirectoryHelper> ModDirectoryHelpers => modDirectoryHelpers ?? (modDirectoryHelpers = ModDirectoryHelper.CreateDirectoryHelpers(Mods.NullToEmpty(), ForceModOverwriting));


        private ILocalisationApiHelper localisation;
        private ILocalisationApiHelper Localisation => localisation ?? (localisation = new LocalisationApiHelper(StellarisDirectoryHelper, ModDirectoryHelpers, Language));
        
        private ICWParserHelper cwParser;
        private ICWParserHelper CWParser => cwParser ?? (cwParser = new CWParserHelper(new ScriptedVariableAccessor(StellarisDirectoryHelper, ModDirectoryHelpers)));


        /// <summary>
        /// Will initalise optional values to sensible defaults.
        /// </summary>
        /// <param name="stellarisRoot"></param>
        /// <param name="outputRoot"></param>
        public TechTreeCreatorManager(string stellarisRoot, string outputRoot) {
            this.outputRoot = outputRoot;
            //Support UTF-8
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Language = STLLang.English;
            
            OutputDirectoryHelper = new OutputDirectoryHelper(outputRoot);
            
            // setup parser
            // Include extra pie! Especially for Piebadger.
            StellarisDirectoryHelper = new StellarisDirectoryHelper(stellarisRoot);

            CopyImages = true;
        }
        
        public void Parse(IEnumerable<ParseTarget> parseTargets) {
            var techTreeGraphCreator = new TechTreeGraphCreator(Localisation, CWParser, StellarisDirectoryHelper, ModDirectoryHelpers);
            ModEntityData<Tech> techData = techTreeGraphCreator.CreateTechnologyGraph();

            Log.Logger.Debug("Processed {entityCount} techs with {linkCount} Links", techData.EntityCount, techData.LinkCount);
            // process technolgoies first
            var techImageOutputDir = OutputDirectoryHelper.GetImagesPath(ParseTarget.Technologies.ImagesDirectory());
            if (CopyImages) {
                CopyMainImages(techData.AllEntities, ParseTarget.Technologies.ImagesDirectory(),
                    techImageOutputDir);

                // because the image copying only get the most recent version of the entity, make sure that the image flag is set on all
                // relevant for the vanilla graph display
                var currentTechs = techData.AllEntitiesByKey;
                techData.ApplyToChain((techs, links) =>
                {
                    foreach (var tech in techs.Values)
                    {
                        tech.IconFound = currentTechs[tech.Id].IconFound;
                    }
                }); 
                
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

            // process dependant objects
            ObjectsDependantOnTechs dependants = null;
            var parseTargetsWithoutTechs = parseTargets.WithoutTechs().ToList();
            if (parseTargetsWithoutTechs.Any()) {
                var dependantsGraphCreator = new DependantsGraphCreator(Localisation, CWParser, StellarisDirectoryHelper, ModDirectoryHelpers, techData);
                dependants = dependantsGraphCreator.CreateDependantGraph(parseTargetsWithoutTechs);

                if (CopyImages) {
                    foreach (var parseTarget in parseTargetsWithoutTechs) {
                        var imageOutputDir = OutputDirectoryHelper.GetImagesPath(parseTarget.ImagesDirectory());
                        var entityData = dependants.Get(parseTarget);
                        CopyMainImages(entityData, parseTarget.ImagesDirectory(), imageOutputDir);
                    }
                    dependants.FixImages();
                }
            }
            
            var visDataMarshaler = new VisDataMarshaler(localisation, OutputDirectoryHelper);
            IDictionary<string, VisData> techVisResults = visDataMarshaler.CreateTechVisData(techData, techImageOutputDir);
            ModEntityData<Tech> coreGameTech = techData.FindCoreGameData();
            if (coreGameTech != null) {
                IDictionary<string,VisData> techVisData = visDataMarshaler.CreateTechVisData(coreGameTech, techImageOutputDir);
                techVisResults["Stellaris-No-Mods"] = techVisData[StellarisDirectoryHelper.StellarisCoreRootDirectory];
            }
            else {
                Log.Logger.Warning("Could not find core game tech files to generate vanilla tree");
            }
   
            VisData rootNotes = visDataMarshaler.CreateRootNotes(techData, techImageOutputDir);
            techVisResults["Tech-Root-Nodes"] = rootNotes;
            WriteVisData(techVisResults, true);

            if (dependants != null) {
                var dependantVisResults = visDataMarshaler.CreateGroupedVisDependantData(techVisResults, dependants, parseTargetsWithoutTechs);
                var coreDependantsOnly = dependants.CopyOnlyCore();

                // also do a no-mods lookup
                var stellarisNoModsTechVisResult = techVisResults["Stellaris-No-Mods"];
                IDictionary<string, VisData> visLookupData =
                    stellarisNoModsTechVisResult != null ? new Dictionary<string, VisData>() {{"Stellaris-No-Mods", stellarisNoModsTechVisResult}} : techVisResults;
                var coreDependantData = visDataMarshaler.CreateGroupedVisDependantData(visLookupData, coreDependantsOnly, parseTargetsWithoutTechs);
                
                if (coreDependantData.ContainsKey(StellarisDirectoryHelper.StellarisCoreRootDirectory)) {
                    dependantVisResults["Stellaris-No-Mods"] = coreDependantData[StellarisDirectoryHelper.StellarisCoreRootDirectory];
                }
                else {
                    Log.Logger.Warning("Could not find core game dependant files to generate vanilla tree");
                }

                WriteVisData(dependantVisResults, false);
            }
        }

        private void WriteVisData(IDictionary<string, VisData> visResults, bool isTech) {
            string jsFileNameSuffix = isTech ? "-tech.js" : "-dependants.js";
            string jsVariableSuffix = isTech ? "GraphDataTech" : "GraphDataDependants";
            string jsModMappingFileName = isTech ? "TechFiles.js" : "DependantFiles.js";
            string jsModMappingFileVariable = isTech ? "techDataFiles" : "dependantDataFiles";
            string jsImportFileName =  isTech ? "JS-Tech-Imports.txt" : "JS-Dependants-Imports.txt";
            
            List<JSModInfo> modInfos = new List<JSModInfo>();
            foreach (var (modGroup, visResult) in visResults) {
                var (jsFileName, jsVariable) = visResult.WriteVisDataToOneJSFile(OutputDirectoryHelper.Data, modGroup + jsFileNameSuffix, modGroup + jsVariableSuffix);
                modInfos.Add(new JSModInfo() {name = modGroup, jsVarable = jsVariable, fileName = jsFileName});
            }

            VisData.WriteJavascriptObject(modInfos, OutputDirectoryHelper.Data, jsModMappingFileName, jsModMappingFileVariable);

            var importListing = modInfos.Select(x => x.fileName).Select(x => $"<script type=\"text/javascript\" src=\"data/{x}?v=2\"></script>").ToList();
            importListing.Sort();
            File.WriteAllLines(Path.Combine(OutputDirectoryHelper.Data, jsImportFileName), importListing);
        }
        
        private void CopyMainImages(IEnumerable<Entity> entities, string inputPath, string outputPath) {
            //build the mod input list
            var inputDirectories = StellarisDirectoryHelper
                .CreateCombinedList(StellarisDirectoryHelper, modDirectoryHelpers, Last)
                .Select(x => Path.Combine(x.Icons, inputPath)).ToList();

            // tech images
            ImageOutput.TransformAndOutputImages(inputDirectories, Path.Combine(outputRoot, outputPath), entities);
        }

        private void CopyCategoryImages(string techOutputDir) {
            var categoryDir = Path.Combine(techOutputDir, "categories");
            Directory.CreateDirectory(categoryDir);
            
            var helpers = StellarisDirectoryHelper
                .CreateCombinedList(StellarisDirectoryHelper, modDirectoryHelpers);

            foreach (var helper in helpers) {
                List<FileInfo> categoryFiles = new List<FileInfo>();
                var catPath = Path.Combine(helper.Technology, "category");
                if (Directory.Exists(catPath)) {
                    List<FileInfo> findFilesInDirectoryTree = DirectoryWalker.FindFilesInDirectoryTree(catPath, StellarisDirectoryHelper.TextMask);
                    Log.Logger.Debug("{helper} has categories files: {modFiles}", helper.ModName, findFilesInDirectoryTree.Select(x => x.Name));
                    categoryFiles.AddRange(findFilesInDirectoryTree);
                }
                else {
                    Log.Logger.Debug("{helper} has no tech categories folder", helper.ModName);
                }

                var categoryFileNodes = new CWParserHelper().ParseParadoxFiles(categoryFiles.Select(x => x.FullName).ToList());
                foreach (CWNode fileNode in categoryFileNodes.Values) {
                    foreach (var category in fileNode.Nodes) {
                        var catName = category.Key;
                        var imagePath = category.GetKeyValue("icon") ?? category.Key;

                        ImageOutput.TransformAndOutputImage(Path.Combine(helper.Root, imagePath),
                            Path.Combine(categoryDir, catName + ".png"));
                    };
                }
            }
        }
    }
}
