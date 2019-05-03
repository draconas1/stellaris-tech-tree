using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.FileParsing;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using Serilog;
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
            IgnoreFiles.AddRange(new [] { "00_tier.txt", "00_category.txt", "eac_category.txt" });
        }

        public ModEntityData<Tech> CreateTechnologyGraph() {
            ModEntityData<Tech> techs = null;
            foreach (var modDirectoryHelper in StellarisDirectoryHelper.CreateCombinedList(stellarisDirectoryHelper, modDirectoryHelpers)) {
                techs = ProcessDirectoryHelper(techs, modDirectoryHelper, null);
            }
            
            // post process because while most things work on the principle of:
            // later mod override core
            // later mod override core
            // core
            // some have the core first, then additional features that depend on it (Zenith, I am looking at you)
            // so need to post process

            techs?.ApplyToChain((modTechs, modLinks) => {
                foreach (var (key, tech) in modTechs) {
                    if (tech.Prerequisites.Count() == tech.PrerequisiteIds.Count()) {
                        continue;
                    }

                    Log.Logger.Debug("Tech {id} had missing pre-requisite, trying to find it in the complete listing", key);
                    var populatedPreReqs = tech.Prerequisites.Select(preReq => preReq.Id).ToHashSet();
                    foreach (var missingPreReq in tech.PrerequisiteIds.Where(x => !populatedPreReqs.Contains(x))) {
                        Tech attemptToFindPreq = techs[missingPreReq];
                        if (attemptToFindPreq != null) {
                            tech.Prerequisites.Add(attemptToFindPreq);
                            modLinks.Add(new Link() {From = attemptToFindPreq, To = tech});
                            Log.Logger.Debug("Found prereq {key} in file {file}", attemptToFindPreq.Id, attemptToFindPreq.FilePath);
                        }
                        else {
                            Log.Logger.Debug("Still unable to find {prereqId} for Tech {id}", missingPreReq, key);
                        }
                    }
                }
            });

            return techs;
        }
        
        protected override Tech Construct(CWNode node) {
            return new Tech(node.Key);
        }


        private CWNode DepthFirstSearchNodes(IEnumerable<CWNode> nodes, KeyValuePair<string, string> kv) {
            foreach (var cwNode in nodes) {
                var found = cwNode.RawKeyValues.Where(x => x.Equals(kv)).FirstOrDefault(null);
                if (found != null) {
                    return DepthFirstSearchNodes(cwNode.Nodes, kv);
                }
                else {
                    return cwNode;
                }
            }

            return null;
        }

        private bool ResolveBoolean(bool startingValue, CWNode node) {
            bool endingValue;
            switch (node.Key.ToLowerInvariant()) {
                case "and":
                case "or":
                    endingValue = startingValue;
                    break;
                case "nor":
                case "not":
                    endingValue = !startingValue;
                    break;
                default: endingValue = startingValue;
                    break;
            }

            if (node.Parent != null) {
                return ResolveBoolean(endingValue, node.Parent);
            }

            return endingValue;
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
            result.Tier = node.GetKeyValueOrDefault("tier", "0").ToInt();
            result.BaseCost = node.GetKeyValueOrDefault("cost", "0").ToInt();

            var potentialNodes = node.GetNodes("potential").ToList();
            CWNode machineNode = DepthFirstSearchNodes(potentialNodes, new KeyValuePair<string, string>("has_authority", "auth_machine_intelligence"));
            if (machineNode != null) {
                result.Machines = ResolveBoolean(true, machineNode);
            }
            CWNode gestaltNode = DepthFirstSearchNodes(potentialNodes, new KeyValuePair<string, string>("has_ethic", "ethic_gestalt_consciousness"));
            if (gestaltNode != null) {
                result.Gestalt = ResolveBoolean(true, gestaltNode);
            }

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

            node.ActOnNodes("prereqfor_desc", cwNode => {
                var description = cwNode.GetKeyValue("desc");
                if (description != null) {
                    result.ExtraDesc = description;
                    result.ExtraName = cwNode.GetKeyValue("title");
                }
                else {
                    var subNode = cwNode.Nodes.FirstOrDefault();
                    if (subNode != null) {
                        result.ExtraDesc = subNode.GetKeyValue("desc");;
                        result.ExtraName = subNode.GetKeyValue("title");
                    }
                }

                result.ExtraDesc = result.ExtraDesc != null ? LocalisationApiHelper.GetName(result.ExtraDesc) : null;
                result.ExtraName = result.ExtraName != null ? LocalisationApiHelper.GetName(result.ExtraName) : null;
            });
        }

        protected override string GetDirectory(StellarisDirectoryHelper directoryHelper) {
            return directoryHelper.Technology;
        }
    }
}
