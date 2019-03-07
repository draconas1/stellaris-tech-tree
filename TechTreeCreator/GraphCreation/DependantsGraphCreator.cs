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
    class DependantsGraphCreator
    {
        private readonly ILocalisationApiHelper localisationApiHelper;
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
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers)
        {
            this.localisationApiHelper = localisationApiHelper;
            this.cwParserHelper = cwParserHelper;
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers.NullToEmpty();
            IgnoreFiles = new List<string>();
            IgnoreFiles.AddRange(new [] { "00_tier.txt", "00_category.txt" });
            ParseFileMask = StellarisDirectoryHelper.TextMask;
        }

        public ObjectsDependantOnTechs CreateDependantGraph(TechsAndDependencies techsAndDependencies)
        {
            var buildings = new Dictionary<string, Building>();
            var links = new HashSet<Link>();
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                GetBuildingsFromFile(buildings, modDirectoryHelper);
            }
            
            // populate prerequisites
            foreach (var (id, building) in buildings) {
                // it is possible that the pre-reqs for a technology do not exist
                // in this we do not add them to the populated list, but leave them in the ids list
                var prereqs = new List<Tech>();
                if (building.PrerequisiteIds != null) {
                    foreach (var prerequisiteId in building.PrerequisiteIds) {
                        if (techsAndDependencies.Techs.TryGetValue(prerequisiteId, out var prereq)) {
                            prereqs.Add(prereq);
                            links.Add(new Link() {From = prereq, To = building});
                        }
                        else {
                            Debug.WriteLine("Could not find prerequisite {0} for building {1}", prerequisiteId, id);
                        }
                    }
                }

                building.Prerequisites = prereqs;
            }

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
                    var building = CreateBuilding(node);
                    if (building.PrerequisiteIds.Any()) {
                        buildings[building.Id] = building;
                    }
                }
            }
        }

        private Building CreateBuilding(CWNode node) {
            var result = new Building(node.Key) {
                Name =  localisationApiHelper.GetName(node.Key),
                Description = localisationApiHelper.GetDescription(node.Key),
                BaseBuildTime = int.Parse(node.GetKeyValue("base_buildtime") ?? "0"),
                Category = node.GetKeyValue("category"),
            };
            
            // if icon has been defined
            if (node.GetKeyValue("icon") != null)
            {
                result.Icon = node.GetKeyValue("icon");
            }
            
            node.ActOnNodes("prerequisites", cwNode => result.PrerequisiteIds = cwNode.Values, () => result.PrerequisiteIds = new string[]{});
            
            node.ActOnNodes("resources", cwNode => {
                cwNode.ActOnNodes("cost", costNode => costNode.KeyValues.ForEach(value => result.Cost[value.Key] = Int32.Parse(value.Value)));
                cwNode.ActOnNodes("upkeep", costNode => costNode.KeyValues.ForEach(value => result.Upkeep[value.Key] = Int32.Parse(value.Value)));
                cwNode.ActOnNodes("produces", costNode => costNode.KeyValues.ForEach(value => result.Produces[value.Key] = Int32.Parse(value.Value)));
            });

            return result;
        }
    }
}
