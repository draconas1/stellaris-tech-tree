using System;
using System.Collections.Generic;
using System.Linq;
using TechTreeCreator.DTO;

namespace TechTreeCreator.Output.Vis {
    public static class VisHelpers {
        public static void SetBorder(VisNode node, string borderColour)
        {
            node.color = new VisColor { border = borderColour };
            node.borderWidth = 1;
        }
        
        public static  VisNode CreateNode(Entity entity, string imagesRelativePath, string nodeType) {
            var result = new VisNode {
                id = entity.Id,
                label = entity.Name,
                title = $"<b>{entity.Name}</b> ({entity.Id})",
                image = $"{imagesRelativePath}/{entity.Id}.png",
                hasImage = entity.IconFound,
                nodeType = nodeType
            };
            result.title = result.title + $"<br/><i>{entity.Description}</i>";

            if (entity.Mod != "Stellaris") {
                result.title = result.title + $"<br/><b>Mod: </b>{entity.Mod}";
            } 
            
            if (entity.DLC != null) {
                result.title = result.title + $"<br/><i>Requires the {entity.DLC} DLC</i>";
            }
            

            return result;
        }

        public static void SetLevel(VisNode node, Entity entity, IDictionary<string, VisNode> prereqTechNodeLookup) {
            // find the highest prerequisite tech level and then add 1 to it to ensure it is rendered in a sensible place.
            var highestLevelOfPrerequisiteTechs = entity.Prerequisites.Select(x => prereqTechNodeLookup[x.Id].level).Max();
            if (!highestLevelOfPrerequisiteTechs.HasValue) {
                throw new Exception(entity.Name + " Had no prerequiste levels: " + entity.FilePath);
            }

            node.level = highestLevelOfPrerequisiteTechs + 1;
        }

        public static string CreateRelativePath(string fullPath, string relativeTo) {
            var relativePath = fullPath.Replace(relativeTo, "");
            if (relativePath.StartsWith("/")) {
                relativePath = relativePath.Remove(0, 1);
            }

            return relativePath;
        }

        public static VisEdge MarshalLink(Link node)
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
    }
}