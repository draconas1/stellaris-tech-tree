using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CWTools.Process;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation
{
    class DependantsGraphCreator : EntityCreator
    {
        private readonly ICWParserHelper cwParserHelper;
        private readonly StellarisDirectoryHelper stellarisDirectoryHelper;
        private readonly IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers;

        /// <summary>
        /// List of file names (exact) that will be skipped when parsing.  Defaults to  "00_tier.txt", "00_category.txt" 
        /// </summary>
        public List<string> IgnoreFiles { get; set; }
        /// <summary>
        /// File mask used for finding files.  defaults to "*.txt"
        /// </summary>
        public string ParseFileMask { get; set; }


        public DependantsGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper, 
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers) : base(localisationApiHelper) 
        {
            this.cwParserHelper = cwParserHelper;
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers;
            IgnoreFiles = new List<string>();
            IgnoreFiles.AddRange(new [] { "00_tier.txt", "00_category.txt" });
            ParseFileMask = StellarisDirectoryHelper.TextMask;
        }

        public ObjectsDependantOnTechs CreateDependantGraph(TechsAndDependencies techsAndDependencies)
        {
            var buildings = new Dictionary<string, Building>();
           
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                GetBuildingsFromFile(buildings, modDirectoryHelper);
            }

            var links = PopulateTechDependenciesAndReturnLinks(buildings.Values, techsAndDependencies.Techs);

            return new ObjectsDependantOnTechs() {
                Buildings = buildings,
                Prerequisites = links
            };
        }

        private void GetBuildingsFromFile(Dictionary<string, Building> buildings, StellarisDirectoryHelper directoryHelper) {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(directoryHelper.Buildings, ParseFileMask, IgnoreFiles);
            var parsedTechFiles = cwParserHelper.ParseParadoxFiles(techFiles.Select(x => x.FullName).ToList());
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immediate children of each file, which is the individual items.
                foreach (var node in file.Value.Nodes) {
                    var building = CreateBuilding(file.Key, node);
                    if (building.PrerequisiteIds.Any()) {
                        buildings[building.Id] = building;
                    }
                }
            }
        }

        private Building CreateBuilding(string filePath, CWNode node) {
            var result = new Building(node.Key) {
                BaseBuildTime = int.Parse(node.GetKeyValue("base_buildtime") ?? "0"),
                Category = node.GetKeyValue("category"),
                FilePath = filePath
            };
            Initialise(result, filePath, node);
            
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = Int32.Parse(value.Value)));
                cwNode.ActOnNodes("upkeep", costNode => costNode.KeyValues.ForEach(value => result.Upkeep[value.Key] = Int32.Parse(value.Value)));
                cwNode.ActOnNodes("produces", costNode => costNode.KeyValues.ForEach(value => result.Produces[value.Key] = Int32.Parse(value.Value)));
            });

            return result;
        }
    }
}
