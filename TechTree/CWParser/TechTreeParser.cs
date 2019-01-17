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

namespace TechTree.CWParser
{
    class TechTreeParser
    {
        private readonly ILocalisationAPI localisationAPI;

        public TechTreeParser(ILocalisationAPI localisationAPI)
        {
            this.localisationAPI = localisationAPI;
        }

        public VisData ParseTechFiles(IEnumerable<FileInfo> files)
        {
            VisData result = new VisData();

            foreach (FileInfo paradoxFile in files)
            {
                // raw parsing
                var parsed = CKParser.parseEventFile(paradoxFile.FullName);

                // this is an extension method in CWTools.CSharp
                var eventFile = parsed.GetResult();

                //"Process" result into nicer format
                var processed = CK2Process.processEventFile(eventFile);

                // marshall this into a more c# fieldy type using the CWTools example
                CWNode tech = CWParsedFileMapper.ToMyNode(processed);

                // the nodes here are all tech items
                foreach (CWNode node in tech.Nodes)
                {
                    // key values
                    result.nodes.Add(ProcessNode(node));
                    result.edges.AddRange(ProcessNodeLinks(node));
                }
            }

            return result;
        }

        public VisNode ProcessNode(CWNode node)
        {
            return new VisNode
            {
                id = node.Key,
                label = localisationAPI.GetName(node.Key),
                title = localisationAPI.GetDescription(node.Key),
                group = node.GetKeyValue("area")
            };
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
