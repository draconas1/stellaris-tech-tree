using CWTools.Parser;
using CWTools.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CWTools.Process;
using TechTree.DTO;
using static CWTools.Localisation.STLLocalisation;
using CWTools.Localisation;
using CWTools.Common;
using TechTree.Extensions;
using System.Linq;
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

        //the minimum cost to be on a specific level of the graph
        private readonly int[] techLevelCosts;
        // in vanilla tier costs generally go up, however free floating costs or a mod could adjust those so a tierX tech is the same price as a tierX-1 tech.
        // so this lookup sets the mimum level a tech can be at based on its tier.
        private readonly Dictionary<int, int> techLevelLookupStartByTier;
       

        public TechTreeParser(ILocalisationAPI localisationAPI, Dictionary<string, string> scriptedVariables, string rootTechDir)
        {
            this.localisationAPI = localisationAPI;
            this.scriptedVariables = scriptedVariables;
            this.rootTechDir = rootTechDir;
            IgnoreFiles = new List<string>();
            ParseFileMask = "*.txt";
            Areas = new HashSet<string>();
            Categories = new HashSet<string>();

            // build the level list using the tech tier prices from scripted variables.
            Dictionary<int, int> startIndex = new Dictionary<int, int>();
            startIndex[0] = 0;
            List<int> levels = new List<int>();
            levels.Add(0);
            levels.Add(500);
            for(int tier = 1; tier <= 6; tier++)
            {
                for (int cost = 1; cost <= 5; cost++)
                {
                    var key = "@tier" + tier + "cost" + cost;
                    if (scriptedVariables.ContainsKey(key))
                    {
                        if (!startIndex.ContainsKey(tier))
                        {
                            startIndex[tier] = levels.Count();
                        }
                        levels.Add(Int32.Parse(scriptedVariables[key]));
                    }
                }
            }
            // catch repeatable techs
            levels.Add(levels.Last() + 1000);
            // another level for much higher costing things
            levels.Add(levels.Last() + 10000);
            techLevelLookupStartByTier = startIndex;
            techLevelCosts = levels.ToArray();
        }

        public VisData ParseTechFiles()
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(rootTechDir, ParseFileMask, IgnoreFiles);
            var parsedTechFiles = new CWParserHelper().ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());
            var result = new VisData();
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

                    if (process)
                    {
                        result.nodes.Add(ProcessNode(node));
                        result.edges.AddRange(ProcessNodeLinks(node));
                    }
                }
            }
            
            var nodeIds = result.nodes.ToDictionary(x => x.id);

            // remove edges that are missing ends.  - TODO: make method with logging
            result.edges.RemoveAll(x => !(nodeIds.ContainsKey(x.from) || nodeIds.ContainsKey(x.to)));

        
            // find edges with no parent
            var edgeTos = result.edges.Select(x => x.to);
            foreach(string nodeId in edgeTos)
            {
                nodeIds.Remove(nodeId);
            }

            // create edges from the root node to everything that doesn't have a parent
//            foreach (var area in new String[]{"physics", "society", "engineering"}) {
//                result.nodes.Add(new VisNode
//                {
//                    id = area + "_root",
//                    label = "Root",
//                    group = area
//                });
//            }
//            
//            foreach (var node in nodeIds)
//            {
//                result.edges.Add(new VisEdge
//                {
//                    from = node.Value.group + "_root",
//                    to = node.Key,
//                    color = new VisColor
//                    {
//                        opacity = 0
//                    }
//                });
//            }

            return result;
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

        public VisNode ProcessNode(CWNode node)
        {
            var result = new VisNode
            {
                id = node.Key,
                label = localisationAPI.GetName(node.Key),
                title = "<i>" + localisationAPI.GetDescription(node.Key) + "</i>",
                group = node.GetKeyValue("area")
            };

            var tier = int.Parse(node.GetKeyValue("tier", scriptedVariables) ?? "0");
            result.title = result.title + "<br/><b>Tier: </b>" + tier;

            if (node.GetNode("category") != null)
            {
                var categories = node.GetNode("category").Values;
                var catString = categories.Count > 1 ? "Categories" : "Category";
                var categoriesLocalised = categories.Select(x => localisationAPI.GetName(x)).ToArray();

                result.title = result.title + "<br/><b>" + catString + ": </b>" + string.Join(",", categoriesLocalised);
            }


            // get the cost, this may be a variable.
            // eugh, probably need to add scripted variables from the files themselves for working with mods.
            string coststr = node.GetKeyValue("cost", scriptedVariables) ?? "0";
            int cost = int.Parse(coststr);
            result.title = result.title + "<br/><b>Base cost: </b>" + cost;

            // find what level of the graph its meant to be on.  
            // find the minimum level first based on its tier
            var startindex = techLevelLookupStartByTier[tier];
            result.level = startindex;
            // no go through the remaining levels and keep bumping our level up if we make the cut.
            for (int i = startindex; i< techLevelCosts.Length; i++)
            {
                if (cost >= techLevelCosts[i])
                {
                    result.level = i;
                }
                else
                {
                    break;
                }
            }
            

            // rare purple tech
            if ("yes".Equals(node.GetKeyValue("is_rare"), StringComparison.InvariantCultureIgnoreCase))
            {

                setBorder(result, "#8900CE");
            }

            // starter technology
            if ("yes".Equals(node.GetKeyValue("start_tech"), StringComparison.InvariantCultureIgnoreCase))
            {
                setBorder(result, "#00CE56");
                result.level = 0; //start tech is always level 0
            }

            // may cause endgame crisis or AI revolution
            if ("yes".Equals(node.GetKeyValue("is_dangerous"), StringComparison.InvariantCultureIgnoreCase))
            {
                setBorder(result, "#D30000");
            }
            
            // tech that requires an acquisition - base weight is 0
            if ("0".Equals(node.GetKeyValue("weight"), StringComparison.InvariantCultureIgnoreCase))
            {
                setBorder(result, "#CE7C00");
            }

            // tech that is repeatable
            if (node.GetKeyValue("cost_per_level") != null)
            {
                setBorder(result, "#0078CE");
            }
            return result;
        }

        private void setBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 2;
        }

        public IEnumerable<VisEdge> ProcessNodeLinks(CWNode node)
        {
            var result = new List<VisEdge>();
            if (node.GetNode("prerequisites") != null)
            {
                foreach (string prereq in node.GetNode("prerequisites").Values)
                {
                    result.Add(new VisEdge
                    {
                        from = prereq,
                        to = node.Key,
                        arrows = "to"
                    });
                }
            }
            return result;
        }
    }
}
