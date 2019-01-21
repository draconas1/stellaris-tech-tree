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
        private readonly ILocalisationAPI localisationAPI;
        private readonly string rootTechDir;

        public TechTreeParser(ILocalisationAPI localisationAPI, string rootTechDir)
        {
            this.localisationAPI = localisationAPI;
            this.rootTechDir = rootTechDir;
            IgnoreFiles = new List<string>();
            ParseFileMask = "*.txt";
        }

        public VisData ParseTechFiles()
        {
            var techFiles = DirectoryWalker.FindFilesInDirectoryTree(rootTechDir, ParseFileMask, IgnoreFiles);
            var parsedTechFiles = new CWParserHelper().ParseParadoxFile(techFiles.Select(x => x.FullName).ToList());
            var result = new VisData();
            foreach(var file in parsedTechFiles)
            {
                // top level nodes are files, so we process the immiedate children of each file, which is the individual techs.
                foreach (var node in file.Nodes)
                {
                    if (node.GetKeyValue("area") == "society")
                    {
                       
                    }
                    result.nodes.Add(ProcessNode(node));
                    result.edges.AddRange(ProcessNodeLinks(node));
                }
            }
            return result;
        }


        public VisNode ProcessNode(CWNode node)
        {
            var result = new VisNode
            {
                id = node.Key,
                label = localisationAPI.GetName(node.Key),
                title = localisationAPI.GetDescription(node.Key),
                group = node.GetKeyValue("area")
            };

            // rare purple tech
            if ("yes".Equals(node.GetKeyValue("is_rare"), StringComparison.InvariantCultureIgnoreCase))
            {

                setBorder(result, "#8900CE");
            }

            // starter technology
            if ("yes".Equals(node.GetKeyValue("start_tech"), StringComparison.InvariantCultureIgnoreCase))
            {
                setBorder(result, "#00CE56");
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
