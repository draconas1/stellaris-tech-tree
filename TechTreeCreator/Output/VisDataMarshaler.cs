using System;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.Directories;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using NetExtensions.Object;
using Serilog;
using TechTreeCreator.DTO;

namespace TechTreeCreator.Output {
    public class VisDataMarshaler {
        private readonly ILocalisationApiHelper localisationApi;
        private readonly OutputDirectoryHelper outputDirectoryHelper;

        public VisDataMarshaler(ILocalisationApiHelper localisationApi, OutputDirectoryHelper outputDirectoryHelper) {
            this.localisationApi = localisationApi;
            this.outputDirectoryHelper = outputDirectoryHelper;
        }

        public IDictionary<string, VisData> CreateGroupedVisDependantData(IDictionary<string, VisData> techData, ObjectsDependantOnTechs objectsDependantOnTechs, IEnumerable<ParseTarget> parseTargets) {
            Dictionary<string,VisNode> prereqTechNodeLookup = techData.Values.Select(x => x.nodes).SelectMany(x => x).Distinct(IEqualityComparerExtensions.Create<VisNode>(x => x.id)).ToDictionary(x => x.id);    
            var result = new Dictionary<string, VisData>();
            objectsDependantOnTechs.ModGroups.ForEach(x => result[x] = CreateDependantDataForModGroup(x, prereqTechNodeLookup, objectsDependantOnTechs, parseTargets));
            return result;
        }
        public VisData CreateRootNotes(ModEntityData<Tech> techsAndDependencies, string imagesPath) {
            var result = new VisData() {ModGroup = StellarisDirectoryHelper.StellarisCoreRootDirectory};
            var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
            var rootNodes = new Dictionary<TechArea, VisNode>();
            var rootNodeCategories = new Dictionary<TechArea, HashSet<string>>();
            foreach (var tech in techsAndDependencies.AllEntities) {
                rootNodeCategories.ComputeIfAbsent(tech.Area, ignored => new HashSet<string>()).AddRange(tech.Categories);
            }
            foreach (var techArea in techAreas) {
                var rootNode = BuildRootNode(techArea, imagesPath);
                rootNode.categories = rootNodeCategories.ComputeIfAbsent(techArea, ignored => new HashSet<string>()).ToArray();
                rootNodes[techArea] = rootNode;
            }
            result.nodes.AddRange(rootNodes.Values);
            return result;
        }
        
        
        public IDictionary<string, VisData> CreateVisData(ModEntityData<Tech> techsAndDependencies, string imagesPath) {

            // perform longest path analysis to find out how many levels we want in each tech
            var maxPathPerTier = new Dictionary<int, int>();
            
            List<Tech> techsWithNoPrereqs = new List<Tech>();
            foreach (var tech in techsAndDependencies.AllEntities) {
                if (!tech.Tier.HasValue) {
                    throw new InvalidOperationException("All Techs must have Tiers to create vis data.  " + tech.Id);
                }
                
                int pathLength = NumberOfPrereqsInSameTier(tech);
                
                var currentMaxForTier = maxPathPerTier.ComputeIfAbsent(tech.Tier.Value, ignored => 0);
                if (pathLength > currentMaxForTier) {
                    maxPathPerTier[tech.Tier.Value] = pathLength;
                }

                // need to link to a supernode to make it look good
                if (!tech.Prerequisites.Any()) {
                    techsWithNoPrereqs.Add(tech);
                }
            }
            Log.Logger.Debug("Max path per tier {@maxPaths} Number of techs with no pre-requiste {noPreqCount}", maxPathPerTier, techsWithNoPrereqs.Count);
            
            
            // determine the base levels in the graph that each node will be on.
            var minimumLevelForTier = CalculateMinimumLevelForTier(maxPathPerTier);
            Log.Logger.Debug("Minimum level per tier {@minLevels", minimumLevelForTier);

            //sort the input by area as it produces a nicer graph.
            var techList = techsAndDependencies.AllEntities.ToList();
            techList.Sort((tech1, tech2) => {
                var primary = tech1.Area - tech2.Area;
                return primary != 0 ? primary : string.Compare(tech1.Id, tech2.Id, StringComparison.Ordinal);
            });
            
            var modGroups = techsAndDependencies.AllLinks.Select(x => x.To.ModGroup).Distinct().ToList();
       
            // link to supernodes
            var results = new Dictionary<string, VisData>();
            foreach (var modGroup in modGroups) {
                var result = new VisData() {
                    nodes = techList.Where(x => Filter(x, modGroup)).Select(x => MarshalTech(x, minimumLevelForTier, imagesPath)).ToList(),
                    edges = techsAndDependencies.AllLinks.Where(x => Filter(x.To, modGroup)).Select(MarshalLink).ToList()
                };

                foreach (var tech in techsWithNoPrereqs.Where(x => Filter(x, modGroup))) {
                    result.edges.Add(BuildRootLink(tech.Area, tech.Id));
                }

                results[modGroup] = result;
            }

            return results;
        }
        
        private VisData CreateDependantDataForModGroup(string modGroup, IDictionary<string, VisNode> prereqTechNodeLookup, ObjectsDependantOnTechs objectsDependantOnTechs, IEnumerable<ParseTarget> parseTargets) {
            var result = new VisData() {
                ModGroup = modGroup,
            };

            foreach (var parseTarget in parseTargets.WithoutTechs()) {

                switch (parseTarget) {
                    case ParseTarget.Buildings:
                        result.nodes.AddRange(objectsDependantOnTechs.Buildings.AllEntitiesForModGroup(modGroup).Select(x => MarshalBuilding(x, prereqTechNodeLookup, outputDirectoryHelper.GetImagesPath(parseTarget.ImagesDirectory()))));
                        result.edges.AddRange(objectsDependantOnTechs.Buildings.AllLinksForModGroup(modGroup).Select(MarshalLink));
                        break;
                    default:
                        throw new Exception(parseTarget.ToString());
                }
            }

            return result;
        }
        
        private bool Filter<T>(T entity, string modGroup) where T : Entity {
            return modGroup == null || entity.ModGroup == modGroup;
        }

        private VisNode BuildRootNode(TechArea area, string imagesPath) {
            var areaName = area.ToString();
            var result = new VisNode
            {
                id = BuildRootNodeName(area),
                label = localisationApi.GetName(areaName.ToLower()),
                group = areaName,
                image = imagesPath + "/" + BuildRootNodeName(area) + ".png",
                hasImage = true,
                level = 0,
                nodeType = "tech"
            };
            return result;
        }

        private VisEdge BuildRootLink(TechArea area, string to) {
            return new VisEdge() {
                @from = BuildRootNodeName(area),
                to = to,
                color = new VisColor() {
                    opacity = 0
                }
            };
        }

        private static string BuildRootNodeName(TechArea techArea) {
            return techArea + "-root";
        }

        private Dictionary<int, int> CalculateMinimumLevelForTier(Dictionary<int, int> maxPathsPerTier) {
            var dictionary = new Dictionary<int, int>();
            var maxTier = maxPathsPerTier.Keys.Max();
            // Level 0 is the supernodes if I have them
            // Tier 0 starts at 1
            dictionary[0] = 1;
            for (int i = 1; i <= maxTier; i++) {
                var previousLevel = i - 1;
                dictionary[i] = 1 + dictionary[previousLevel] + maxPathsPerTier[previousLevel];
            }

            return dictionary;
        }

        private int CalculateLevel(Tech tech, Dictionary<int, int> tierStartingLengths) {
            // number of techs in the same tier that are pre-requistes, so must be on lower levels, this may be 0.
            var levelInTier = NumberOfPrereqsInSameTier(tech);
            return tierStartingLengths[tech.TierValue] + levelInTier;
        }


        private int NumberOfPrereqsInSameTier(Tech tech) {
            int maxPreqs = 0;
            foreach (var techPrerequisite in tech.Prerequisites) {
                if (techPrerequisite.TierValue == tech.TierValue) {
                    var numberOfPrereqsDownThisBranch = 1 + NumberOfPrereqsInSameTier(techPrerequisite);
                    if (numberOfPrereqsDownThisBranch > maxPreqs) {
                        maxPreqs = numberOfPrereqsDownThisBranch;
                    }
                }
            }
            
            return maxPreqs;
        }

        private VisNode MarshalBuilding(Building building, IDictionary<string,VisNode> prereqTechNodeLookup, string imagesPath) {
            var result = CreateNode(building, imagesPath, "building"); 
            result.prerequisites = building.PrerequisiteIds != null
                ? building.PrerequisiteIds.ToArray()
                : new string[] { };
    
            result.title = result.title + "<br/><b>Build Time: </b>" + building.BaseBuildTime;
            result.group = "Building";
            
            if (building.Category != null) {
                result.title = result.title + "<br/><b>Category: </b>" + localisationApi.GetName(building.Category);
            }

            result.title = result.title + AddBuildingResources("Cost", building.Cost);
            result.title = result.title + AddBuildingResources("Upkeep", building.Upkeep);
            result.title = result.title + AddBuildingResources("Produces", building.Produces);

            // find the highest prerequisite tech level and then add 1 to it to ensure it is rendered in a sensible place.
            var highestLevelOfPrerequisiteTechs = building.Prerequisites.Select(x => prereqTechNodeLookup[x.Id].level).Max();
            if (!highestLevelOfPrerequisiteTechs.HasValue) {
                throw new Exception(building.Name + " Had no prerequiste levels: " + building.FilePath);
            }
            result.level = highestLevelOfPrerequisiteTechs + 1;


            return result;
        }

        private string AddBuildingResources<T>(string resourceType, IDictionary<string, T> costs) {
            if (costs.Any()) {
                string costString = "<br/><b>" + resourceType + ":</b>";
                foreach (var (key, value) in costs) {
                    costString = costString + " " + value + " " + localisationApi.GetName(key) + ",";
                }
                
                return costString.Remove(costString.Length - 1);;
            }
            else {
                return "";
            }
        }

        private VisNode MarshalTech(Tech tech, Dictionary<int, int> startingLevelsByTier, string imagesPath)
        {
            var result = CreateNode(tech, imagesPath, "tech");
            result.prerequisites = tech.PrerequisiteIds != null
                ? tech.PrerequisiteIds.ToArray()
                : new[] {BuildRootNodeName(tech.Area)};
            
           
            result.group = (tech.Mod == "Stellaris" ? "" : "Mod") + tech.Area;
            result.title = result.title + "<br/><b>Tier: </b>" + tech.TierValue;

            if (tech.Categories != null)
            {
                var catString = tech.Categories.Count() > 1 ? "Categories" : "Category";
                var categoriesLocalised = tech.Categories.Select(x => localisationApi.GetName(x)).ToArray();

                result.title = result.title + "<br/><b>" + catString + ": </b>" + string.Join(",", categoriesLocalised);

                result.categories = tech.Categories.ToArray();
            }

            result.title = result.title + "<br/><b>Base cost: </b>" + (tech.BaseCost ?? 0);

            // we are assigning levels in the graph, so work out where this tech sits.
            result.level = tech.TierValue == -1 ? 0 : CalculateLevel(tech, startingLevelsByTier);  

            if (tech.Flags.Any())
            {
                result.title = result.title + "<br/><b>Attributes: </b>" + string.Join(", ", tech.Flags);
            }

            // rare purple tech
            if (tech.Flags.Contains(TechFlag.Rare))
            {
                setBorder(result, "#8900CE");
            }

            // starter technology
            if (tech.Flags.Contains(TechFlag.Starter))
            {
                setBorder(result, "#00CE56");
            }

            // may cause endgame crisis or AI revolution
            if (tech.Flags.Contains(TechFlag.Dangerous))
            {
                setBorder(result, "#D30000");
            }
            
            // tech that requires an acquisition - base weight is 0
            if (tech.Flags.Contains(TechFlag.NonTechDependency))
            {
                setBorder(result, "#CE7C00");
            }

            // tech that is repeatable
            if (tech.Flags.Contains(TechFlag.Repeatable))
            {
                setBorder(result, "#0078CE");
            }
            return result;
        }
        
        private VisNode CreateNode(Entity entity, string imagesPath, string nodeType) {
            var result = new VisNode {
                id = entity.Id,
                label = entity.Name,
                title = "<b>" + entity.Name + "</b>",
                image = imagesPath + "/" + entity.Id + ".png",
                hasImage = entity.IconFound,
                nodeType = nodeType
            };
            result.title = result.title + "<br/><i>" + entity.Description + "</i>";

            if (entity.Mod != "Stellaris") {
                result.title = result.title + "<br/><b>Mod: </b>" + entity.Mod + "";
            } 
            
            if (entity.DLC != null) {
                result.title = result.title + "<br/><i>Requires the " + entity.DLC + " DLC</i>";
            }
            

            return result;
        }
        
        private VisEdge MarshalLink(Link node)
        {
            return new VisEdge
            {
                @from = node.From.Id,
                to = node.To.Id,
                arrows = "to",
                dashes = true,
                color = new VisColor
                {
                    color = "grey"
                }
            };
        }

        private void setBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 1;
        }
    }
}