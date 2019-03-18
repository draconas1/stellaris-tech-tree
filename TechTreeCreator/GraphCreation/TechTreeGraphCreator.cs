using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using TechTreeCreator.DTO;

namespace TechTreeCreator.GraphCreation
{
    class TechTreeGraphCreator : EntityCreator<Tech> {
        private readonly StellarisDirectoryHelper stellarisDirectoryHelper;
        private readonly IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers;

        public TechTreeGraphCreator(ILocalisationApiHelper localisationApiHelper, ICWParserHelper cwParserHelper, 
            StellarisDirectoryHelper stellarisDirectoryHelper, IEnumerable<StellarisDirectoryHelper> modDirectoryHelpers) : base(localisationApiHelper, cwParserHelper)
        {
            this.stellarisDirectoryHelper = stellarisDirectoryHelper;
            this.modDirectoryHelpers = modDirectoryHelpers;
            IgnoreFiles = new List<string>();
            IgnoreFiles.AddRange(new [] { "00_tier.txt", "00_category.txt" });
        }

        public TechsAndDependencies CreateTechnologyGraph()
        {
            var techs = new Dictionary<string, Tech>();
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                ProcessDirectoryHelper(techs, modDirectoryHelper);
            }
            var links = PopulateTechDependenciesAndReturnLinks(techs.Values, techs);
            return new TechsAndDependencies() {Techs = techs, Prerequisites = links};
        }
        
        protected override Tech Construct(CWNode node) {
            return new Tech(node.Key);
        }

        protected override void SetVariables(Tech result, CWNode node) {
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
            if (node.GetRawKeyValue("tier") == "@fallentechtier" && node.GetRawKeyValue("cost") == "@fallentechcost")
            {
                techFlags.Add(TechFlag.NonTechDependency);
            }

            // tech that is repeatable
            if (node.GetKeyValue("cost_per_level") != null)
            {
                techFlags.Add(TechFlag.Repeatable);
            }

            result.Flags = techFlags;
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.Technology;
        }
    }
}
