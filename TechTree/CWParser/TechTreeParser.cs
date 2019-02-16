using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CWTools.Localisation;
using CWToolsHelpers;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.ScriptedVariables;
using TechTree.DTO;
using TechTree.Extensions;

namespace TechTree.CWParser
{
    class TechTreeParser
    {
        private readonly LocalisationApiHelper localisationApiHelper;
        private readonly CWParserHelper cwParserHelper;
        private readonly StellarisDirectoryHelper stellarisDirectoryHelper;
        private readonly IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers;

        /// <summary>
        /// List of file names (exact) that will be skipped when parsing.  Defaults to nont (all files)
        /// </summary>
        public List<string> IgnoreFiles { get; set; }
        /// <summary>
        /// File mask used for finding files.  defaults to "*.txt"
        /// </summary>
        public string ParseFileMask { get; set; }


        public TechTreeParser(LocalisationApiHelper localisationApiHelper, CWParserHelper cwParserHelper, 
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers)
        {
            this.localisationApiHelper = localisationApiHelper;
            this.cwParserHelper = cwParserHelper;
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers;
            IgnoreFiles = new List<string>();
            IgnoreFiles.AddRange(new [] { "00_tier.txt", "00_category.txt" });
            ParseFileMask = "*.txt";
        }

        private void getTechsFromFile(Dictionary<string, Tech> techs, StellarisDirectoryHelper directoryHelper) {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(directoryHelper.Technology, ParseFileMask, IgnoreFiles);
            var parsedTechFiles = cwParserHelper.ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immediate children of each file, which is the individual techs.
                foreach (var node in file.Nodes)
                {
                    Tech tech = ProcessNodeModel(node);
                    techs[tech.Id] = tech;
                }
            }
        }
        
        public TechsAndDependencies ParseTechFiles()
        {
            var techs = new Dictionary<string, Tech>();
            var links = new HashSet<Link>();
            getTechsFromFile(techs, stellarisDirectoryHelper);
            if (modDirectoryHelpers != null) {
                foreach (var modDirectoryHelper in modDirectoryHelpers) {
                    getTechsFromFile(techs, modDirectoryHelper);
                }
            }

            // populate prerequisites
            foreach (var (id, tech) in techs) {
                // it is possible that the pre-reqs for a technology do not exist
                // in this we do not add them to the populated list, but leave them in the ids list
                var prereqs = new List<Tech>();
                if (tech.PrerequisiteIds != null) {
                    foreach (var prerequisiteId in tech.PrerequisiteIds) {
                        Tech prereq;
                        if (techs.TryGetValue(prerequisiteId, out prereq)) {
                            prereqs.Add(prereq);
                            links.Add(new Link() {From = prereq, To = tech});
                        }
                        else {
                            Debug.WriteLine("Could not find prerequisite {0} for tech {1}", prerequisiteId, id);
                        }
                    }
                }

                tech.Prerequisites = prereqs;
            }

            return new TechsAndDependencies() {Techs = techs, Prerequisites = links};
        }

        private Tech ProcessNodeModel(CWNode node) {
            var result = new Tech(node.Key) {
                Name =  localisationApiHelper.GetName(node.Key),
                Description = localisationApiHelper.GetDescription(node.Key)
            };

            TechArea area;
            string areaKeyValue = node.GetKeyValue("area");
            if ("physics".Equals(areaKeyValue, StringComparison.OrdinalIgnoreCase)) {
                area = TechArea.Physics;
            }
            else if ("society".Equals(areaKeyValue, StringComparison.OrdinalIgnoreCase)) {
                area = TechArea.Society;
            }
            else if ("engineering".Equals(areaKeyValue, StringComparison.OrdinalIgnoreCase)) {
                area = TechArea.Engineering;
            }
            else {
                throw new Exception("Unable to determine tech area for " + node.Key + " found " + areaKeyValue);
            }

            result.Area = area;
            result.Tier = int.Parse(node.GetKeyValue("tier") ?? "0");
            result.BaseCost = int.Parse(node.GetKeyValue("cost") ?? "0");

           
            //categories, usually only one, but can be more
            if (node.GetNode("category") != null)
            {
                result.Categories = node.GetNode("category").Values;
            }
            
            // interesting flags about the tech
            var techFlags = new List<TechFlag>();
            // rare purple tech
            if ("yes".Equals(node.GetKeyValue("is_rare"), StringComparison.InvariantCultureIgnoreCase))
            {
                techFlags.Add(TechFlag.Rare);
            }

            // starter technology
            if ("yes".Equals(node.GetKeyValue("start_tech"), StringComparison.InvariantCultureIgnoreCase))
            {
                techFlags.Add(TechFlag.Starter);
            }

            // may cause endgame crisis or AI revolution
            if ("yes".Equals(node.GetKeyValue("is_dangerous"), StringComparison.InvariantCultureIgnoreCase))
            {
                techFlags.Add(TechFlag.Dangerous);
            }
            
            // tech that requires an acquisition - base weight is 0
            // this is not foolproof, look up some other things
            if ("0".Equals(node.GetKeyValue("weight"), StringComparison.InvariantCultureIgnoreCase))
            {
                techFlags.Add(TechFlag.NonTechDependency);
            }
            else
            {
                // for some reason some tech has a weighting of 1, then a weight modifier that just sets it to 0.
                var weightNode = node.GetNode("weight_modifier");
                if (weightNode != null)
                {
                    var weightFactor = weightNode.GetKeyValue("factor");
                    if (weightFactor == "0" && !weightNode.Nodes.Any())
                    {
                        techFlags.Add(TechFlag.NonTechDependency);
                    }
                }
            }

            // fallen empire tech has a weight of 1 then a node that sets to 0 if you are not a fallen empire.
            // imperfect method for finding fallen empire tech that needs acquisition.
            if (node.GetKeyValue("tier") == "@fallentechtier" && node.GetRawKeyValue("cost") == "@fallentechcost")
            {
                techFlags.Add(TechFlag.NonTechDependency);
            }

            // tech that is repeatable
            if (node.GetKeyValue("cost_per_level") != null)
            {
                techFlags.Add(TechFlag.Repeatable);
            }

            result.Flags = techFlags;
            
            node.ActOnNodes("prerequisites", cwNode => result.PrerequisiteIds = cwNode.Values);

            // if icon has been defined
            if (node.GetKeyValue("icon") != null)
            {
                result.Icon = node.GetKeyValue("icon");
            }
            
            // if its a DLC tech
            node.ActOnNodes("potential", potentialNode => {
                result.DLC = potentialNode.GetKeyValue("host_has_dlc");
            });

            return result;
        }
    }
}
