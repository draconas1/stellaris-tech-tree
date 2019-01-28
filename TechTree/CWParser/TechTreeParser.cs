using System;
using System.Collections.Generic;
using System.Linq;
using CWTools.Localisation;
using TechTree.DTO;
using TechTree.Extensions;
using TechTree.FileIO;

namespace TechTree.CWParser
{
    class TechTreeParser
    {
        /// <summary>
        /// List of file names (exact) that will be skipped when parsing.  Defaults to nont (all files)
        /// </summary>
        public List<string> IgnoreFiles { get; set; }
        /// <summary>
        /// File mask used for finding files.  defaults to "*.txt"
        /// </summary>
        public string ParseFileMask { get; set; }

        public ISet<string> Areas { get; set; }

        public ISet<string> Categories { get; set; }

        private readonly ILocalisationAPI localisationAPI;
        private readonly Dictionary<string, string> scriptedVariables;
        private readonly string rootTechDir;

        public TechTreeParser(ILocalisationAPI localisationAPI, Dictionary<string, string> scriptedVariables, string rootTechDir)
        {
            this.localisationAPI = localisationAPI;
            this.scriptedVariables = scriptedVariables;
            this.rootTechDir = rootTechDir;
            IgnoreFiles = new List<string>();
            ParseFileMask = "*.txt";
            Areas = new HashSet<string>();
            Categories = new HashSet<string>();
        }

        public TechsAndDependencies ParseTechFiles()
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(rootTechDir, ParseFileMask, IgnoreFiles);
            var parsedTechFiles = new CWParserHelper().ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());

            var techs = new Dictionary<string, Tech>();
            var links = new HashSet<Link>();
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immiediate children of each file, which is the individual techs.
                foreach (var node in file.Nodes)
                {
                    // only process if we have no area filter, or this is a tech from that area
                    bool process = !(Areas.Any() && !Areas.Contains(node.GetKeyValue("area")));
                    
                    // if there is a category filter, filter by tech category
                    if (Categories.Any() && !(Categories.Intersect(getCategories(node)).Any()))
                    {
                        process = false;
                    }

                    if (process) {
                        Tech tech = ProcessNodeModel(node);
                        techs[tech.Id] = tech;
                    }
                }
            }

            // populate prerequisites
            foreach (var (id, tech) in techs) {
                // it is possible that the pre-reqs for a technology do not exist
                // e.g. they were not processed due to the filters above.
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
                            Console.WriteLine("Could not find prerequisite {0} for tech {1}", prerequisiteId, id);
                        }
                    }
                }

                tech.Prerequisites = prereqs;
            }

            return new TechsAndDependencies() {Techs = techs, Prerequisites = links};
        }

        private List<string> getCategories(CWNode node)
        {
            var cats = node.GetNode("category");
            if (cats != null)
            {
                return cats.Values;
            }
            return new List<string>();
        }


        public Tech ProcessNodeModel(CWNode node) {
            var result = new Tech(node.Key) {
                Name =  localisationAPI.GetName(node.Key),
                Description = localisationAPI.GetDescription(node.Key)
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
            result.Tier = int.Parse(node.GetKeyValue("tier", scriptedVariables) ?? "0");
            result.BaseCost = int.Parse(node.GetKeyValue("cost", scriptedVariables) ?? "0");

           
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
                techFlags.Add(TechFlag.RequiresAcquisition);
            }

            // tech that is repeatable
            if (node.GetKeyValue("cost_per_level") != null)
            {
                techFlags.Add(TechFlag.Repeatable);
            }

            result.Flags = techFlags;
            
            node.ActOnNode("prerequisites", cwNode => result.PrerequisiteIds = cwNode.Values);

            // if icon has been defined
            if (node.GetKeyValue("icon") != null)
            {
                result.Icon = node.GetKeyValue("icon", scriptedVariables);
            }

            return result;
        }
    }
}
