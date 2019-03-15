using System;
using System.Collections.Generic;
using System.Linq;
using CWToolsHelpers.Localisation;
using NetExtensions.Collection;
using TechTreeCreator.DTO;

namespace TechTreeCreator.Output {
    public class VisDataMarshaler {
        private readonly ILocalisationApiHelper localisationApi;

        public VisDataMarshaler(ILocalisationApiHelper localisationApi)
        {
            this.localisationApi = localisationApi;
        }

        public VisData CreateVisData(VisData techData, ObjectsDependantOnTechs objectsDependantOnTechs, string imagesPath) {
            Dictionary<string,VisNode> nodeLookup = techData.nodes.ToDictionary(x => x.id);

            var result = new VisData() {
                nodes = objectsDependantOnTechs.Buildings.Values.Select(x => MarshalBuilding(x, nodeLookup, imagesPath)).ToList(),
                edges = objectsDependantOnTechs.Prerequisites.Select(MarshalLink).ToList()
            };

            return result;

        }
        
        public VisData CreateVisData(TechsAndDependencies techsAndDependencies, string imagesPath) {

            // perform longest path analysis to find out how many levels we want in each tech
            var maxPathPerTier = new Dictionary<int, int>();
            
            List<Tech> techsWithNoPrereqs = new List<Tech>();
            foreach (var tech in techsAndDependencies.Techs.Values) {
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
            
            // determine the base levels in the graph that each node will be on.
            var minimumLevelForTier = CalculateMinimumLevelForTier(maxPathPerTier);

            //sort the input by area as it produces a nicer graph.
            var techList = techsAndDependencies.Techs.Values.ToList();
            techList.Sort((tech1, tech2) => {
                var primary = tech1.Area - tech2.Area;
                return primary != 0 ? primary : string.Compare(tech1.Id, tech2.Id, StringComparison.Ordinal);
            });
            var result = new VisData() {
                nodes = techList.Select(x => MarshalTech(x, minimumLevelForTier, imagesPath)).ToList(),
                edges = techsAndDependencies.Prerequisites.Select(MarshalLink).ToList()
            };
            
            // add supernodes
            var techAreas = Enum.GetValues(typeof(TechArea)).Cast<TechArea>();
            var rootNodes = new Dictionary<TechArea, VisNode>();
            foreach (var techArea in techAreas) {
                var rootNode = BuildRootNode(techArea, imagesPath);
                result.nodes.Add(rootNode);
                rootNodes[techArea] = rootNode;
            }
            
            // link to supernodes
            var rootNodeCategories = new Dictionary<TechArea, HashSet<string>>();
            foreach (var tech in techsWithNoPrereqs) {
                result.edges.Add(BuildRootLink(tech.Area, tech.Id));
                rootNodeCategories.ComputeIfAbsent(tech.Area, ignored => new HashSet<string>()).AddRange(tech.Categories);
            }

            foreach (var (key, value) in rootNodes) {
                value.categories = rootNodeCategories.ComputeIfAbsent(key, ignored => new HashSet<string>()).ToArray();
            }

            return result;
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

        private VisNode MarshalBuilding(Building building, Dictionary<string,VisNode> nodeLookup, string imagesPath) {
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
            var highestLevelOfPrerequisiteTechs = building.Prerequisites.Select(x => nodeLookup[x.Id].level).Max();
            result.level = highestLevelOfPrerequisiteTechs + 1;


            return result;
        }

        private string AddBuildingResources(string resourceType, IDictionary<string, int> costs) {
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
            result.group = tech.Area.ToString();
          
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