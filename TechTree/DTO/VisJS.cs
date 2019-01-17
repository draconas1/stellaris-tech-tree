using System;
using System.Collections.Generic;
using System.Text;

namespace TechTree.DTO
{
#pragma warning disable IDE1006 // Naming Styles - must match json properties
    class VisData
    {
        public List<VisNode> nodes { get; set; }
        public List<VisEdge> edges { get; set; }

        public VisData()
        {
            nodes = new List<VisNode>();
            edges = new List<VisEdge>();
        }
    }

    class VisNode
    {
        // {id: 'soc-Hydroponics', label: 'Hydroponics', group: 'society', title: 'Large-scale hydroponics farms producing nutrient-rich produce helps sustain a growing population while minimizing negative environmental impact.'},

        public string id { get; set; }

        public string label { get; set; }
        public string group { get; set; }
        public string title { get; set; }
    }

    class VisEdge
    {
        //{from: 'soc-Hydroponics', to: 'soc-Eco Simulatio', arrows: 'to'}

        public string from { get; set; }
        public string to { get; set; }
        public string arrows { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
