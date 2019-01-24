using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CWTools.Localisation;
using SQLitePCL;
using TechTree.DTO;
using TechTree.Extensions;

namespace TechTree.Output {
    public class VisDataMarshaler {
        private readonly ILocalisationAPI localisationAPI;
        private readonly Dictionary<string, string> scriptedVariables;

        public VisDataMarshaler(ILocalisationAPI localisationAPI, Dictionary<string, string> scriptedVariables)
        {
            this.localisationAPI = localisationAPI;
            this.scriptedVariables = scriptedVariables;
        }
        
        public VisData CreateVisData(TechsAndDependencies techsAndDependencies) {

            // perform longest path analysis to find out how many levels we want in each tech
            var maxPathPerTier = new Dictionary<int, int>();
            foreach (var tech in techsAndDependencies.Techs.Values) {
                int pathLength = NumberOfPrereqsInSameTier(tech);
                if (!tech.Tier.HasValue) {
                    throw new InvalidOperationException("All Techs must have Tiers to create vis data.  " + tech.Id);
                }
                var currentMaxForTier = maxPathPerTier.ComputeIfAbsent(tech.Tier.Value, ignored => 0);
                if (pathLength > currentMaxForTier) {
                    maxPathPerTier[tech.Tier.Value] = pathLength;
                }
            }
            
            // determine the base levels in the graph that each node will be on.
            var minimumLevelForTier = CalculateMinimumLevelForTier(maxPathPerTier);

            return new VisData() {
                nodes = techsAndDependencies.Techs.Values.Select(x => MarshalTech(x, minimumLevelForTier)).ToList(),
                edges = techsAndDependencies.Prerequisites.Select(MarshalLink).ToList()
            };
        }

        private Dictionary<int, int> CalculateMinimumLevelForTier(Dictionary<int, int> maxPathsPerTier) {
            var dictionary = new Dictionary<int, int>();
            var maxTier = maxPathsPerTier.Keys.Max();
            // Level 0 is the supernodes if I have them
            // Tier 0 starts at 1
            dictionary[0] = 1;
            for (int i = 1; i <= maxTier; i++) {
                var previousLevel = i - 1;
                dictionary[i] = dictionary[previousLevel] + maxPathsPerTier[previousLevel];
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
                if (techPrerequisite.Tier == tech.Tier) {
                    var numberOfPrereqsDownThisBranch = 1 + NumberOfPrereqsInSameTier(techPrerequisite);
                    if (numberOfPrereqsDownThisBranch > maxPreqs) {
                        maxPreqs = numberOfPrereqsDownThisBranch;
                    }
                }
            }
            
            return maxPreqs;
        }
        
        
        private VisNode MarshalTech(Tech tech,  Dictionary<int, int> startingLevelsByTier)
        {
            var result = new VisNode
            {
                id = tech.Id,
                label = tech.Name,
                title = "<i>" + tech.Description + "</i>",
                group = tech.Area.ToString()
            };

            result.title = result.title + "<br/><b>Tier: </b>" + tech.TierValue;

            if (tech.Categories != null)
            {
                var catString = tech.Categories.Count() > 1 ? "Categories" : "Category";
                var categoriesLocalised = tech.Categories.Select(x => localisationAPI.GetName(x)).ToArray();

                result.title = result.title + "<br/><b>" + catString + ": </b>" + string.Join(",", categoriesLocalised);
            }


            // get the cost, this may be a variable.
            // eugh, probably need to add scripted variables from the files themselves for working with mods.
            result.title = result.title + "<br/><b>Base cost: </b>" + (tech.BaseCost ?? 0);

            result.level = CalculateLevel(tech, startingLevelsByTier);  

          
            // rare purple tech
            if (tech.Flags.Contains(TechFlag.Rare))
            {
                setBorder(result, "#8900CE");
            }

            // starter technology
            if (tech.Flags.Contains(TechFlag.Starter))
            {
                setBorder(result, "#00CE56");
                result.level = 0; //start tech is always level 0
            }

            // may cause endgame crisis or AI revolution
            if (tech.Flags.Contains(TechFlag.Dangerous))
            {
                setBorder(result, "#D30000");
            }
            
            // tech that requires an acquisition - base weight is 0
            if (tech.Flags.Contains(TechFlag.RequiresAcquisition))
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
        
        private VisEdge MarshalLink(Link node)
        {
            return new VisEdge
            {
                from = node.From.Id,
                to = node.To.Id,
                arrows = "to"
            };
        }

        private void setBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 2;
        }

        

    }
}