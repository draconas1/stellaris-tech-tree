using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TechTree.DTO
{
#pragma warning disable IDE1006 // Naming Styles - must match json properties
    public class VisData
    {
        public List<VisNode> nodes { get; set; }
        public List<VisEdge> edges { get; set; }

        public VisData()
        {
            nodes = new List<VisNode>();
            edges = new List<VisEdge>();
        }

        /// <summary>
        /// Output JSON to files for loading into graph
        /// </summary>
        /// <param name="toDir">Directory where the files will be written</param>
        /// <param name="edgesFile">Name of the edges file, defaults to "edges.json"</param>
        /// <param name="nodesFile">Name of the nodes file, defaults to "nodes.json"</param>
        public void WriteVisData(string toDir, string nodesFile = "nodes.json", string edgesFile = "edges.json")
        {

            JsonSerializer serializer = getSerializer();

            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(Path.Combine(toDir, nodesFile))))
            {
                serializer.Serialize(writer, nodes);
            }
            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(Path.Combine(toDir, edgesFile))))
            {
                serializer.Serialize(writer, edges);
            }
        }

        /// <summary>
        /// Writes a single file that is a JS object of the data json.  The file will have a single javascript variable called GraphData that will be a single string of this VisData object.  (template so requires an Es6 complient browser.  
        /// This can then be loaded into html using {script} tags and accessed using 
        ///  var rawData = JSON.parse(GraphData);
        /// var nodeJson = new vis.DataSet(rawData.nodes);
        /// var edgeJson = new vis.DataSet(rawData.edges);
        /// </summary>
        /// <param name="toDir"></param>
        /// <param name="fileName"></param>
        public void WriteVisDataToOneJSFile(string toDir, string fileName = "graph.json")
        {
            JsonSerializer serializer = getSerializer();

            var writer = new StringWriter();
            writer.Write("GraphData = ");

            serializer.Serialize(new JsonTextWriter(writer), this);
            
            using (var streamWriter = new StreamWriter(Path.Combine(toDir, fileName)))
            {
                streamWriter.WriteLine(writer.ToString());
            }
        }

        private static JsonSerializer getSerializer()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            serializer.NullValueHandling = NullValueHandling.Ignore;
            return serializer;
        }
    }

    public class VisNode
    {
        // {id: 'soc-Hydroponics', label: 'Hydroponics', group: 'society', title: 'Large-scale hydroponics farms producing nutrient-rich produce helps sustain a growing population while minimizing negative environmental impact.'},

        public string id { get; set; }

        public string label { get; set; }
        public string group { get; set; }
        public string title { get; set; }
        public VisColor color { get; set; }
        public int? borderWidth { get; set; }
        public int? level { get; set; }
    }

    public class VisColor
    {
        public string border { get; set; }
        public string color { get; set; }

        public string background { get; set; }

        public float? opacity { get; set; }
    }

    public class VisEdge
    {
        //{from: 'soc-Hydroponics', to: 'soc-Eco Simulatio', arrows: 'to'}

        public string from { get; set; }
        public string to { get; set; }
        public string arrows { get; set; }
        public VisColor color { get; set; }
        public bool dashes { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
